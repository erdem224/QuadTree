#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto
{
    /// <summary>
    /// Base interface for operators that serve as stream-based signature verifiers.
    /// </summary>
    public interface IVerifierFactory
	{
        /// <summary>The algorithm details object for this verifier.</summary>
        Object AlgorithmDetails { get ; }

        /// <summary>
        /// Create a stream calculator for this verifier. The stream
        /// calculator is used for the actual operation of entering the data to be verified
        /// and producing a result which can be used to verify the original signature.
        /// </summary>
        /// <returns>A calculator producing an IVerifier which can verify the signature.</returns>
        IStreamCalculator CreateCalculator();
    }
}

#endif