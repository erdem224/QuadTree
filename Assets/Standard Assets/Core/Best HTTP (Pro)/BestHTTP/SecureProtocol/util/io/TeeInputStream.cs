#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.Diagnostics;
using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.util.io
{
	public class TeeInputStream
		: BaseInputStream
	{
		private readonly Stream input, tee;

		public TeeInputStream(Stream input, Stream tee)
		{
			Debug.Assert(input.CanRead);
			Debug.Assert(tee.CanWrite);

			this.input = input;
			this.tee = tee;
		}

#if PORTABLE || NETFX_CORE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Platform.Dispose(input);
                Platform.Dispose(tee);
            }
            base.Dispose(disposing);
        }
#else
        public override void Close()
		{
            Platform.Dispose(input);
            Platform.Dispose(tee);
            base.Close();
		}
#endif

        public override int Read(byte[] buf, int off, int len)
		{
			int i = input.Read(buf, off, len);

			if (i > 0)
			{
				tee.Write(buf, off, i);
			}

			return i;
		}

		public override int ReadByte()
		{
			int i = input.ReadByte();

			if (i >= 0)
			{
				tee.WriteByte((byte)i);
			}

			return i;
		}
	}
}

#endif
