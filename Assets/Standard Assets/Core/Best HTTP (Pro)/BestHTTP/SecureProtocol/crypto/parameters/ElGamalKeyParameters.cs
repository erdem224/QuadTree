#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Standard_Assets.Core.BestHTTP.SecureProtocol.util;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class ElGamalKeyParameters
		: AsymmetricKeyParameter
    {
        private readonly ElGamalParameters parameters;

		protected ElGamalKeyParameters(
            bool				isPrivate,
            ElGamalParameters	parameters)
			: base(isPrivate)
        {
			// TODO Should we allow 'parameters' to be null?
            this.parameters = parameters;
        }

		public ElGamalParameters Parameters
        {
            get { return parameters; }
        }

		public override bool Equals(
            object obj)
        {
			if (obj == this)
				return true;

			ElGamalKeyParameters other = obj as ElGamalKeyParameters;

			if (other == null)
				return false;

			return Equals(other);
        }

		protected bool Equals(
			ElGamalKeyParameters other)
		{
			return Platform.Equals(parameters, other.parameters)
				&& base.Equals(other);
		}

		public override int GetHashCode()
        {
			int hc = base.GetHashCode();

			if (parameters != null)
			{
				hc ^= parameters.GetHashCode();
			}

			return hc;
        }
    }
}

#endif
