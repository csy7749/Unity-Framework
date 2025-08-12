using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
using WeChatWASM;
#endif

namespace UnityFramework
{
    /// <summary>
    /// 资源管理模块（YooAsset 封装）。
    /// - 负责包裹初始化、清单更新、资源加载/实例化与回收、下载器创建等。
    /// - 内部对 AssetInfo、加载中 Key、对象池等做了缓存与保护。
    /// </summary>
    internal sealed partial class ResourceModule : Module, IResourceModule
    {
        /// <summary>默认资源包名称。</summary>
        public string DefaultPackageName { get; set; } = "DefaultPackage";

        /// <summary>
        /// 资源系统运行模式（编辑器/单机/联机/WebGL）。
        /// </summary>
        public EPlayMode PlayMode { get; set; } = EPlayMode.OfflinePlayMode;

        /// <summary>
        /// 资源加密类型（None/偏移/流）。
        /// </summary>
        public EncryptionType EncryptionType { get; set; } = EncryptionType.None;

        /// <summary>
        /// 异步系统每帧最大时间切片（毫秒）。
        /// </summary>
        public long Milliseconds { get; set; } = 30;
        
        public override int Priority => 4;

        public override void OnInit() { }
        public override void Shutdown() { }

        /// <summary>
        /// 资源服务器主地址。
        /// </summary>
        public string HostServerURL { get; set; }
        /// <summary>
        /// 资源服务器备用地址。
        /// </summary>
        public string FallbackHostServerURL { get; set; }

        private string _applicableGameVersion;
        private int _internalResourceVersion;

        /// <summary>
        /// 当前资源适用的游戏版本号（默认取 Application.version）。
        /// </summary>
        public string ApplicableGameVersion => _applicableGameVersion;
        /// <summary>
        /// 当前内部资源版本号（可按需从 Manifest 扩展读取）。
        /// </summary>
        public int InternalResourceVersion => _internalResourceVersion;

        /// <summary>
        /// 当前最新包裹版本号（清单更新成功后写入）
        /// </summary>
        public string PackageVersion { set; get; }

        /// <summary>
        /// 同时下载最大数。
        /// </summary>
        public int DownloadingMaxNum { get; set; }
        /// <summary>
        /// 下载失败允许的最大重试次数。
        /// </summary>
        public int FailedTryAgain { get; set; }
        /// <summary>
        /// 是否允许「边玩边下」。为 false 时，远端资源会判定为 NotReady。
        /// </summary>
        public bool UpdatableWhilePlaying { get; set; }

        #region internal state

        /// <summary>
        /// 默认资源包引用。
        /// </summary>
        internal ResourcePackage DefaultPackage { private set; get; }

        /// <summary>
        /// 资源包字典（包名 -> 包实例）。
        /// </summary>
        private Dictionary<string, ResourcePackage> PackageMap { get; } = new Dictionary<string, ResourcePackage>();

        /// <summary>
        /// AssetInfo 缓存（key: package/name 或 name）。
        /// 清单更新成功后会清空。
        /// </summary>
        private readonly Dictionary<string, AssetInfo> _assetInfoMap = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// 正在加载中的 Key 列表（避免同资源并发创建句柄，提供等待机制）。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();

        #endregion

        /// <summary>
        /// 初始化 YooAssets（设置日志、时间片、默认包）以及对象池。
        /// </summary>
        public void Initialize()
        {
            YooAssets.Initialize(new ResourceLogger());
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);

            // 创建并设置默认包
            string packageName = DefaultPackageName;
            var defaultPackage = YooAssets.TryGetPackage(packageName);
            if (defaultPackage == null)
            {
                defaultPackage = YooAssets.CreatePackage(packageName);
                YooAssets.SetDefaultPackage(defaultPackage);
            }
            DefaultPackage = defaultPackage;

            // 对象池模块
            IObjectPoolModule objectPoolManager = ModuleSystem.GetModule<IObjectPoolModule>();
            SetObjectPoolModule(objectPoolManager);

            // 初始版本信息
            _applicableGameVersion = Application.version;
            _internalResourceVersion = 0;
        }

