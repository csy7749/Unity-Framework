using YooAsset;

namespace UnityFramework
{
    /// <summary>
    /// 音频数据（持有 YooAsset 句柄）。
    /// </summary>
    public class AudioData : MemoryObject
    {
        public AssetHandle AssetHandle { private set; get; }
        public bool InPool             { private set; get; } = false;

        public override void InitFromPool() { }

        /// <summary>
        /// 回收到对象池：
        /// - 非池化播放：Dispose 句柄（引用计数-1）
        /// - 清空引用，避免悬挂
        /// </summary>
        public override void RecycleToPool()
        {
            if (!InPool)
                AssetHandle?.Dispose();

            InPool = false;
            AssetHandle = null;
        }

        internal static AudioData Alloc(AssetHandle assetHandle, bool inPool)
        {
            var ret = MemoryPool.Acquire<AudioData>();
            ret.AssetHandle = assetHandle;
            ret.InPool = inPool;
            ret.InitFromPool();
            return ret;
        }

        internal static void DeAlloc(AudioData audioData)
        {
            if (audioData != null)
            {
                // ★ 先回收（内部会处置非池化句柄），再归还对象池，避免二次释放
                audioData.RecycleToPool();
                MemoryPool.Release(audioData);
            }
        }
    }
}