#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters;
using Standard_Assets.Core.BestHTTP.SecureProtocol.math;
using Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec;
using Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.multiplier;
using Standard_Assets.Core.BestHTTP.SecureProtocol.security;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.signers
{
    /**
     * GOST R 34.10-2001 Signature Algorithm
     */
    public class ECGost3410Signer
        : IDsa
    {
        private ECKeyParameters key;
        private SecureRandom random;

        public virtual string AlgorithmName
        {
            get { return "ECGOST3410"; }
        }

        public virtual void Init(
            bool				forSigning,
            ICipherParameters	parameters)
        {
            if (forSigning)
            {
                if (parameters is ParametersWithRandom)
                {
                    ParametersWithRandom rParam = (ParametersWithRandom)parameters;

                    this.random = rParam.Random;
                    parameters = rParam.Parameters;
                }
                else
                {
                    this.random = new SecureRandom();
                }

                if (!(parameters is ECPrivateKeyParameters))
                    throw new InvalidKeyException("EC private key required for signing");

                this.key = (ECPrivateKeyParameters) parameters;
            }
            else
            {
                if (!(parameters is ECPublicKeyParameters))
                    throw new InvalidKeyException("EC public key required for verification");

                this.key = (ECPublicKeyParameters)parameters;
            }
        }

        /**
         * generate a signature for the given message using the key we were
         * initialised with. For conventional GOST3410 the message should be a GOST3411
         * hash of the message of interest.
         *
         * @param message the message that will be verified later.
         */
        public virtual BigInteger[] GenerateSignature(
            byte[] message)
        {
            byte[] mRev = new byte[message.Length]; // conversion is little-endian
            for (int i = 0; i != mRev.Length; i++)
            {
                mRev[i] = message[mRev.Length - 1 - i];
            }

            BigInteger e = new BigInteger(1, mRev);

            ECDomainParameters ec = key.Parameters;
            BigInteger n = ec.N;
            BigInteger d = ((ECPrivateKeyParameters)key).D;

            BigInteger r, s = null;

            ECMultiplier basePointMultiplier = CreateBasePointMultiplier();

            do // generate s
            {
                BigInteger k;
                do // generate r
                {
                    do
                    {
                        k = new BigInteger(n.BitLength, random);
                    }
                    while (k.SignValue == 0);

                    ECPoint p = basePointMultiplier.Multiply(ec.G, k).Normalize();

                    r = p.AffineXCoord.ToBigInteger().Mod(n);
                }
                while (r.SignValue == 0);

                s = (k.Multiply(e)).Add(d.Multiply(r)).Mod(n);
            }
            while (s.SignValue == 0);

            return new BigInteger[]{ r, s };
        }

        /**
         * return true if the value r and s represent a GOST3410 signature for
         * the passed in message (for standard GOST3410 the message should be
         * a GOST3411 hash of the real message to be verified).
         */
        public virtual bool VerifySignature(
            byte[]		message,
            BigInteger	r,
            BigInteger	s)
        {
            byte[] mRev = new byte[message.Length]; // conversion is little-endian
            for (int i = 0; i != mRev.Length; i++)
            {
                mRev[i] = message[mRev.Length - 1 - i];
            }

            BigInteger e = new BigInteger(1, mRev);
            BigInteger n = key.Parameters.N;

            // r in the range [1,n-1]
            if (r.CompareTo(BigInteger.One) < 0 || r.CompareTo(n) >= 0)
            {
                return false;
            }

            // s in the range [1,n-1]
            if (s.CompareTo(BigInteger.One) < 0 || s.CompareTo(n) >= 0)
            {
                return false;
            }

            BigInteger v = e.ModInverse(n);

            BigInteger z1 = s.Multiply(v).Mod(n);
            BigInteger z2 = (n.Subtract(r)).Multiply(v).Mod(n);

            ECPoint G = key.Parameters.G; // P
            ECPoint Q = ((ECPublicKeyParameters)key).Q;

            ECPoint point = ECAlgorithms.SumOfTwoMultiplies(G, z1, Q, z2).Normalize();

            if (point.IsInfinity)
                return false;

            BigInteger R = point.AffineXCoord.ToBigInteger().Mod(n);

            return R.Equals(r);
        }

        protected virtual ECMultiplier CreateBasePointMultiplier()
        {
            return new FixedPointCombMultiplier();
        }
    }
}

#endif
