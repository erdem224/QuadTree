#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using System;
using Standard_Assets.Core.BestHTTP.SecureProtocol.util.date;
//#if NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE
//#endif

#if UNITY_WSA && !UNITY_EDITOR && !ENABLE_IL2CPP
using System.TypeFix;
#endif

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.util
{
    internal abstract class Enums
    {
        internal static Enum GetEnumValue(System.Type enumType, string s)
        {
            /*if (!enumType.IsEnum)
                throw new ArgumentException("Not an enumeration type", "enumType");*/

            // We only want to parse single named constants
            if (s.Length > 0 && char.IsLetter(s[0]) && s.IndexOf(',') < 0)
            {
                s = s.Replace('-', '_');
                s = s.Replace('/', '_');

#if NETCF_1_0
                FieldInfo field = enumType.GetField(s, BindingFlags.Static | BindingFlags.Public);
                if (field != null)
                {
                    return (Enum)field.GetValue(null);
                }
#else
                return (Enum)Enum.Parse(enumType, s, false);
#endif
            }

            throw new ArgumentException();
        }

        internal static Array GetEnumValues(System.Type enumType)
        {
            /*if (!enumType.IsEnum)
                throw new ArgumentException("Not an enumeration type", "enumType");*/

#if NETCF_1_0 || NETCF_2_0 || SILVERLIGHT
            IList result = Platform.CreateArrayList();
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                // Note: Argument to GetValue() ignored since the fields are static,
                //     but Silverlight for Windows Phone throws exception if we pass null
                result.Add(field.GetValue(enumType));
            }
            object[] arr = new object[result.Count];
            result.CopyTo(arr, 0);
            return arr;
#else
            return Enum.GetValues(enumType);
#endif
        }

        internal static Enum GetArbitraryValue(System.Type enumType)
        {
            Array values = GetEnumValues(enumType);
            int pos = (int)(DateTimeUtilities.CurrentUnixMs() & int.MaxValue) % values.Length;
            return (Enum)values.GetValue(pos);
        }

        internal static bool IsEnumType(System.Type t)
        {
#if NEW_REFLECTION || NETFX_CORE
            return t.GetTypeInfo().IsEnum;
#else
            return t.IsEnum;
#endif
        }
    }
}

#endif