        /// <summary>
        /// 初始化指定包裹（根据 PlayMode 选择不同的参数）。
        /// </summary>
        public async UniTask<InitializationOperation> InitPackage(string packageName)
        {
#if UNITY_EDITOR
            EPlayMode playMode = (EPlayMode)UnityEditor.EditorPrefs.GetInt("EditorPlayMode");
            Log.Warning($"Editor Module Used :{playMode}");
#else
            EPlayMode playMode = (EPlayMode)PlayMode;
#endif
            // 防止重复初始化
            if (PackageMap.TryGetValue(packageName, out var existed))
            {
                if (existed.InitializeStatus is EOperationStatus.Processing or EOperationStatus.Succeed)
                {
                    Log.Error($"ResourceSystem has already init package : {packageName}");
                    return null;
                }
                PackageMap.Remove(packageName);
            }

            // 创建包裹
            var package = YooAssets.TryGetPackage(packageName) ?? YooAssets.CreatePackage(packageName);
            PackageMap[packageName] = package;

            InitializationOperation initOp = null;

            // 编辑器模拟模式
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(packageName);
                var createParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory)
                };
                initOp = package.InitializeAsync(createParameters);
            }

            // 解密服务（Offline/Host）
            IDecryptionServices dec = CreateDecryptionServices();

            // 单机模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var p = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(dec)
                };
                initOp = package.InitializeAsync(p);
            }

            // 联机（缓存）模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                IRemoteServices remote = new RemoteServices(HostServerURL, FallbackHostServerURL);
                var p = new HostPlayModeParameters
                {
                    BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(dec),
                    CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remote, dec)
                };
                initOp = package.InitializeAsync(p);
            }

            // WebGL 模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                IRemoteServices remote = new RemoteServices(HostServerURL, FallbackHostServerURL);
                IWebDecryptionServices wdec = CreateWebDecryptionServices();
                var p = new WebPlayModeParameters();
#if UNITY_WEBGL && WEIXINMINIGAME && !UNITY_EDITOR
                Log.Info("=======================WEIXINMINIGAME=======================");
                string packageRoot = $"{WeChatWASM.WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";
                p.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remote, wdec);
#else
                Log.Info("=======================UNITY_WEBGL=======================");
                p.WebRemoteFileSystemParameters = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remote, wdec);
                p.WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(wdec);
