using System;
using System.IO;
using UnityEngine;
using YooAsset;

namespace UnityFramework
{
    /// <summary>
    /// 远端地址拼接服务（主/备 URL）。
    /// </summary>
    internal class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName) => $"{_defaultHostServer}/{fileName}";
        string IRemoteServices.GetRemoteFallbackURL(string fileName) => $"{_fallbackHostServer}/{fileName}";
    }

    /// <summary>
    /// 构建期的「文件流」加密（简单 XOR 示例）。
    /// </summary>
    public class FileStreamEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var data = File.ReadAllBytes(fileInfo.FileLoadPath);
            for (int i = 0; i < data.Length; i++) data[i] ^= BundleStream.KEY;

            return new EncryptResult { Encrypted = true, EncryptedData = data };
        }
    }

    /// <summary>
    /// 运行期的「文件流」解密（通过 BundleStream 读取并 XOR）。
    /// </summary>
    public class FileStreamDecryption : IDecryptionServices
    {
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            var bs = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new DecryptResult { ManagedStream = bs, Result = AssetBundle.LoadFromStream(bs, 0, GetManagedReadBufferSize()) };
        }

        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            var bs = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new DecryptResult { ManagedStream = bs, CreateRequest = AssetBundle.LoadFromStreamAsync(bs, 0, GetManagedReadBufferSize()) };
        }

        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo) => throw new NotImplementedException();
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo) => throw new NotImplementedException();

        private static uint GetManagedReadBufferSize() => 1024;
    }

    /// <summary>
    /// 构建期的「文件偏移」加密（数据前面补 32 字节偏移）。
    /// </summary>
    public class FileOffsetEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            const int offset = 32;
            byte[] src = File.ReadAllBytes(fileInfo.FileLoadPath);
            var dst = new byte[src.Length + offset];
            Buffer.BlockCopy(src, 0, dst, offset, src.Length);
            return new EncryptResult { Encrypted = true, EncryptedData = dst };
        }
    }

    /// <summary>
    /// 运行期的「文件偏移」解密（从文件偏移位置开始加载）。
    /// </summary>
    public class FileOffsetDecryption : IDecryptionServices
    {
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            return new DecryptResult { ManagedStream = null, Result = AssetBundle.LoadFromFile(fileInfo.FileLoadPath, 0, GetFileOffset()) };
        }

        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            return new DecryptResult { ManagedStream = null, CreateRequest = AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, 0, GetFileOffset()) };
        }

        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo) => throw new NotImplementedException();
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo) => throw new NotImplementedException();

        private static ulong GetFileOffset() => 32;
    }

    #region Web Decryption（WebGL/小程序等内存态解密）

    /// <summary>
    /// Web 端的「文件偏移」解密（从内存解开）。
    /// </summary>
    public class FileOffsetWebDecryption : IWebDecryptionServices
    {
        public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            int offset = 32;
            byte[] data = new byte[fileInfo.FileData.Length - offset];
            Buffer.BlockCopy(fileInfo.FileData, offset, data, 0, data.Length);
            return new WebDecryptResult { Result = AssetBundle.LoadFromMemory(data) };
        }
    }

    /// <summary>
    /// Web 端的「文件流 XOR」解密（内存里逐字节 XOR）。
    /// </summary>
    public class FileStreamWebDecryption : IWebDecryptionServices
    {
        public WebDecryptResult LoadAssetBundle(WebDecryptFileInfo fileInfo)
        {
            byte[] data = new byte[fileInfo.FileData.Length];
            Buffer.BlockCopy(fileInfo.FileData, 0, data, 0, data.Length);
            for (int i = 0; i < data.Length; i++) data[i] ^= BundleStream.KEY;
            return new WebDecryptResult { Result = AssetBundle.LoadFromMemory(data) };
        }
    }

    #endregion
}
