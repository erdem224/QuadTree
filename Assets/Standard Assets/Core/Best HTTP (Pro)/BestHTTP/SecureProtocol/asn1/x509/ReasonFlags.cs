#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.x509
{
    /**
     * The ReasonFlags object.
     * <pre>
     * ReasonFlags ::= BIT STRING {
     *    unused(0),
     *    keyCompromise(1),
     *    cACompromise(2),
     *    affiliationChanged(3),
     *    superseded(4),
     *    cessationOfOperation(5),
     *    certficateHold(6)
     * }
     * </pre>
     */
    public class ReasonFlags
        : DerBitString
    {
        public const int Unused                 = (1 << 7);
        public const int KeyCompromise          = (1 << 6);
        public const int CACompromise           = (1 << 5);
        public const int AffiliationChanged     = (1 << 4);
        public const int Superseded             = (1 << 3);
        public const int CessationOfOperation   = (1 << 2);
        public const int CertificateHold        = (1 << 1);
        public const int PrivilegeWithdrawn     = (1 << 0);
        public const int AACompromise           = (1 << 15);

		/**
         * @param reasons - the bitwise OR of the Key Reason flags giving the
         * allowed uses for the key.
         */
        public ReasonFlags(int reasons)
             : base(reasons)
        {
        }

        public ReasonFlags(
            DerBitString reasons)
             : base(reasons.GetBytes(), reasons.PadBits)
        {
        }
    }
}

#endif
