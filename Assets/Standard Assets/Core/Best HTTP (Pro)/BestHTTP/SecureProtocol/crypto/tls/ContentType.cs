#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /**
     * RFC 2246 6.2.1
     */
    public abstract class ContentType
    {
        public const byte change_cipher_spec = 20;
        public const byte alert = 21;
        public const byte handshake = 22;
        public const byte application_data = 23;
        public const byte heartbeat = 24;
    }
}

#endif
