#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || NETFX_CORE || PORTABLE)
    [Serializable]
#endif
    public class Asn1Exception
		: IOException
	{
		public Asn1Exception()
			: base()
		{
		}

		public Asn1Exception(
			string message)
			: base(message)
		{
		}

		public Asn1Exception(
			string		message,
			Exception	exception)
			: base(message, exception)
		{
		}
	}
}

#endif
