#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /// <summary>RFC 2246</summary>
    /// <remarks>
    /// Note that the values here are implementation-specific and arbitrary. It is recommended not to
    /// depend on the particular values (e.g. serialization).
    /// </remarks>
    public abstract class EncryptionAlgorithm
    {
        public const int NULL = 0;
        public const int RC4_40 = 1;
        public const int RC4_128 = 2;
        public const int RC2_CBC_40 = 3;
        public const int IDEA_CBC = 4;
        public const int DES40_CBC = 5;
        public const int DES_CBC = 6;
        public const int cls_3DES_EDE_CBC = 7;

        /*
         * RFC 3268
         */
        public const int AES_128_CBC = 8;
        public const int AES_256_CBC = 9;

        /*
         * RFC 5289
         */
        public const int AES_128_GCM = 10;
        public const int AES_256_GCM = 11;

        /*
         * RFC 4132
         */
        public const int CAMELLIA_128_CBC = 12;
        public const int CAMELLIA_256_CBC = 13;

        /*
         * RFC 4162
         */
        public const int SEED_CBC = 14;

        /*
         * RFC 6655
         */
        public const int AES_128_CCM = 15;
        public const int AES_128_CCM_8 = 16;
        public const int AES_256_CCM = 17;
        public const int AES_256_CCM_8 = 18;

        /*
         * RFC 6367
         */
        public const int CAMELLIA_128_GCM = 19;
        public const int CAMELLIA_256_GCM = 20;

        /*
         * draft-ietf-tls-chacha20-poly1305-04
         */
        public const int CHACHA20_POLY1305 = 102;
        [Obsolete] public const int AEAD_CHACHA20_POLY1305 = CHACHA20_POLY1305;

        /*
         * draft-zauner-tls-aes-ocb-04
         */
        public const int AES_128_OCB_TAGLEN96 = 103;
        public const int AES_256_OCB_TAGLEN96 = 104;
    }
}

#endif