#endif
                initOp = package.InitializeAsync(p);
            }

            // 等待初始化完成
            await initOp.ToUniTask();

            // 尽力记录版本信息
            try
            {
                PackageVersion = package.GetPackageVersion();
                _applicableGameVersion = Application.version;
                _internalResourceVersion = 0;
            }
            catch (Exception e)
            {
                Log.Warning($"Get package version failed: {e}");
            }

            Log.Info($"Init resource package status : {initOp?.Status}, version: {PackageVersion}");
            return initOp;
        }

        /// <summary>
        /// 创建本地/联机的解密服务。
        /// </summary>
        private IDecryptionServices CreateDecryptionServices() =>
            EncryptionType switch
            {
                EncryptionType.FileOffSet => new FileOffsetDecryption(),
                EncryptionType.FileStream => new FileStreamDecryption(),
                _ => null
            };

        /// <summary>
        /// 创建 WebGL 的解密服务。
        /// </summary>
        private IWebDecryptionServices CreateWebDecryptionServices() =>
            EncryptionType switch
            {
                EncryptionType.FileOffSet => new FileOffsetWebDecryption(),
                EncryptionType.FileStream => new FileStreamWebDecryption(),
                _ => null
            };

        /// <summary>
        /// 获取当前包裹版本（从 YooAssets 包对象读取）。
        /// </summary>
        public string GetPackageVersion(string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package == null ? string.Empty : package.GetPackageVersion();
        }

        /// <summary>
        /// 异步请求远端包裹最新版本（仅 Host/Web 模式有效）。
        /// </summary>
        public RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = false, int timeout = 60, string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package.RequestPackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 设置远端服务地址。
        /// </summary>
        public void SetRemoteServicesUrl(string defaultHostServer, string fallbackHostServer)
        {
            HostServerURL = defaultHostServer;
            FallbackHostServerURL = fallbackHostServer;
        }

        /// <summary>
        /// 向远端请求并更新清单。
        /// 成功后：
        /// - 清空 AssetInfo 缓存，避免使用旧信息；
        /// - 写入 PackageVersion；
        /// 失败仅做 Warning。
        /// </summary>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60, string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);

            var op = package.UpdatePackageManifestAsync(packageVersion, timeout);
            op.Completed += _ =>
            {
                var status = op.Status;
                if (status == EOperationStatus.Succeed)
                {
                    _assetInfoMap.Clear();
                    PackageVersion = packageVersion;
                    Log.Info($"Update manifest succeed. PackageVersion = {PackageVersion}");
                }
                else
                {
                    Log.Warning($"Update manifest failed: {op.Error}");
                }
            };
            return op;
        }

        /// <summary>
        /// 资源下载器实例（可供 UI 展示进度）。
        /// </summary>
        public ResourceDownloaderOperation Downloader { get; set; }

        /// <summary>
        /// 创建资源下载器（下载当前版本所有需要的资源包文件）。
        /// 受 DownloadingMaxNum / FailedTryAgain 影响。
        /// </summary>
        public ResourceDownloaderOperation CreateResourceDownloader(string customPackageName = "")
        {
            ResourcePackage package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);

            Downloader = package.CreateResourceDownloader(DownloadingMaxNum, FailedTryAgain);
            return Downloader;
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件（传入的 clearMode 会被正确转发）。
        /// </summary>
        public ClearCacheFilesOperation ClearCacheFilesAsync(EFileClearMode clearMode = EFileClearMode.ClearUnusedBundleFiles, string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            return package.ClearCacheFilesAsync(clearMode);
        }

        /// <summary>
        /// 清理沙盒路径（删除所有缓存的 bundle 文件）。
        /// </summary>
        public void ClearAllBundleFiles(string customPackageName = "")
        {
            var package = string.IsNullOrEmpty(customPackageName)
                ? YooAssets.GetPackage(DefaultPackageName)
                : YooAssets.GetPackage(customPackageName);
            package.ClearCacheFilesAsync(EFileClearMode.ClearAllBundleFiles);
        }

        #region 资源回收

        /// <summary>
        /// 低内存回调（由 Driver 注入）；这里转发到外部策略。
        /// </summary>
        public void OnLowMemory()
        {
            Log.Warning("Low memory reported...");
            _forceUnloadUnusedAssetsAction?.Invoke(true);
        }

        private Action<bool> _forceUnloadUnusedAssetsAction;

        /// <summary>
        /// 设置低内存时的强制回收策略（由外部 Driver 提供）。
        /// </summary>
        public void SetForceUnloadUnusedAssetsAction(Action<bool> action) => _forceUnloadUnusedAssetsAction = action;

        /// <summary>
        /// 回收引用计数为 0 的资源：
        /// - 先释放对象池中未使用的实例；
        /// - 再对每个已初始化包裹执行 YooAssets 的 UnloadUnusedAssetsAsync。
        /// </summary>
        public void UnloadUnusedAssets()
        {
            _assetPool.ReleaseAllUnused();
            foreach (var package in PackageMap.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadUnusedAssetsAsync();
                }
            }
        }

        /// <summary>
        /// 强制回收所有资源（WEBGL 不支持）。
        /// </summary>
        public void ForceUnloadAllAssets()
        {
#if UNITY_WEBGL
            Log.Warning($"WebGL not support invoke {nameof(ForceUnloadAllAssets)}");
            return;
#else
            foreach (var package in PackageMap.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadAllAssetsAsync();
                }
            }
