#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls
{
    /// <summary>
    /// RFC 5246 7.2
    /// </summary>
    public abstract class AlertLevel
    {
        public const byte warning = 1;
        public const byte fatal = 2;

        public static string GetName(byte alertDescription)
        {
            switch (alertDescription)
            {
            case warning:
                return "warning";
            case fatal:
                return "fatal";
            default:
                return "UNKNOWN";
            }
        }

        public static string GetText(byte alertDescription)
        {
            return GetName(alertDescription) + "(" + alertDescription + ")";
        }
    }
}

#endif
