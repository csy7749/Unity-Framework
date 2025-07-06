using YooAsset;

namespace GameLogic.OtherPackage
{

    public class OtherRemoteServices:IRemoteServices
    {        
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public OtherRemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        public string GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}{fileName}";
        }

        public string GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}{fileName}";
        }
    }
}