#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{

    /// <summary> Cipher parameters with a fixed salt value associated with them.</summary>
    public class ParametersWithSalt : ICipherParameters
    {
        private byte[] salt;
        private ICipherParameters parameters;

        public ParametersWithSalt(ICipherParameters parameters, byte[] salt):this(parameters, salt, 0, salt.Length)
        {
        }

        public ParametersWithSalt(ICipherParameters parameters, byte[] salt, int saltOff, int saltLen)
        {
            this.salt = new byte[saltLen];
            this.parameters = parameters;

            Array.Copy(salt, saltOff, this.salt, 0, saltLen);
        }

        public byte[] GetSalt()
        {
            return salt;
        }

        public ICipherParameters Parameters
        {
            get
            {
                return parameters;
            }
        }
    }
}

#endif
