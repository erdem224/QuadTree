#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.util.io
{
	public class PushbackStream
		: FilterStream
	{
		private int buf = -1;

		public PushbackStream(
			Stream s)
			: base(s)
		{
		}

		public override int ReadByte()
		{
			if (buf != -1)
			{
				int tmp = buf;
				buf = -1;
				return tmp;
			}

			return base.ReadByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buf != -1 && count > 0)
			{
				// TODO Can this case be made more efficient?
				buffer[offset] = (byte) buf;
				buf = -1;
				return 1;
			}

			return base.Read(buffer, offset, count);
		}

		public virtual void Unread(int b)
		{
			if (buf != -1)
				throw new InvalidOperationException("Can only push back one byte");

			buf = b & 0xFF;
		}
	}
}

#endif
