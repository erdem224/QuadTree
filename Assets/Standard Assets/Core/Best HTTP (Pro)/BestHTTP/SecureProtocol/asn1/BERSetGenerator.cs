#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1
{
	public class BerSetGenerator
		: BerGenerator
	{
		public BerSetGenerator(
			Stream outStream)
			: base(outStream)
		{
			WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.Set);
		}

		public BerSetGenerator(
			Stream	outStream,
			int		tagNo,
			bool	isExplicit)
			: base(outStream, tagNo, isExplicit)
		{
			WriteBerHeader(Asn1Tags.Constructed | Asn1Tags.Set);
		}
	}
}

#endif
