#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System.Collections;
using Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.anssi;
using Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.nist;
using Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.sec;
using Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.teletrust;
using Standard_Assets.Core.BestHTTP.SecureProtocol.util;
using Standard_Assets.Core.BestHTTP.SecureProtocol.util.collections;

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.asn1.x9
{
    /**
     * A general class that reads all X9.62 style EC curve tables.
     */
    public class ECNamedCurveTable
    {
        /**
         * return a X9ECParameters object representing the passed in named
         * curve. The routine returns null if the curve is not present.
         *
         * @param name the name of the curve requested
         * @return an X9ECParameters object or null if the curve is not available.
         */
        public static X9ECParameters GetByName(string name)
        {
            X9ECParameters ecP = X962NamedCurves.GetByName(name);

            if (ecP == null)
            {
                ecP = SecNamedCurves.GetByName(name);
            }

            if (ecP == null)
            {
                ecP = NistNamedCurves.GetByName(name);
            }

            if (ecP == null)
            {
                ecP = TeleTrusTNamedCurves.GetByName(name);
            }

            if (ecP == null)
            {
                ecP = AnssiNamedCurves.GetByName(name);
            }

            return ecP;
        }

        public static string GetName(DerObjectIdentifier oid)
        {
            string name = X962NamedCurves.GetName(oid);
            if (name == null)
            {
                name = SecNamedCurves.GetName(oid);
            }
            if (name == null)
            {
                name = NistNamedCurves.GetName(oid);
            }
            if (name == null)
            {
                name = TeleTrusTNamedCurves.GetName(oid);
            }
            if (name == null)
            {
                name = AnssiNamedCurves.GetName(oid);
            }
            return name;
        }

        /**
         * return the object identifier signified by the passed in name. Null
         * if there is no object identifier associated with name.
         *
         * @return the object identifier associated with name, if present.
         */
        public static DerObjectIdentifier GetOid(string name)
        {
            DerObjectIdentifier oid = X962NamedCurves.GetOid(name);

            if (oid == null)
            {
                oid = SecNamedCurves.GetOid(name);
            }

            if (oid == null)
            {
                oid = NistNamedCurves.GetOid(name);
            }

            if (oid == null)
            {
                oid = TeleTrusTNamedCurves.GetOid(name);
            }

            if (oid == null)
            {
                oid = AnssiNamedCurves.GetOid(name);
            }

            return oid;
        }

        /**
         * return a X9ECParameters object representing the passed in named
         * curve.
         *
         * @param oid the object id of the curve requested
         * @return an X9ECParameters object or null if the curve is not available.
         */
        public static X9ECParameters GetByOid(DerObjectIdentifier oid)
        {
            X9ECParameters ecP = X962NamedCurves.GetByOid(oid);

            if (ecP == null)
            {
                ecP = SecNamedCurves.GetByOid(oid);
            }

            // NOTE: All the NIST curves are currently from SEC, so no point in redundant OID lookup

            if (ecP == null)
            {
                ecP = TeleTrusTNamedCurves.GetByOid(oid);
            }

            if (ecP == null)
            {
                ecP = AnssiNamedCurves.GetByOid(oid);
            }

            return ecP;
        }

        /**
         * return an enumeration of the names of the available curves.
         *
         * @return an enumeration of the names of the available curves.
         */
        public static IEnumerable Names
        {
            get
            {
                IList v = Platform.CreateArrayList();
                CollectionUtilities.AddRange(v, X962NamedCurves.Names);
                CollectionUtilities.AddRange(v, SecNamedCurves.Names);
                CollectionUtilities.AddRange(v, NistNamedCurves.Names);
                CollectionUtilities.AddRange(v, TeleTrusTNamedCurves.Names);
                CollectionUtilities.AddRange(v, AnssiNamedCurves.Names);
                return v;
            }
        }
    }
}

#endif
