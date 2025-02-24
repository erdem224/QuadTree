#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class ParametersWithIV
		: ICipherParameters
    {
		private readonly ICipherParameters	parameters;
		private readonly byte[]				iv;

		public ParametersWithIV(
            ICipherParameters	parameters,
            byte[]				iv)
			: this(parameters, iv, 0, iv.Length)
		{
		}

		public ParametersWithIV(
            ICipherParameters	parameters,
            byte[]				iv,
            int					ivOff,
            int					ivLen)
        {
            // NOTE: 'parameters' may be null to imply key re-use
			if (iv == null)
				throw new ArgumentNullException("iv");

			this.parameters = parameters;
			this.iv = new byte[ivLen];
            Array.Copy(iv, ivOff, this.iv, 0, ivLen);
        }

		public byte[] GetIV()
        {
			return (byte[]) iv.Clone();
        }

		public ICipherParameters Parameters
        {
            get { return parameters; }
        }
    }
}

#endif
