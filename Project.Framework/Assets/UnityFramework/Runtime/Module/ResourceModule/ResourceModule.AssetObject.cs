using YooAsset;

namespace UnityFramework
{
    internal partial class ResourceModule
    {
        /// <summary>
        /// 资源对象（与 YooAsset 的 AssetHandle 绑定）。
        /// - 进入对象池时持有 handle 与实例；
        /// - 释放（销毁）时会 Dispose 句柄，避免泄漏；
        /// - Clear 时清空引用，避免悬挂。
        /// </summary>
        private sealed class AssetObject : ObjectBase
        {
            private AssetHandle _assetHandle = null;
            private ResourceModule _resourceModule;

            public static AssetObject Create(string name, object target, object assetHandle, ResourceModule resourceModule)
            {
                if (assetHandle == null) throw new GameFrameworkException("Resource is invalid.");
                if (resourceModule == null) throw new GameFrameworkException("Resource Manager is invalid.");

                var obj = MemoryPool.Acquire<AssetObject>();
                obj.Initialize(name, target);
                obj._assetHandle = (AssetHandle)assetHandle;
                obj._resourceModule = resourceModule;
                return obj;
            }

            public static AssetObject TryCreate(string name, object target, object assetHandle, ResourceModule resourceModule)
            {
                if (assetHandle == null || resourceModule == null) return null;

                var obj = MemoryPool.Acquire<AssetObject>();
                obj.Initialize(name, target);
                obj._assetHandle = (AssetHandle)assetHandle;
                obj._resourceModule = resourceModule;
                return obj;
            }

            public override void Clear()
            {
                base.Clear();
                _assetHandle = null;
                _resourceModule = null;
            }

            /// <summary>
            /// 真正释放对象（从对象池移除时调用）。
            /// 非关机路径会主动 Dispose 句柄，防止引用计数残留。
            /// </summary>
            protected internal override void Release(bool isShutdown)
            {
                if (!isShutdown)
                {
                    if (_assetHandle is { IsValid: true })
                        _assetHandle.Dispose();
                }
                _assetHandle = null;
                _resourceModule = null;
            }
        }
    }
}
