using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;

namespace UnityFramework
{
    internal class SceneModule : Module, ISceneModule
    {
        private string _currentMainSceneName = string.Empty;
        private SceneHandle _currentMainScene;
        private readonly Dictionary<string, SceneHandle> _subScenes = new Dictionary<string, SceneHandle>();
        private readonly HashSet<string> _handlingScene = new HashSet<string>();

        public string CurrentMainSceneName => _currentMainSceneName;

        public override void OnInit()
        {
            _currentMainScene = null;
            // 用激活场景更稳（可能并非 BuildIndex 0 启动）
            _currentMainSceneName = SceneManager.GetActiveScene().name;
        }

        public override void Shutdown()
        {
            // 非阻塞卸载（如需可等待版本，可额外提供 ShutdownAsync）
            foreach (var kv in _subScenes)
            {
                kv.Value?.UnloadAsync();
            }

            _subScenes.Clear();
            _handlingScene.Clear();

            _currentMainScene?.Release();
            _currentMainScene = null;
            _currentMainSceneName = string.Empty;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        public async UniTask<Scene> LoadSceneAsync(
            string location,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            bool suspendLoad = false,
            uint priority = 100,
            bool gcCollect = true,
            Action<float> progressCallBack = null)
        {
            if (!_handlingScene.Add(location))
            {
                Log.Error($"Could not load scene while handling: {location}");
                return default;
            }

            try
            {
                if (sceneMode == LoadSceneMode.Additive)
                {
                    if (_subScenes.TryGetValue(location, out var existed))
                    {
                        Log.Warning($"SubScene already loaded: {location}");
                        return existed.SceneObject;
                    }

                    var handle = YooAssets.LoadSceneAsync(location, LoadSceneMode.Additive, LocalPhysicsMode.None, suspendLoad, priority);

                    if (progressCallBack != null)
                        await TrackLoadProgress(handle, progressCallBack);
                    else
                        await handle.ToUniTask();

                    if (handle.Status != EOperationStatus.Succeed)
                    {
                        Log.Error($"Load additive scene failed: {location}, {handle.LastError}");
                        return default;
                    }

                    _subScenes[location] = handle;
                    return handle.SceneObject;
                }
                else
                {
                    // 主场景加载中的并发保护
                    if (_currentMainScene is { IsDone: false })
                        throw new Exception($"Could not load MainScene while loading. CurrentMainScene: {_currentMainSceneName}.");

                    var oldHandle = _currentMainScene;
                    var oldName = _currentMainSceneName;

                    // 先创建句柄，以便期间可以调用 Activate/IsMainScene 等
                    var handle = YooAssets.LoadSceneAsync(location, LoadSceneMode.Single, LocalPhysicsMode.None, suspendLoad, priority);
                    _currentMainScene = handle;

                    if (progressCallBack != null)
                        await TrackLoadProgress(handle, progressCallBack);
                    else
                        await handle.ToUniTask();

                    if (handle.Status != EOperationStatus.Succeed)
                    {
                        // 失败回滚
                        _currentMainScene = oldHandle;
                        _currentMainSceneName = oldName;
                        Log.Error($"Load main scene failed: {location}, {handle.LastError}");
                        return default;
                    }

                    // 成功后再更新主场景名，并释放旧句柄
                    _currentMainSceneName = location;
                    oldHandle?.Release();

                    // 回收在完成后进行，避免中途回收导致卡顿/黑屏
                    SafeForceUnloadUnusedAssets(gcCollect);

                    return handle.SceneObject;
                }
            }
            finally
            {
                _handlingScene.Remove(location);
            }
        }

        /// <summary>
        /// 回调式加载场景
        /// </summary>
        public void LoadScene(
            string location,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            bool suspendLoad = false,
            uint priority = 100,
            Action<Scene> callBack = null,
            bool gcCollect = true,
            Action<float> progressCallBack = null)
        {
            if (!_handlingScene.Add(location))
            {
                Log.Error($"Could not load scene while handling: {location}");
                return;
            }

            try
            {
                if (sceneMode == LoadSceneMode.Additive)
                {
                    if (_subScenes.TryGetValue(location, out var existed))
                    {
                        Log.Warning($"SubScene already loaded: {location}");
                        callBack?.Invoke(existed.SceneObject);
                        return;
                    }

                    var handle = YooAssets.LoadSceneAsync(location, LoadSceneMode.Additive, LocalPhysicsMode.None, suspendLoad, priority);

                    // 仅在完成且成功后，登记到字典
                    handle.Completed += h =>
                    {
                        _handlingScene.Remove(location);

                        if (h.Status == EOperationStatus.Succeed)
                            _subScenes[location] = h;
                        else
                            Log.Error($"Load additive scene failed: {location}, {h.LastError}");

                        callBack?.Invoke(h.SceneObject);
                    };

                    if (progressCallBack != null)
                        TrackLoadProgress(handle, progressCallBack).Forget();
                }
                else
                {
                    if (_currentMainScene is { IsDone: false })
                    {
                        Log.Warning($"Could not load MainScene while loading. CurrentMainScene: {_currentMainSceneName}.");
                        _handlingScene.Remove(location);
                        return;
                    }

                    var oldHandle = _currentMainScene;
                    var oldName = _currentMainSceneName;

                    var handle = YooAssets.LoadSceneAsync(location, LoadSceneMode.Single, LocalPhysicsMode.None, suspendLoad, priority);
                    _currentMainScene = handle; // 先记录，避免期间空引用

                    handle.Completed += h =>
                    {
                        if (h.Status == EOperationStatus.Succeed)
                        {
                            _currentMainSceneName = location;
                            oldHandle?.Release();
                            SafeForceUnloadUnusedAssets(gcCollect);
                        }
                        else
                        {
                            // 失败回滚
                            _currentMainScene = oldHandle;
                            _currentMainSceneName = oldName;
                            Log.Error($"Load main scene failed: {location}, {h.LastError}");
                        }

                        _handlingScene.Remove(location);
                        callBack?.Invoke(h.SceneObject);
                    };

                    if (progressCallBack != null)
                        TrackLoadProgress(handle, progressCallBack).Forget();
                }
            }
            catch (Exception e)
            {
                _handlingScene.Remove(location);
                Log.Error(e.ToString());
            }
        }

        public bool ActivateScene(string location)
        {
            if (string.Equals(_currentMainSceneName, location, StringComparison.Ordinal))
            {
                if (_currentMainScene != null)
                    return _currentMainScene.ActivateScene();

                Log.Warning($"ActivateScene failed, main handle is null. location: {location}");
                return false;
            }

            if (_subScenes.TryGetValue(location, out var sub) && sub != null)
                return sub.ActivateScene();

            Log.Warning($"ActivateScene invalid location: {location}");
            return false;
        }

        public bool UnSuspend(string location)
        {
            if (string.Equals(_currentMainSceneName, location, StringComparison.Ordinal))
            {
                if (_currentMainScene != null)
                    return _currentMainScene.UnSuspend();

                Log.Warning($"UnSuspend failed, main handle is null. location: {location}");
                return false;
            }

            if (_subScenes.TryGetValue(location, out var sub) && sub != null)
                return sub.UnSuspend();

            Log.Warning($"UnSuspend invalid location: {location}");
            return false;
        }

        public bool IsMainScene(string location)
        {
            // 仅判断“传入 location 是否为当前主场景”
            return string.Equals(_currentMainSceneName, location, StringComparison.Ordinal);
        }

        public async UniTask<bool> UnloadAsync(string location, Action<float> progressCallBack = null)
        {
            if (!_subScenes.TryGetValue(location, out var subScene) || subScene == null)
            {
                Log.Warning($"UnloadAsync invalid location: {location}");
                return false;
            }

            if (subScene.SceneObject == default)
            {
                Log.Error($"Could not unload Scene while not loaded. Scene: {location}");
                return false;
            }

            if (!_handlingScene.Add(location))
            {
                Log.Warning($"Could not unload Scene while handling: {location}");
                return false;
            }

            try
            {
                var op = subScene.UnloadAsync();

                if (progressCallBack != null)
                    await TrackUnloadProgress(op, progressCallBack);
                else
                    await op.ToUniTask();

                _subScenes.Remove(location);
                return op.Status == EOperationStatus.Succeed;
            }
            finally
            {
                _handlingScene.Remove(location);
            }
        }

        public void Unload(string location, Action callBack = null, Action<float> progressCallBack = null)
        {
            if (!_subScenes.TryGetValue(location, out var subScene) || subScene == null)
            {
                Log.Warning($"Unload invalid location: {location}");
                return;
            }

            if (subScene.SceneObject == default)
            {
                Log.Error($"Could not unload Scene while not loaded. Scene: {location}");
                return;
            }

            if (!_handlingScene.Add(location))
            {
                Log.Warning($"Could not unload Scene while handling: {location}");
                return;
            }

            try
            {
                var op = subScene.UnloadAsync();

                op.Completed += _ =>
                {
                    _subScenes.Remove(location);
                    _handlingScene.Remove(location);
                    callBack?.Invoke();
                };

                if (progressCallBack != null)
                    TrackUnloadProgress(op, progressCallBack).Forget();
            }
            catch (Exception e)
            {
                _handlingScene.Remove(location);
                Log.Error(e.ToString());
            }
        }

        public bool IsContainScene(string location)
        {
            if (string.Equals(_currentMainSceneName, location, StringComparison.Ordinal))
                return true;

            return _subScenes.ContainsKey(location);
        }

        // -------- Helpers --------

        private async UniTask TrackLoadProgress(SceneHandle handle, Action<float> progress)
        {
            try
            {
                if (handle == null) return;

                while (!handle.IsDone && handle.IsValid)
                {
                    progress?.Invoke(handle.Progress);
                    await UniTask.Yield();
                }

                // 收尾，确保回调到 1
                progress?.Invoke(1f);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        private async UniTask TrackUnloadProgress(UnloadSceneOperation op, Action<float> progress)
        {
            try
            {
                if (op == null) return;

                while (!op.IsDone && op.Status != EOperationStatus.Failed)
                {
                    progress?.Invoke(op.Progress);
                    await UniTask.Yield();
                }

                progress?.Invoke(op.Status == EOperationStatus.Succeed ? 1f : op.Progress);
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        private void SafeForceUnloadUnusedAssets(bool gcCollect)
        {
            try
            {
                ModuleSystem.GetModule<IResourceModule>()?.ForceUnloadUnusedAssets(gcCollect);
            }
            catch (Exception e)
            {
                Log.Warning($"ForceUnloadUnusedAssets exception: {e}");
            }
        }

        //供外部需要“可等待关闭”时使用
        public async UniTask ShutdownAsync()
        {
            var tasks = new List<UniTask>();
            foreach (var kv in _subScenes)
            {
                var op = kv.Value?.UnloadAsync();
                if (op != null) tasks.Add(op.ToUniTask());
            }
            await UniTask.WhenAll(tasks);

            _subScenes.Clear();
            _handlingScene.Clear();

            _currentMainScene?.Release();
            _currentMainScene = null;
            _currentMainSceneName = string.Empty;
        }
    }
}
