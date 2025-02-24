#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.IO;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1
{
    public class Asn1OutputStream
        : DerOutputStream
    {
        public Asn1OutputStream(Stream os) : base(os)
        {
        }

		[Obsolete("Use version taking an Asn1Encodable arg instead")]
        public override void WriteObject(
            object    obj)
        {
            if (obj == null)
            {
                WriteNull();
            }
            else if (obj is Asn1Object)
            {
                ((Asn1Object)obj).Encode(this);
            }
            else if (obj is Asn1Encodable)
            {
                ((Asn1Encodable)obj).ToAsn1Object().Encode(this);
            }
            else
            {
                throw new IOException("object not Asn1Encodable");
            }
        }
    }
}

#endif
