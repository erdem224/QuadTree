#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    public interface TlsSignerCredentials
        :   TlsCredentials
    {
        /// <exception cref="IOException"></exception>
        byte[] GenerateCertificateSignature(byte[] hash);

        SignatureAndHashAlgorithm SignatureAndHashAlgorithm { get; }
    }
}

#endif
