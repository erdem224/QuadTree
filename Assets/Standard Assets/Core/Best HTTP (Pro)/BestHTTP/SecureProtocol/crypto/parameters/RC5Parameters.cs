#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class RC5Parameters
		: KeyParameter
    {
        private readonly int rounds;

		public RC5Parameters(
            byte[]	key,
            int		rounds)
			: base(key)
        {
            if (key.Length > 255)
                throw new ArgumentException("RC5 key length can be no greater than 255");

			this.rounds = rounds;
        }

		public int Rounds
        {
			get { return rounds; }
        }
    }
}

#endif
