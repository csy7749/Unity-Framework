namespace UnityFramework
{
    internal partial class ResourceModule
    {
        private IObjectPool<AssetObject> _assetPool;

        /// <summary>
        /// 资源对象池：自动释放间隔（秒）。
        /// </summary>
        public float AssetAutoReleaseInterval
        {
            get => _assetPool.AutoReleaseInterval;
            set => _assetPool.AutoReleaseInterval = value;
        }

        /// <summary>
        /// 资源对象池：容量（对象数量上限）。
        /// </summary>
        public int AssetCapacity
        {
            get => _assetPool.Capacity;
            set => _assetPool.Capacity = value;
        }

        /// <summary>
        /// 资源对象池：对象过期时间（秒）。
        /// </summary>
        public float AssetExpireTime
        {
            get => _assetPool.ExpireTime;
            set => _assetPool.ExpireTime = value;
        }

        /// <summary>
        /// 资源对象池：优先级（影响释放策略）。
        /// </summary>
        public int AssetPriority
        {
            get => _assetPool.Priority;
            set => _assetPool.Priority = value;
        }

        /// <summary>
        /// 卸载资源实例（将对象返还对象池；真正释放由对象池与 AssetObject.Release 控制）。
        /// </summary>
        public void UnloadAsset(object asset)
        {
            if (_assetPool != null)
                _assetPool.Unspawn(asset);
        }

        /// <summary>
        /// 设置对象池模块并创建本模块使用的多实例对象池。
        /// </summary>
        public void SetObjectPoolModule(IObjectPoolModule objectPoolModule)
        {
            if (objectPoolModule == null)
                throw new GameFrameworkException("Object pool manager is invalid.");
            _assetPool = objectPoolModule.CreateMultiSpawnObjectPool<AssetObject>("Asset Pool");
        }
    }
}