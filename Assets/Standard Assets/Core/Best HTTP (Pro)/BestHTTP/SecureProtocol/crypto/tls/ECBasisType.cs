#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /// <summary>RFC 4492 5.4. (Errata ID: 2389)</summary>
    public abstract class ECBasisType
    {
        public const byte ec_basis_trinomial = 1;
        public const byte ec_basis_pentanomial = 2;

        public static bool IsValid(byte ecBasisType)
        {
            return ecBasisType >= ec_basis_trinomial && ecBasisType <= ec_basis_pentanomial;
        }
    }
}

#endif
