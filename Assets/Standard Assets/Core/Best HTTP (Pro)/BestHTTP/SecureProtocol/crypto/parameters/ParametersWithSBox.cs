#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
	public class ParametersWithSBox : ICipherParameters
	{
		private ICipherParameters  parameters;
		private byte[] sBox;

		public ParametersWithSBox(
			ICipherParameters parameters,
			byte[] sBox)
		{
			this.parameters = parameters;
			this.sBox = sBox;
		}

		public byte[] GetSBox() { return sBox; }

		public ICipherParameters Parameters { get { return parameters; } }
	}
}

#endif
