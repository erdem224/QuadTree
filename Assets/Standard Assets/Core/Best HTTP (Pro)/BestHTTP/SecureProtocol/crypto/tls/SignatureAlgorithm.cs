#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /**
     * RFC 5246 7.4.1.4.1 (in RFC 2246, there were no specific values assigned)
     */
    public abstract class SignatureAlgorithm
    {
        public const byte anonymous = 0;
        public const byte rsa = 1;
        public const byte dsa = 2;
        public const byte ecdsa = 3;
    }
}

#endif
