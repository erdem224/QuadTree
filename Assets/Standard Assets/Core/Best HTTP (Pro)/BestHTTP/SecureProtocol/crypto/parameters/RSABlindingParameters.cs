#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Standard_Assets.Core.BestHTTP.SecureProtocol.math;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
	public class RsaBlindingParameters
		: ICipherParameters
	{
		private readonly RsaKeyParameters	publicKey;
		private readonly BigInteger			blindingFactor;

		public RsaBlindingParameters(
			RsaKeyParameters	publicKey,
			BigInteger			blindingFactor)
		{
			if (publicKey.IsPrivate)
				throw new ArgumentException("RSA parameters should be for a public key");

			this.publicKey = publicKey;
			this.blindingFactor = blindingFactor;
		}

		public RsaKeyParameters PublicKey
		{
			get { return publicKey; }
		}

		public BigInteger BlindingFactor
		{
			get { return blindingFactor; }
		}
	}
}

#endif
