#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    public abstract class DefaultTlsClient
        :   AbstractTlsClient
    {
        public DefaultTlsClient()
            :   base()
        {
        }

        public DefaultTlsClient(TlsCipherFactory cipherFactory)
            :   base(cipherFactory)
        {
        }

        public override int[] GetCipherSuites()
        {
            return new int[]
            {
                CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
                CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
                CipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
                CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
                CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256,
                CipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
                CipherSuite.TLS_DHE_DSS_WITH_AES_128_GCM_SHA256,
                CipherSuite.TLS_DHE_DSS_WITH_AES_128_CBC_SHA256,
                CipherSuite.TLS_DHE_DSS_WITH_AES_128_CBC_SHA,
                CipherSuite.TLS_DHE_RSA_WITH_AES_128_GCM_SHA256,
                CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA256,
                CipherSuite.TLS_DHE_RSA_WITH_AES_128_CBC_SHA,
                CipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
                CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA256,
                CipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
            };
        }

        public override TlsKeyExchange GetKeyExchange()
        {
            int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite);

            switch (keyExchangeAlgorithm)
            {
            case KeyExchangeAlgorithm.DH_DSS:
            case KeyExchangeAlgorithm.DH_RSA:
                return CreateDHKeyExchange(keyExchangeAlgorithm);

            case KeyExchangeAlgorithm.DHE_DSS:
            case KeyExchangeAlgorithm.DHE_RSA:
                return CreateDheKeyExchange(keyExchangeAlgorithm);

            case KeyExchangeAlgorithm.ECDH_anon:
            case KeyExchangeAlgorithm.ECDH_ECDSA:
            case KeyExchangeAlgorithm.ECDH_RSA:
                return CreateECDHKeyExchange(keyExchangeAlgorithm);

            case KeyExchangeAlgorithm.ECDHE_ECDSA:
            case KeyExchangeAlgorithm.ECDHE_RSA:
                return CreateECDheKeyExchange(keyExchangeAlgorithm);

            case KeyExchangeAlgorithm.RSA:
                return CreateRsaKeyExchange();

            default:
                /*
                    * Note: internal error here; the TlsProtocol implementation verifies that the
                    * server-selected cipher suite was in the list of client-offered cipher suites, so if
                    * we now can't produce an implementation, we shouldn't have offered it!
                    */
                throw new TlsFatalAlert(AlertDescription.internal_error);
            }
        }

        protected virtual TlsKeyExchange CreateDHKeyExchange(int keyExchange)
        {
            return new TlsDHKeyExchange(keyExchange, mSupportedSignatureAlgorithms, null);
        }

        protected virtual TlsKeyExchange CreateDheKeyExchange(int keyExchange)
        {
            return new TlsDheKeyExchange(keyExchange, mSupportedSignatureAlgorithms, null);
        }

        protected virtual TlsKeyExchange CreateECDHKeyExchange(int keyExchange)
        {
            return new TlsECDHKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mNamedCurves, mClientECPointFormats,
                mServerECPointFormats);
        }

        protected virtual TlsKeyExchange CreateECDheKeyExchange(int keyExchange)
        {
            return new TlsECDheKeyExchange(keyExchange, mSupportedSignatureAlgorithms, mNamedCurves, mClientECPointFormats,
                mServerECPointFormats);
        }

        protected virtual TlsKeyExchange CreateRsaKeyExchange()
        {
            return new TlsRsaKeyExchange(mSupportedSignatureAlgorithms);
        }
    }
}

#endif
