#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.multiplier
{
    /**
    * Interface for classes encapsulating a point multiplication algorithm
    * for <code>ECPoint</code>s.
    */
    public interface ECMultiplier
    {
        /**
         * Multiplies the <code>ECPoint p</code> by <code>k</code>, i.e.
         * <code>p</code> is added <code>k</code> times to itself.
         * @param p The <code>ECPoint</code> to be multiplied.
         * @param k The factor by which <code>p</code> is multiplied.
         * @return <code>p</code> multiplied by <code>k</code>.
         */
        ECPoint Multiply(ECPoint p, BigInteger k);
    }
}

#endif
