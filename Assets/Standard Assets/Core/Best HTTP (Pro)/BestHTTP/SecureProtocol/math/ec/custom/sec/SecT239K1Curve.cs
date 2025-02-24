#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.multiplier;
using Standard_Assets.Core.BestHTTP.SecureProtocol.util.encoders;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.custom.sec
{
    internal class SecT239K1Curve
        : AbstractF2mCurve
    {
        private const int SecT239K1_DEFAULT_COORDS = COORD_LAMBDA_PROJECTIVE;

        protected readonly SecT239K1Point m_infinity;

        public SecT239K1Curve()
            : base(239, 158, 0, 0)
        {
            this.m_infinity = new SecT239K1Point(this, null, null);

            this.m_a = FromBigInteger(BigInteger.Zero);
            this.m_b = FromBigInteger(BigInteger.One);
            this.m_order = new BigInteger(1, Hex.Decode("2000000000000000000000000000005A79FEC67CB6E91F1C1DA800E478A5"));
            this.m_cofactor = BigInteger.ValueOf(4);

            this.m_coord = SecT239K1_DEFAULT_COORDS;
        }

        protected override ECCurve CloneCurve()
        {
            return new SecT239K1Curve();
        }

        public override bool SupportsCoordinateSystem(int coord)
        {
            switch (coord)
            {
            case COORD_LAMBDA_PROJECTIVE:
                return true;
            default:
                return false;
            }
        }

        protected override ECMultiplier CreateDefaultMultiplier()
        {
            return new WTauNafMultiplier();
        }

        public override ECPoint Infinity
        {
            get { return m_infinity; }
        }

        public override int FieldSize
        {
            get { return 239; }
        }

        public override ECFieldElement FromBigInteger(BigInteger x)
        {
            return new SecT239FieldElement(x);
        }

        protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, bool withCompression)
        {
            return new SecT239K1Point(this, x, y, withCompression);
        }

        protected internal override ECPoint CreateRawPoint(ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
        {
            return new SecT239K1Point(this, x, y, zs, withCompression);
        }

        public override bool IsKoblitz
        {
            get { return true; }
        }

        public virtual int M
        {
            get { return 239; }
        }

        public virtual bool IsTrinomial
        {
            get { return true; }
        }

        public virtual int K1
        {
            get { return 158; }
        }

        public virtual int K2
        {
            get { return 0; }
        }

        public virtual int K3
        {
            get { return 0; }
        }
    }
}

#endif
