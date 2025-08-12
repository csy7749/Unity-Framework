using System.IO;

namespace UnityFramework
{
    /// <summary>
    /// 自定义加密读取流：在 Read 阶段对读取到的区间做 XOR 解密。
    /// 关键点：只对「有效读取的字节」异或，且要考虑 offset 偏移；否则会破坏缓存区其他部分的数据。
    /// </summary>
    public class BundleStream : FileStream
    {
        public const byte KEY = 64;

        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access,
            share)
        {
        }

        public BundleStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            int read = base.Read(array, offset, count);
            for (int i = 0; i < read; i++)
                array[offset + i] ^= KEY; // 仅处理 [offset, offset+read) 区间
            return read;
        }
    }
}