#endif
        }

        /// <summary>
        /// 触发强制回收未使用资源（是否附带 GC 由调用方决定）。
        /// </summary>
        public void ForceUnloadUnusedAssets(bool performGCCollect) => _forceUnloadUnusedAssetsAction?.Invoke(performGCCollect);

        #region 资源信息查询

        /// <summary>
        /// 是否需要从远端下载（基于 location）。
        /// </summary>
        public bool IsNeedDownloadFromRemote(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.IsNeedDownloadFromRemote(location);
            var package = YooAssets.GetPackage(packageName);
            return package.IsNeedDownloadFromRemote(location);
        }

        /// <summary>
        /// 是否需要从远端下载（基于 AssetInfo）。
        /// </summary>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.IsNeedDownloadFromRemote(assetInfo);
            var package = YooAssets.GetPackage(packageName);
            return package.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 按单个标签获取 AssetInfo 列表。
        /// </summary>
        public AssetInfo[] GetAssetInfos(string tag, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.GetAssetInfos(tag);
            var package = YooAssets.GetPackage(packageName);
            return package.GetAssetInfos(tag);
        }

        /// <summary>
        /// 按多个标签获取 AssetInfo 列表。
        /// </summary>
        public AssetInfo[] GetAssetInfos(string[] tags, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.GetAssetInfos(tags);
            var package = YooAssets.GetPackage(packageName);
            return package.GetAssetInfos(tags);
        }

        /// <summary>
        /// 获取指定资源的 AssetInfo，并带有简单缓存。
        /// </summary>
        public AssetInfo GetAssetInfo(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            if (string.IsNullOrEmpty(packageName))
            {
                if (_assetInfoMap.TryGetValue(location, out var ai))
                    return ai;
                var assetInfo = YooAssets.GetAssetInfo(location);
                _assetInfoMap[location] = assetInfo;
                return assetInfo;
            }
            else
            {
                string key = $"{packageName}/{location}";
                if (_assetInfoMap.TryGetValue(key, out var ai))
                    return ai;

                var package = YooAssets.GetPackage(packageName)
                              ?? throw new GameFrameworkException($"The package does not exist. Package Name :{packageName}");

                var assetInfo = package.GetAssetInfo(location);
                _assetInfoMap[key] = assetInfo;
                return assetInfo;
            }
        }

        /// <summary>
        /// 判断资源是否存在 / 在线 / 已在本地磁盘。
        /// 注意：此处依赖你的 HasAssetResult 枚举（需包含 AssetOnline/AssetOnDisk/NotExist）。
        /// </summary>
        public HasAssetResult HasAsset(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            if (!CheckLocationValid(location, packageName))
                return HasAssetResult.NotExist;

            var assetInfo = GetAssetInfo(location, packageName);
            if (assetInfo == null)
                return HasAssetResult.NotExist;

            return IsNeedDownloadFromRemote(assetInfo, packageName)
                ? HasAssetResult.AssetOnline
                : HasAssetResult.AssetOnDisk;
        }

        /// <summary>
        /// 校验 Location 是否有效（是否存在于清单）。
        /// </summary>
        public bool CheckLocationValid(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.CheckLocationValid(location);
            var package = YooAssets.GetPackage(packageName);
            return package.CheckLocationValid(location);
        }

        #endregion

        #region 加载核心

        #region 句柄获取（内部封装）

        private AssetHandle GetHandleSync<T>(string location, string packageName = "") where T : UnityEngine.Object =>
            GetHandleSync(location, typeof(T), packageName);

        private AssetHandle GetHandleSync(string location, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.LoadAssetSync(location, assetType);
            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetSync(location, assetType);
        }

        private AssetHandle GetHandleAsync<T>(string location, string packageName = "") where T : UnityEngine.Object =>
            GetHandleAsync(location, typeof(T), packageName);

        private AssetHandle GetHandleAsync(string location, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
                return YooAssets.LoadAssetAsync(location, assetType);
            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync(location, assetType);
        }

        #endregion

        /// <summary>
        /// 获取缓存 Key（默认为 location；多包时为 "package/location"）。
        /// </summary>
        private string GetCacheKey(string location, string packageName = "") =>
            string.IsNullOrEmpty(packageName) || packageName.Equals(DefaultPackageName)
                ? location
                : $"{packageName}/{location}";

        /// <summary>
        /// 同步加载资源并缓存句柄到对象池。
        /// 失败时会释放句柄并返回 null。
        /// </summary>
        public T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            string key = GetCacheKey(location, packageName);
            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
                return pooled.Target as T;

            AssetHandle handle = null;
            try
            {
                handle = GetHandleSync<T>(location, packageName);
                T ret = handle.AssetObject as T;
                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);
                return ret;
            }
            catch (Exception e)
            {
                Log.Warning(nameof(LoadAsset), e);
                handle?.Dispose();
                return null;
            }
        }

        /// <summary>
        /// 同步加载 GameObject 并实例化（实例由 AssetsReference 负责销毁时回收）。
        /// </summary>
        public GameObject LoadGameObject(string location, Transform parent = null, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            string key = GetCacheKey(location, packageName);
            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
                return AssetsReference.Instantiate(pooled.Target as GameObject, parent, this).gameObject;

            AssetHandle handle = null;
            try
            {
                handle = GetHandleSync<GameObject>(location, packageName);
                GameObject go = AssetsReference.Instantiate(handle.AssetObject as GameObject, parent, this).gameObject;
                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);
                return go;
            }
            catch (Exception e)
            {
                Log.Warning(nameof(LoadGameObject), e);
                handle?.Dispose();
                return null;
            }
        }

        /// <summary>
        /// 回调式异步加载（泛型版本）。
        /// - 去重：相同 Key 若正在加载，会等待；
        /// - 完成后注册到对象池，并回调对象；
        /// - 失败回调 null。
        /// </summary>
        public async UniTaskVoid LoadAsset<T>(string location, Action<T> callback, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return;
            }

            string key = GetCacheKey(location, packageName);
            await TryWaitingLoading(key);

            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
            {
                await UniTask.Yield();
                callback?.Invoke(pooled.Target as T);
                return;
            }

            _assetLoadingList.Add(key);
            var handle = GetHandleAsync<T>(location, packageName);

            handle.Completed += assetHandle =>
            {
                try
                {
                    _assetLoadingList.Remove(key);

                    if (assetHandle.Status == EOperationStatus.Succeed && assetHandle.AssetObject != null)
                    {
                        var ao = AssetObject.Create(key, assetHandle.AssetObject, assetHandle, this);
                        _assetPool.Register(ao, true);
                        callback?.Invoke(ao.Target as T);
                    }
                    else
                    {
                        Log.Warning($"LoadAsset Completed with failure: {location}, {assetHandle.LastError}");
                        callback?.Invoke(null);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                    callback?.Invoke(null);
                }
            };
        }

        /// <summary>
        /// 同步加载子资源（暂未实现）。
        /// </summary>
        public TObject[] LoadSubAssetsSync<TObject>(string location, string packageName = "") where TObject : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// 异步加载子资源（暂未实现）。
        /// </summary>
        public UniTask<TObject[]> LoadSubAssetsAsync<TObject>(string location, string packageName = "") where TObject : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// 泛型返回式异步加载。
        /// - 去重等待；
        /// - UpdatableWhilePlaying=false 且需要远端时，立即返回 null；
        /// - 成功注册句柄并返回对象，否则返回 null。
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            string key = GetCacheKey(location, packageName);
            await TryWaitingLoading(key);

            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
            {
                await UniTask.Yield();
                return pooled.Target as T;
            }

            _assetLoadingList.Add(key);
            try
            {
                var assetInfo = GetAssetInfo(location, packageName);
                if (!string.IsNullOrEmpty(assetInfo.Error))
                    return null;

                if (!UpdatableWhilePlaying && IsNeedDownloadFromRemote(assetInfo, packageName))
                    return null;

                var handle = GetHandleAsync<T>(location, packageName);
                try
                {
                    await handle.ToUniTask().AttachExternalCancellation(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }

                if (handle.Status != EOperationStatus.Succeed || handle.AssetObject == null)
                    return null;

                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);
                return handle.AssetObject as T;
            }
            finally
            {
                _assetLoadingList.Remove(key);
            }
        }

        /// <summary>
        /// 返回式异步加载并实例化 GameObject。
        /// - 同上，支持 UpdatableWhilePlaying 判定。
        /// </summary>
        public async UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");

            string key = GetCacheKey(location, packageName);
            await TryWaitingLoading(key);

            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
            {
                await UniTask.Yield();
                return AssetsReference.Instantiate(pooled.Target as GameObject, parent, this).gameObject;
            }

            _assetLoadingList.Add(key);
            try
            {
                var assetInfo = GetAssetInfo(location, packageName);
                if (!string.IsNullOrEmpty(assetInfo.Error))
                    return null;

                if (!UpdatableWhilePlaying && IsNeedDownloadFromRemote(assetInfo, packageName))
                    return null;

                var handle = GetHandleAsync<GameObject>(location, packageName);
                try
                {
                    await handle.ToUniTask().AttachExternalCancellation(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }

                if (handle.Status != EOperationStatus.Succeed || handle.AssetObject == null)
                    return null;

                GameObject go = AssetsReference.Instantiate(handle.AssetObject as GameObject, parent, this).gameObject;

                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);
                return go;
            }
            finally
            {
                _assetLoadingList.Remove(key);
            }
        }

        /// <summary>
        /// 回调式异步加载（带显式 Type）。
        /// - 对 location 有效性、类型兼容性、是否需要远端均做了前置检查；
        /// - 失败通过 LoadResourceStatus 枚举精确回报；
        /// - 进度回调收尾会补 1.0。
        /// </summary>
        public async void LoadAssetAsync(string location, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");
            if (loadAssetCallbacks == null)
                throw new GameFrameworkException("Load asset callbacks is invalid.");

            string key = GetCacheKey(location, packageName);
            await TryWaitingLoading(key);

            float start = Time.time;

            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(location, pooled.Target, Time.time - start, userData);
                return;
            }

            _assetLoadingList.Add(key);
            try
            {
                // 位置有效性
                if (!CheckLocationValid(location, packageName))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist, $"Invalid location : {location}", userData);
                    return;
                }

                // AssetInfo
                var assetInfo = GetAssetInfo(location, packageName);
                if (!string.IsNullOrEmpty(assetInfo.Error))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist,
                        Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location, assetInfo.Error), userData);
                    return;
                }

                // 类型兼容性校验（请求类型必须能赋值自清单类型）
                if (assetType != null && assetInfo.AssetType != null && !assetType.IsAssignableFrom(assetInfo.AssetType))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.TypeError,
                        $"Type mismatch. Request: {assetType}, Asset: {assetInfo.AssetType}", userData);
                    return;
                }

                // 边玩边下策略
                if (!UpdatableWhilePlaying && IsNeedDownloadFromRemote(assetInfo, packageName))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotReady,
                        $"Asset not cached yet : {location}", userData);
                    return;
                }

                var handle = GetHandleAsync(location, assetType, packageName);

                if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
                    InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();

                await handle.ToUniTask();

                if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
                {
                    var status = ClassifyFailure(handle.LastError);
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, status,
                        $"Load failed : {location}. {handle.LastError}", userData);
                    return;
                }

                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);

                loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(location, handle.AssetObject, Time.time - start, userData);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.AssetError, e.Message, userData);
            }
            finally
            {
                _assetLoadingList.Remove(key);
            }
        }

        /// <summary>
        /// 回调式异步加载（类型从 AssetInfo 推断）。
        /// </summary>
        public async void LoadAssetAsync(string location, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
                throw new GameFrameworkException("Asset name is invalid.");
            if (loadAssetCallbacks == null)
                throw new GameFrameworkException("Load asset callbacks is invalid.");

            string key = GetCacheKey(location, packageName);
            await TryWaitingLoading(key);

            float start = Time.time;

            var pooled = _assetPool.Spawn(key);
            if (pooled != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(location, pooled.Target, Time.time - start, userData);
                return;
            }

            _assetLoadingList.Add(key);
            try
            {
                if (!CheckLocationValid(location, packageName))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist, $"Invalid location : {location}", userData);
                    return;
                }

                var assetInfo = GetAssetInfo(location, packageName);
                if (!string.IsNullOrEmpty(assetInfo.Error))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotExist,
                        Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location, assetInfo.Error), userData);
                    return;
                }

                if (!UpdatableWhilePlaying && IsNeedDownloadFromRemote(assetInfo, packageName))
                {
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.NotReady,
                        $"Asset not cached yet : {location}", userData);
                    return;
                }

                var handle = GetHandleAsync(location, assetInfo.AssetType, packageName);

                if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
                    InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();

                await handle.ToUniTask();

                if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
                {
                    var status = ClassifyFailure(handle.LastError);
                    loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, status,
                        $"Load failed : {location}. {handle.LastError}", userData);
                    return;
                }

                var ao = AssetObject.Create(key, handle.AssetObject, handle, this);
                _assetPool.Register(ao, true);

                loadAssetCallbacks.LoadAssetSuccessCallback?.Invoke(location, handle.AssetObject, Time.time - start, userData);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                loadAssetCallbacks.LoadAssetFailureCallback?.Invoke(location, LoadResourceStatus.AssetError, e.Message, userData);
            }
            finally
            {
                _assetLoadingList.Remove(key);
            }
        }

        /// <summary>
        /// 进度回调帮助：在 handle 完成后补一次 1.0，保证 UI 不卡在 0.99。
        /// </summary>
        private async UniTaskVoid InvokeProgress(string location, AssetHandle handle, LoadAssetUpdateCallback cb, object userData)
        {
            if (cb == null) return;

            while (handle is { IsValid: true, IsDone: false })
            {
                await UniTask.Yield();
                cb(location, handle.Progress, userData);
            }
            cb(location, 1f, userData); // 收尾补 1.0
        }

        /// <summary>
        /// 基于错误文案的失败分类（尽力而为，可按项目自定义更精确的解析）。
        /// </summary>
        private LoadResourceStatus ClassifyFailure(string lastError)
        {
            if (string.IsNullOrEmpty(lastError))
                return LoadResourceStatus.AssetError;

            var msg = lastError.ToLowerInvariant();
            if (msg.Contains("depend"))
                return LoadResourceStatus.DependencyError;
            if (msg.Contains("type") || msg.Contains("cast"))
                return LoadResourceStatus.TypeError;
            if (msg.Contains("not found") || msg.Contains("missing") || msg.Contains("invalid location"))
                return LoadResourceStatus.NotExist;
            return LoadResourceStatus.AssetError;
        }

        /// <summary>
        /// 获取同步资源操作句柄（给高级用法/自管生命周期）。
        /// </summary>
        public AssetHandle LoadAssetSyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object =>
            string.IsNullOrEmpty(packageName) ? YooAssets.LoadAssetSync<T>(location) : YooAssets.GetPackage(packageName).LoadAssetSync<T>(location);

        /// <summary>
        /// 获取异步资源操作句柄（给高级用法/自管生命周期）。
        /// </summary>
        public AssetHandle LoadAssetAsyncHandle<T>(string location, string packageName = "") where T : UnityEngine.Object =>
            string.IsNullOrEmpty(packageName) ? YooAssets.LoadAssetAsync<T>(location) : YooAssets.GetPackage(packageName).LoadAssetAsync<T>(location);

        #endregion

        // Editor 下的加载并发等待控制器；超时则打印错误，避免无限等待。
        private readonly TimeoutController _timeoutController = new TimeoutController();

        /// <summary>
        /// 若相同 key 正在加载，等待其完成（Editor 可设置超时；Player 不超时）。
        /// </summary>
        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(() => !_assetLoadingList.Contains(assetObjectKey))
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                        Log.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                }
            }
        }

        #endregion

        #region 下载系统自定义

        // /// <summary>
        // /// 设置下载系统请求委托（可注入自定义 Header、鉴权等）。
        // /// </summary>
        // public void SetDownloadSystemUnityWebRequest(UnityWebRequestDelegate downloadSystemUnityWebRequest) =>
        //     YooAssets.SetDownloadSystemUnityWebRequest(downloadSystemUnityWebRequest);
        //
        // /// <summary>
        // /// 自定义示例请求（演示 Basic 认证；正式项目建议从配置注入）。
        // /// </summary>
        // public UnityEngine.Networking.UnityWebRequest CustomWebRequester(string url)
        // {
        //     var req = new UnityEngine.Networking.UnityWebRequest(url, UnityEngine.Networking.UnityWebRequest.kHttpVerbGET);
        //     // TODO: 替换为外部配置
        //     var authorization = GetAuthorization("Admin", "12345"); 
        //     req.SetRequestHeader("AUTHORIZATION", authorization);
        //     return req;
        // }
        //
        // private string GetAuthorization(string userName, string password)
        // {
        //     string auth = $"{userName}:{password}";
        //     var bytes = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth);
        //     return $"Basic {Convert.ToBase64String(bytes)}";
        // }

        #endregion
    }
}
