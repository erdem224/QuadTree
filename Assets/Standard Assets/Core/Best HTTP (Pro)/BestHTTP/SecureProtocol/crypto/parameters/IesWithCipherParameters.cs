#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class IesWithCipherParameters : IesParameters
    {
        private int cipherKeySize;

        /**
         * @param derivation the derivation parameter for the KDF function.
         * @param encoding the encoding parameter for the KDF function.
         * @param macKeySize the size of the MAC key (in bits).
         * @param cipherKeySize the size of the associated Cipher key (in bits).
         */
        public IesWithCipherParameters(
            byte[]  derivation,
            byte[]  encoding,
            int     macKeySize,
            int     cipherKeySize) : base(derivation, encoding, macKeySize)
        {
            this.cipherKeySize = cipherKeySize;
        }

        public int CipherKeySize
        {
            get
            {
                return cipherKeySize;
            }
        }
    }

}

#endif
