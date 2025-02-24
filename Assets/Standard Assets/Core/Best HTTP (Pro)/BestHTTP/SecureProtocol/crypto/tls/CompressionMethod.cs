#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /// <summary>
    /// RFC 2246 6.1
    /// </summary>
    public abstract class CompressionMethod
    {
        public const byte cls_null = 0;

        /*
         * RFC 3749 2
         */
        public const byte DEFLATE = 1;

        /*
         * Values from 224 decimal (0xE0) through 255 decimal (0xFF)
         * inclusive are reserved for private use.
         */
    }
}

#endif
