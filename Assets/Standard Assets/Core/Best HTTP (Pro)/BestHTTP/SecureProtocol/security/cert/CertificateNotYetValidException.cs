#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.security.cert
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || NETFX_CORE || PORTABLE)
    [Serializable]
#endif
    public class CertificateNotYetValidException : CertificateException
	{
		public CertificateNotYetValidException() : base() { }
		public CertificateNotYetValidException(string message) : base(message) { }
		public CertificateNotYetValidException(string message, Exception exception) : base(message, exception) { }
	}
}

#endif
