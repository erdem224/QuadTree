﻿namespace Standard_Assets.Core.BestHTTP.Forms
{
    public enum HTTPFormUsage
    {
        /// <summary>
        /// The plugin will try to choose the best form sending method.
        /// </summary>
        Automatic,

        /// <summary>
        /// The plugin will use the Url-Encoded form sending.
        /// </summary>
        UrlEncoded,

        /// <summary>
        /// The plugin will use the Multipart form sending.
        /// </summary>
        Multipart,

        /// <summary>
        /// Using this type of form, the plugin will send the data converted to a JSon string.
        /// </summary>
        RawJSon,

#if !BESTHTTP_DISABLE_UNITY_FORM
        /// <summary>
        /// The legacy, Unity-based form sending.
        /// </summary>
        Unity
#endif
    }
}