#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Standard_Assets.Core.BestHTTP.SecureProtocol.math.raw;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.custom.sec
{
    internal class SecP384R1Point
        : AbstractFpPoint
    {
        /**
         * Create a point which encodes with point compression.
         * 
         * @param curve
         *            the curve to use
         * @param x
         *            affine x co-ordinate
         * @param y
         *            affine y co-ordinate
         * 
         * @deprecated Use ECCurve.createPoint to construct points
         */
        public SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
            : this(curve, x, y, false)
        {
        }

        /**
         * Create a point that encodes with or without point compresion.
         * 
         * @param curve
         *            the curve to use
         * @param x
         *            affine x co-ordinate
         * @param y
         *            affine y co-ordinate
         * @param withCompression
         *            if true encode with point compression
         * 
         * @deprecated per-point compression property will be removed, refer
         *             {@link #getEncoded(bool)}
         */
        public SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
            : base(curve, x, y, withCompression)
        {
            if ((x == null) != (y == null))
                throw new ArgumentException("Exactly one of the field elements is null");
        }

        internal SecP384R1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs, bool withCompression)
            : base(curve, x, y, zs, withCompression)
        {
        }

        protected override ECPoint Detach()
        {
            return new SecP384R1Point(null, AffineXCoord, AffineYCoord);
        }

        public override ECPoint Add(ECPoint b)
        {
            if (this.IsInfinity)
                return b;
            if (b.IsInfinity)
                return this;
            if (this == b)
                return Twice();

            ECCurve curve = this.Curve;

            SecP384R1FieldElement X1 = (SecP384R1FieldElement)this.RawXCoord, Y1 = (SecP384R1FieldElement)this.RawYCoord;
            SecP384R1FieldElement X2 = (SecP384R1FieldElement)b.RawXCoord, Y2 = (SecP384R1FieldElement)b.RawYCoord;

            SecP384R1FieldElement Z1 = (SecP384R1FieldElement)this.RawZCoords[0];
            SecP384R1FieldElement Z2 = (SecP384R1FieldElement)b.RawZCoords[0];

            uint c;
            uint[] tt1 = Nat.Create(24);
            uint[] tt2 = Nat.Create(24);
            uint[] t3 = Nat.Create(12);
            uint[] t4 = Nat.Create(12);

            bool Z1IsOne = Z1.IsOne;
            uint[] U2, S2;
            if (Z1IsOne)
            {
                U2 = X2.x;
                S2 = Y2.x;
            }
            else
            {
                S2 = t3;
                SecP384R1Field.Square(Z1.x, S2);

                U2 = tt2;
                SecP384R1Field.Multiply(S2, X2.x, U2);

                SecP384R1Field.Multiply(S2, Z1.x, S2);
                SecP384R1Field.Multiply(S2, Y2.x, S2);
            }

            bool Z2IsOne = Z2.IsOne;
            uint[] U1, S1;
            if (Z2IsOne)
            {
                U1 = X1.x;
                S1 = Y1.x;
            }
            else
            {
                S1 = t4;
                SecP384R1Field.Square(Z2.x, S1);

                U1 = tt1;
                SecP384R1Field.Multiply(S1, X1.x, U1);

                SecP384R1Field.Multiply(S1, Z2.x, S1);
                SecP384R1Field.Multiply(S1, Y1.x, S1);
            }

            uint[] H = Nat.Create(12);
            SecP384R1Field.Subtract(U1, U2, H);

            uint[] R = Nat.Create(12);
            SecP384R1Field.Subtract(S1, S2, R);

            // Check if b == this or b == -this
            if (Nat.IsZero(12, H))
            {
                if (Nat.IsZero(12, R))
                {
                    // this == b, i.e. this must be doubled
                    return this.Twice();
                }

                // this == -b, i.e. the result is the point at infinity
                return curve.Infinity;
            }

            uint[] HSquared = t3;
            SecP384R1Field.Square(H, HSquared);

            uint[] G = Nat.Create(12);
            SecP384R1Field.Multiply(HSquared, H, G);

            uint[] V = t3;
            SecP384R1Field.Multiply(HSquared, U1, V);

            SecP384R1Field.Negate(G, G);
            Nat384.Mul(S1, G, tt1);

            c = Nat.AddBothTo(12, V, V, G);
            SecP384R1Field.Reduce32(c, G);

            SecP384R1FieldElement X3 = new SecP384R1FieldElement(t4);
            SecP384R1Field.Square(R, X3.x);
            SecP384R1Field.Subtract(X3.x, G, X3.x);

            SecP384R1FieldElement Y3 = new SecP384R1FieldElement(G);
            SecP384R1Field.Subtract(V, X3.x, Y3.x);
            Nat384.Mul(Y3.x, R, tt2);
            SecP384R1Field.AddExt(tt1, tt2, tt1);
            SecP384R1Field.Reduce(tt1, Y3.x);

            SecP384R1FieldElement Z3 = new SecP384R1FieldElement(H);
            if (!Z1IsOne)
            {
                SecP384R1Field.Multiply(Z3.x, Z1.x, Z3.x);
            }
            if (!Z2IsOne)
            {
                SecP384R1Field.Multiply(Z3.x, Z2.x, Z3.x);
            }

            ECFieldElement[] zs = new ECFieldElement[] { Z3 };

            return new SecP384R1Point(curve, X3, Y3, zs, IsCompressed);
        }

        public override ECPoint Twice()
        {
            if (this.IsInfinity)
                return this;

            ECCurve curve = this.Curve;

            SecP384R1FieldElement Y1 = (SecP384R1FieldElement)this.RawYCoord;
            if (Y1.IsZero)
                return curve.Infinity;

            SecP384R1FieldElement X1 = (SecP384R1FieldElement)this.RawXCoord, Z1 = (SecP384R1FieldElement)this.RawZCoords[0];

            uint c;
            uint[] t1 = Nat.Create(12);
            uint[] t2 = Nat.Create(12);

            uint[] Y1Squared = Nat.Create(12);
            SecP384R1Field.Square(Y1.x, Y1Squared);

            uint[] T = Nat.Create(12);
            SecP384R1Field.Square(Y1Squared, T);

            bool Z1IsOne = Z1.IsOne;

            uint[] Z1Squared = Z1.x;
            if (!Z1IsOne)
            {
                Z1Squared = t2;
                SecP384R1Field.Square(Z1.x, Z1Squared);
            }

            SecP384R1Field.Subtract(X1.x, Z1Squared, t1);

            uint[] M = t2;
            SecP384R1Field.Add(X1.x, Z1Squared, M);
            SecP384R1Field.Multiply(M, t1, M);
            c = Nat.AddBothTo(12, M, M, M);
            SecP384R1Field.Reduce32(c, M);

            uint[] S = Y1Squared;
            SecP384R1Field.Multiply(Y1Squared, X1.x, S);
            c = Nat.ShiftUpBits(12, S, 2, 0);
            SecP384R1Field.Reduce32(c, S);

            c = Nat.ShiftUpBits(12, T, 3, 0, t1);
            SecP384R1Field.Reduce32(c, t1);

            SecP384R1FieldElement X3 = new SecP384R1FieldElement(T);
            SecP384R1Field.Square(M, X3.x);
            SecP384R1Field.Subtract(X3.x, S, X3.x);
            SecP384R1Field.Subtract(X3.x, S, X3.x);

            SecP384R1FieldElement Y3 = new SecP384R1FieldElement(S);
            SecP384R1Field.Subtract(S, X3.x, Y3.x);
            SecP384R1Field.Multiply(Y3.x, M, Y3.x);
            SecP384R1Field.Subtract(Y3.x, t1, Y3.x);

            SecP384R1FieldElement Z3 = new SecP384R1FieldElement(M);
            SecP384R1Field.Twice(Y1.x, Z3.x);
            if (!Z1IsOne)
            {
                SecP384R1Field.Multiply(Z3.x, Z1.x, Z3.x);
            }

            return new SecP384R1Point(curve, X3, Y3, new ECFieldElement[] { Z3 }, IsCompressed);
        }

        public override ECPoint TwicePlus(ECPoint b)
        {
            if (this == b)
                return ThreeTimes();
            if (this.IsInfinity)
                return b;
            if (b.IsInfinity)
                return Twice();

            ECFieldElement Y1 = this.RawYCoord;
            if (Y1.IsZero)
                return b;

            return Twice().Add(b);
        }

        public override ECPoint ThreeTimes()
        {
            if (this.IsInfinity || this.RawYCoord.IsZero)
                return this;

            // NOTE: Be careful about recursions between TwicePlus and ThreeTimes
            return Twice().Add(this);
        }

        public override ECPoint Negate()
        {
            if (IsInfinity)
                return this;

            return new SecP384R1Point(Curve, RawXCoord, RawYCoord.Negate(), RawZCoords, IsCompressed);
        }
    }
}

#endif
