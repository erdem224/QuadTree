#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Standard_Assets.Core.BestHTTP.SecureProtocol.util;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.parameters
{
    public class DsaValidationParameters
    {
        private readonly byte[] seed;
        private readonly int counter;
        private readonly int usageIndex;

        public DsaValidationParameters(byte[] seed, int counter)
            : this(seed, counter, -1)
        {
        }

        public DsaValidationParameters(
            byte[]	seed,
            int		counter,
            int     usageIndex)
        {
            if (seed == null)
                throw new ArgumentNullException("seed");

            this.seed = (byte[]) seed.Clone();
            this.counter = counter;
            this.usageIndex = usageIndex;
        }

        public virtual byte[] GetSeed()
        {
            return (byte[]) seed.Clone();
        }

        public virtual int Counter
        {
            get { return counter; }
        }

        public virtual int UsageIndex
        {
            get { return usageIndex; }
        }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            DsaValidationParameters other = obj as DsaValidationParameters;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected virtual bool Equals(
            DsaValidationParameters other)
        {
            return counter == other.counter
                && Arrays.AreEqual(seed, other.seed);
        }

        public override int GetHashCode()
        {
            return counter.GetHashCode() ^ Arrays.GetHashCode(seed);
        }
    }
}

#endif
