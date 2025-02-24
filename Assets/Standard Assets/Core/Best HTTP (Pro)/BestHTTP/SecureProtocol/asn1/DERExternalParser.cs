#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1
{
	public class DerExternalParser
		: Asn1Encodable
	{
		private readonly Asn1StreamParser _parser;

		public DerExternalParser(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public IAsn1Convertible ReadObject()
		{
			return _parser.ReadObject();
		}

		public override Asn1Object ToAsn1Object()
		{
			return new DerExternal(_parser.ReadVector());
		}
	}
}

#endif
