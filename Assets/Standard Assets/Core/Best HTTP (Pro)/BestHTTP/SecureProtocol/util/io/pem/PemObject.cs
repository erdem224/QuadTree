#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.Collections;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.util.io.pem
{
	public class PemObject
		: PemObjectGenerator
	{
		private string		type;
		private IList		headers;
		private byte[]		content;

		public PemObject(string type, byte[] content)
			: this(type, Platform.CreateArrayList(), content)
		{
		}

		public PemObject(String type, IList headers, byte[] content)
		{
			this.type = type;
            this.headers = Platform.CreateArrayList(headers);
			this.content = content;
		}

		public string Type
		{
			get { return type; }
		}

		public IList Headers
		{
			get { return headers; }
		}

		public byte[] Content
		{
			get { return content; }
		}

		public PemObject Generate()
		{
			return this;
		}
	}
}

#endif
