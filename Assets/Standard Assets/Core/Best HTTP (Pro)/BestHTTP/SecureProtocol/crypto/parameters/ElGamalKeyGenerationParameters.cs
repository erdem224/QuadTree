#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Standard_Assets.Core.BestHTTP.SecureProtocol.security;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class ElGamalKeyGenerationParameters
		: KeyGenerationParameters
    {
        private readonly ElGamalParameters parameters;

		public ElGamalKeyGenerationParameters(
            SecureRandom		random,
            ElGamalParameters	parameters)
			: base(random, GetStrength(parameters))
        {
            this.parameters = parameters;
        }

		public ElGamalParameters Parameters
        {
            get { return parameters; }
        }

		internal static int GetStrength(
			ElGamalParameters parameters)
		{
			return parameters.L != 0 ? parameters.L : parameters.P.BitLength;
		}
    }
}

#endif
