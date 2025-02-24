﻿#if (!NETFX_CORE && !UNITY_WP8) || UNITY_EDITOR
#endif

#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
#endif

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#endif

#if !BESTHTTP_DISABLE_COOKIES && (!UNITY_WEBGL || UNITY_EDITOR)
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading;
using Standard_Assets.Core.BestHTTP.Authentication;
using Standard_Assets.Core.BestHTTP.Caching;
using Standard_Assets.Core.BestHTTP.Connections;
using Standard_Assets.Core.BestHTTP.Cookies;
using Standard_Assets.Core.BestHTTP.Extensions;
using Standard_Assets.Core.BestHTTP.SecureProtocol.crypto.tls;
using Standard_Assets.Core.BestHTTP.SecureProtocol.security;
#if NETFX_CORE || BUILD_FOR_WP8
    using System.Threading.Tasks;
    using Windows.Networking.Sockets;

    using TcpClient = BestHTTP.PlatformSupport.TcpClient.WinRT.TcpClient;

    //Disable CD4014: Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
    #pragma warning disable 4014
#elif UNITY_WP8 && !UNITY_EDITOR
    using TcpClient = BestHTTP.PlatformSupport.TcpClient.WP8.TcpClient;
#else
    using TcpClient = Standard_Assets.Core.BestHTTP.PlatformSupport.TcpClient.TcpClient;
#endif

namespace Standard_Assets.Core.BestHTTP
{
    /// <summary>
    /// https://tools.ietf.org/html/draft-thomson-hybi-http-timeout-03
    /// Test servers: http://tools.ietf.org/ http://nginx.org/
    /// </summary>
    internal sealed class KeepAliveHeader
    {
        /// <summary>
        /// A host sets the value of the "timeout" parameter to the time that the host will allow an idle connection to remain open before it is closed. A connection is idle if no data is sent or received by a host.
        /// </summary>
        public TimeSpan TimeOut { get; private set; }

        /// <summary>
        /// The "max" parameter has been used to indicate the maximum number of requests that would be made on the connection.This parameter is deprecated.Any limit on requests can be enforced by sending "Connection: close" and closing the connection.
        /// </summary>
        public int MaxRequests { get; private set; }

        public void Parse(List<string> headerValues)
        {
            HeaderParser parser = new HeaderParser(headerValues[0]);
            HeaderValue value;
            if (parser.TryGet("timeout", out value) && value.HasValue)
            {
                int intValue = 0;
                if (int.TryParse(value.Value, out intValue))
                    this.TimeOut = TimeSpan.FromSeconds(intValue);
                else
                    this.TimeOut = TimeSpan.MaxValue;
            }

            if (parser.TryGet("max", out value) && value.HasValue)
            {
                int intValue = 0;
                if (int.TryParse("max", out intValue))
                    this.MaxRequests = intValue;
                else
                    this.MaxRequests = int.MaxValue;
            }
        }
    }

    internal enum RetryCauses
    {
        /// <summary>
        /// The request processed without any special case.
        /// </summary>
        None,

        /// <summary>
        /// If the server closed the connection while we sending a request we should reconnect and send the request again. But we will try it once.
        /// </summary>
        Reconnect,

        /// <summary>
        /// We need an another try with Authorization header set.
        /// </summary>
        Authenticate,

#if !BESTHTTP_DISABLE_PROXY
        /// <summary>
        /// The proxy needs authentication.
        /// </summary>
        ProxyAuthenticate,
#endif
    }

    /// <summary>
    /// Represents and manages a connection to a server.
    /// </summary>
    internal sealed class HTTPConnection : ConnectionBase
    {
        public override bool IsRemovable
        {
            get
            {
                // Plugin's own connection pooling
                if (base.IsRemovable)
                    return true;

                // Overridden keep-alive timeout by a Keep-Alive header
                if (IsFree && KeepAlive != null && (DateTime.UtcNow - base.LastProcessTime) >= KeepAlive.TimeOut)
                    return true;

                return false;
            }
        }

        #region Private Properties

        private TcpClient Client;
        private Stream Stream;
        private KeepAliveHeader KeepAlive;

        #endregion

        internal HTTPConnection(string serverAddress)
            :base(serverAddress)
        {}

        #region Request Processing Implementation

        protected override
#if NETFX_CORE
            async
#endif
            void ThreadFunc(object param)
        {
            bool alreadyReconnected = false;
            bool redirected = false;

            RetryCauses cause = RetryCauses.None;

            try
            {
#if !BESTHTTP_DISABLE_PROXY
                if (!HasProxy && CurrentRequest.HasProxy)
                    Proxy = CurrentRequest.Proxy;
#endif

#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                // Try load the full response from an already saved cache entity. If the response
                if (TryLoadAllFromCache())
                    return;
#endif

                if (Client != null && !Client.IsConnected())
                    Close();

                do // of while (reconnect)
                {
                    if (cause == RetryCauses.Reconnect)
                    {
                        Close();
#if NETFX_CORE
                        await Task.Delay(100);
#else
                        Thread.Sleep(100);
#endif
                    }

                    LastProcessedUri = CurrentRequest.CurrentUri;

                    cause = RetryCauses.None;

                    // Connect to the server
                    Connect();

                    if (State == HTTPConnectionStates.AbortRequested)
                        throw new Exception("AbortRequested");

                    #if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                    // Setup cache control headers before we send out the request
                    if (!CurrentRequest.DisableCache)
                        HTTPCacheService.SetHeaders(CurrentRequest);
                    #endif

                    // Write the request to the stream
                    // sentRequest will be true if the request sent out successfully(no SocketException), so we can try read the response
                    bool sentRequest = false;
                    try
                    {
#if !NETFX_CORE
                         Client.NoDelay = CurrentRequest.TryToMinimizeTCPLatency;
#endif
                         CurrentRequest.SendOutTo(Stream);

                         sentRequest = true;
                    }
                    catch (Exception ex)
                    {
                        Close();

                        if (State == HTTPConnectionStates.TimedOut ||
                            State == HTTPConnectionStates.AbortRequested)
                            throw new Exception("AbortRequested");

                        // We will try again only once
                        if (!alreadyReconnected && !CurrentRequest.DisableRetry)
                        {
                            alreadyReconnected = true;
                            cause = RetryCauses.Reconnect;
                        }
                        else // rethrow exception
                            throw ex;
                    }

                    // If sending out the request succeeded, we will try read the response.
                    if (sentRequest)
                    {
                        bool received = Receive();

                        if (State == HTTPConnectionStates.TimedOut ||
                            State == HTTPConnectionStates.AbortRequested)
                            throw new Exception("AbortRequested");

                        if (!received && !alreadyReconnected && !CurrentRequest.DisableRetry)
                        {
                            alreadyReconnected = true;
                            cause = RetryCauses.Reconnect;
                        }

                        if (CurrentRequest.Response != null)
                        {
#if !BESTHTTP_DISABLE_COOKIES && (!UNITY_WEBGL || UNITY_EDITOR)
                            // Try to store cookies before we do anything else, as we may remove the response deleting the cookies as well.
                            if (CurrentRequest.IsCookiesEnabled)
                                CookieJar.Set(CurrentRequest.Response);
#endif

                            switch (CurrentRequest.Response.StatusCode)
                            {
                                // Not authorized
                                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.2
                                case 401:
                                    {
                                        string authHeader = DigestStore.FindBest(CurrentRequest.Response.GetHeaderValues("www-authenticate"));
                                        if (!string.IsNullOrEmpty(authHeader))
                                        {
                                            var digest = DigestStore.GetOrCreate(CurrentRequest.CurrentUri);
                                            digest.ParseChallange(authHeader);

                                            if (CurrentRequest.Credentials != null && digest.IsUriProtected(CurrentRequest.CurrentUri) && (!CurrentRequest.HasHeader("Authorization") || digest.Stale))
                                                cause = RetryCauses.Authenticate;
                                        }

                                        goto default;
                                    }

#if !BESTHTTP_DISABLE_PROXY
                                // Proxy authentication required
                                // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.8
                                case 407:
                                    {
                                        if (CurrentRequest.HasProxy)
                                        {
                                            string authHeader = DigestStore.FindBest(CurrentRequest.Response.GetHeaderValues("proxy-authenticate"));
                                            if (!string.IsNullOrEmpty(authHeader))
                                            {
                                                var digest = DigestStore.GetOrCreate(CurrentRequest.Proxy.Address);
                                                digest.ParseChallange(authHeader);

                                                if (CurrentRequest.Proxy.Credentials != null && digest.IsUriProtected(CurrentRequest.Proxy.Address) && (!CurrentRequest.HasHeader("Proxy-Authorization") || digest.Stale))
                                                    cause = RetryCauses.ProxyAuthenticate;
                                            }
                                        }

                                        goto default;
                                    }
#endif

                                // Redirected
                                case 301: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.2
                                case 302: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.3
                                case 307: // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.3.8
                                case 308: // http://tools.ietf.org/html/rfc7238
                                    {
                                        if (CurrentRequest.RedirectCount >= CurrentRequest.MaxRedirects)
                                            goto default;
                                        CurrentRequest.RedirectCount++;

                                        string location = CurrentRequest.Response.GetFirstHeaderValue("location");
                                        if (!string.IsNullOrEmpty(location))
                                        {
                                            Uri redirectUri = GetRedirectUri(location);

                                            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                                                HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - Redirected to Location: '{1}' redirectUri: '{1}'", this.CurrentRequest.CurrentUri.ToString(), location, redirectUri));

                                            // Let the user to take some control over the redirection
                                            if (!CurrentRequest.CallOnBeforeRedirection(redirectUri))
                                            {
                                                HTTPManager.Logger.Information("HTTPConnection", "OnBeforeRedirection returned False");
                                                goto default;
                                            }

                                            // Remove the previously set Host header.
                                            CurrentRequest.RemoveHeader("Host");

                                            // Set the Referer header to the last Uri.
                                            CurrentRequest.SetHeader("Referer", CurrentRequest.CurrentUri.ToString());

                                            // Set the new Uri, the CurrentUri will return this while the IsRedirected property is true
                                            CurrentRequest.RedirectUri = redirectUri;

                                            // Discard the redirect response, we don't need it any more
                                            CurrentRequest.Response = null;

                                            redirected = CurrentRequest.IsRedirected = true;
                                        }
                                        else
                                            #if !NETFX_CORE
                                                throw new MissingFieldException(string.Format("Got redirect status({0}) without 'location' header!", CurrentRequest.Response.StatusCode.ToString()));
                                            #else
                                                throw new Exception(string.Format("Got redirect status({0}) without 'location' header!", CurrentRequest.Response.StatusCode.ToString()));
                                            #endif

                                        goto default;
                                    }


                                default:
#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                                    TryStoreInCache();
#endif
                                    break;
                            }
                          
                            // Closing the stream is done manually
                            if (CurrentRequest.Response == null || !CurrentRequest.Response.IsClosedManually) {
                                // If we have a response and the server telling us that it closed the connection after the message sent to us, then
                                //  we will close the connection too.
                                bool closeByServer = CurrentRequest.Response == null || CurrentRequest.Response.HasHeaderWithValue("connection", "close");
                                bool closeByClient = !CurrentRequest.IsKeepAlive;

                                if (closeByServer || closeByClient)
                                    Close();
                                else if (CurrentRequest.Response != null)
                                {
                                    var keepAliveheaderValues = CurrentRequest.Response.GetHeaderValues("keep-alive");
                                    if (keepAliveheaderValues != null && keepAliveheaderValues.Count > 0)
                                    {
                                        if (KeepAlive == null)
                                            KeepAlive = new KeepAliveHeader();
                                        KeepAlive.Parse(keepAliveheaderValues);
                                    }
                                }
                            }
                        }
                    }

                } while (cause != RetryCauses.None);
            }
            catch(TimeoutException e)
            {
                CurrentRequest.Response = null;
                CurrentRequest.Exception = e;
                CurrentRequest.State = HTTPRequestStates.ConnectionTimedOut;

                Close();
            }
            catch (Exception e)
            {
                if (CurrentRequest != null)
                {
#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                    if (CurrentRequest.UseStreaming)
                        HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);
#endif

                    // Something gone bad, Response must be null!
                    CurrentRequest.Response = null;

                    switch (State)
                    {
                        case HTTPConnectionStates.Closed:
                        case HTTPConnectionStates.AbortRequested:
                            CurrentRequest.State = HTTPRequestStates.Aborted;
                            break;
                        case HTTPConnectionStates.TimedOut:
                            CurrentRequest.State = HTTPRequestStates.TimedOut;
                            break;
                        default:
                            CurrentRequest.Exception = e;
                            CurrentRequest.State = HTTPRequestStates.Error;
                            break;
                    }
                }

                Close();
            }
            finally
            {
                if (CurrentRequest != null)
                {
                    // Avoid state changes. While we are in this block changing the connection's State, on Unity's main thread
                    //  the HTTPManager's OnUpdate will check the connections's State and call functions that can change the inner state of
                    //  the object. (Like setting the CurrentRequest to null in function Recycle() causing a NullRef exception)
                    lock (HTTPManager.Locker)
                    {
                        if (CurrentRequest != null && CurrentRequest.Response != null && CurrentRequest.Response.IsUpgraded)
                            State = HTTPConnectionStates.Upgraded;
                        else
                            State = redirected ? HTTPConnectionStates.Redirected : (Client == null ? HTTPConnectionStates.Closed : HTTPConnectionStates.WaitForRecycle);

                        // Change the request's state only when the whole processing finished
                        if (CurrentRequest.State == HTTPRequestStates.Processing && (State == HTTPConnectionStates.Closed || State == HTTPConnectionStates.WaitForRecycle))
                        {
                            if (CurrentRequest.Response != null)
                                CurrentRequest.State = HTTPRequestStates.Finished;
                            else
                            {
                                CurrentRequest.Exception = new Exception(string.Format("Remote server closed the connection before sending response header! Previous request state: {0}. Connection state: {1}",
                                        CurrentRequest.State.ToString(),
                                        State.ToString()));
                                CurrentRequest.State = HTTPRequestStates.Error;
                            }
                        }

                        if (CurrentRequest.State == HTTPRequestStates.ConnectionTimedOut)
                            State = HTTPConnectionStates.Closed;

                        LastProcessTime = DateTime.UtcNow;

                        if (OnConnectionRecycled != null)
                            RecycleNow();
                    }

                    #if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                    HTTPCacheService.SaveLibrary();
                    #endif

                    #if !BESTHTTP_DISABLE_COOKIES && (!UNITY_WEBGL || UNITY_EDITOR)
                    CookieJar.Persist();
                    #endif
                }
            }
        }

        private void Connect()
        {
            Uri uri =
#if !BESTHTTP_DISABLE_PROXY
                CurrentRequest.HasProxy ? CurrentRequest.Proxy.Address :
#endif
                CurrentRequest.CurrentUri;

            #region TCP Connection

            if (Client == null)
                Client = new TcpClient();

            if (!Client.Connected)
            {
                Client.ConnectTimeout = CurrentRequest.ConnectTimeout;

#if NETFX_CORE || (UNITY_WP8 && !UNITY_EDITOR)
                Client.UseHTTPSProtocol =
                #if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
                    !CurrentRequest.UseAlternateSSL &&
                #endif
                    HTTPProtocolFactory.IsSecureProtocol(uri);
#endif

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("HTTPConnection", string.Format("'{0}' - Connecting to {1}:{2}", this.CurrentRequest.CurrentUri.ToString(), uri.Host, uri.Port.ToString()));

                Client.Connect(uri.Host, uri.Port);

                if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                    HTTPManager.Logger.Information("HTTPConnection", "Connected to " + uri.Host + ":" + uri.Port.ToString());
            }
            else if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                    HTTPManager.Logger.Information("HTTPConnection", "Already connected to " + uri.Host + ":" + uri.Port.ToString());

            #endregion

            StartTime = DateTime.UtcNow;

            if (Stream == null)
            {
                bool isSecure = HTTPProtocolFactory.IsSecureProtocol(CurrentRequest.CurrentUri);

                Stream = Client.GetStream();
                /*if (Stream.CanTimeout)
                    Stream.ReadTimeout = Stream.WriteTimeout = (int)CurrentRequest.Timeout.TotalMilliseconds;*/


#if !BESTHTTP_DISABLE_PROXY
                #region Proxy Handling

                if (HasProxy && (!Proxy.IsTransparent || (isSecure && Proxy.NonTransparentForHTTPS)))
                {
                    var outStream = new BinaryWriter(Stream);

                    bool retry;
                    do
                    {
                        // If we have to because of a authentication request, we will switch it to true
                        retry = false;

                        string connectStr = string.Format("CONNECT {0}:{1} HTTP/1.1", CurrentRequest.CurrentUri.Host, CurrentRequest.CurrentUri.Port);

                        HTTPManager.Logger.Information("HTTPConnection", "Sending " + connectStr);

                        outStream.SendAsASCII(connectStr);
                        outStream.Write(HTTPRequest.EOL);

                        outStream.SendAsASCII("Proxy-Connection: Keep-Alive");
                        outStream.Write(HTTPRequest.EOL);

                        outStream.SendAsASCII("Connection: Keep-Alive");
                        outStream.Write(HTTPRequest.EOL);

                        outStream.SendAsASCII(string.Format("Host: {0}:{1}", CurrentRequest.CurrentUri.Host, CurrentRequest.CurrentUri.Port));
                        outStream.Write(HTTPRequest.EOL);

                        // Proxy Authentication
                        if (HasProxy && Proxy.Credentials != null)
                        {
                            switch (Proxy.Credentials.Type)
                            {
                                case AuthenticationTypes.Basic:
                                    // With Basic authentication we don't want to wait for a challenge, we will send the hash with the first request
                                    outStream.Write(string.Format("Proxy-Authorization: {0}", string.Concat("Basic ", Convert.ToBase64String(Encoding.UTF8.GetBytes(Proxy.Credentials.UserName + ":" + Proxy.Credentials.Password)))).GetASCIIBytes());
                                    outStream.Write(HTTPRequest.EOL);
                                    break;

                                case AuthenticationTypes.Unknown:
                                case AuthenticationTypes.Digest:
                                    var digest = DigestStore.Get(Proxy.Address);
                                    if (digest != null)
                                    {
                                        string authentication = digest.GenerateResponseHeader(CurrentRequest, Proxy.Credentials);
                                        if (!string.IsNullOrEmpty(authentication))
                                        {
                                            outStream.Write(string.Format("Proxy-Authorization: {0}", authentication).GetASCIIBytes());
                                            outStream.Write(HTTPRequest.EOL);
                                        }
                                    }

                                    break;
                            }
                        }

                        outStream.Write(HTTPRequest.EOL);

                        // Make sure to send all the wrote data to the wire
                        outStream.Flush();

                        CurrentRequest.ProxyResponse = new HTTPResponse(CurrentRequest, Stream, false, false);

                        // Read back the response of the proxy
                        if (!CurrentRequest.ProxyResponse.Receive(-1, true))
                            throw new Exception("Connection to the Proxy Server failed!");

                        if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                            HTTPManager.Logger.Information("HTTPConnection", "Proxy returned - status code: " + CurrentRequest.ProxyResponse.StatusCode + " message: " + CurrentRequest.ProxyResponse.Message);

                        switch(CurrentRequest.ProxyResponse.StatusCode)
                        {
                            // Proxy authentication required
                            // http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.8
                            case 407:
                            {
                                string authHeader = DigestStore.FindBest(CurrentRequest.ProxyResponse.GetHeaderValues("proxy-authenticate"));
                                if (!string.IsNullOrEmpty(authHeader))
                                {
                                    var digest = DigestStore.GetOrCreate(Proxy.Address);
                                    digest.ParseChallange(authHeader);

                                    if (Proxy.Credentials != null && digest.IsUriProtected(Proxy.Address) && (!CurrentRequest.HasHeader("Proxy-Authorization") || digest.Stale))
                                        retry = true;
                                }
                                break;
                            }

                            default:
                                if (!CurrentRequest.ProxyResponse.IsSuccess)
                                    throw new Exception(string.Format("Proxy returned Status Code: \"{0}\", Message: \"{1}\" and Response: {2}", CurrentRequest.ProxyResponse.StatusCode, CurrentRequest.ProxyResponse.Message, CurrentRequest.ProxyResponse.DataAsText));
                                break;
                        }

                    } while (retry);
                }
                #endregion
#endif // #if !BESTHTTP_DISABLE_PROXY

                // We have to use CurrentRequest.CurrentUri here, because uri can be a proxy uri with a different protocol
                if (isSecure)
                {
                    #region SSL Upgrade

#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
                    if (CurrentRequest.UseAlternateSSL)
                    {
                        var handler = new TlsClientProtocol(Client.GetStream(), new SecureRandom());

                        // http://tools.ietf.org/html/rfc3546#section-3.1
                        // It is RECOMMENDED that clients include an extension of type "server_name" in the client hello whenever they locate a server by a supported name type.
                        List<string> hostNames = new List<string>(1);
                        hostNames.Add(CurrentRequest.CurrentUri.Host);

                        handler.Connect(new LegacyTlsClient(CurrentRequest.CurrentUri,
                                                            CurrentRequest.CustomCertificateVerifyer == null ? new AlwaysValidVerifyer() : CurrentRequest.CustomCertificateVerifyer,
                                                            CurrentRequest.CustomClientCredentialsProvider,
                                                            hostNames));

                        Stream = handler.Stream;
                    }
                    else
#endif
                    {
#if !NETFX_CORE && !UNITY_WP8
                        SslStream sslStream = new SslStream(Client.GetStream(), false, (sender, cert, chain, errors) =>
                        {
                            return CurrentRequest.CallCustomCertificationValidator(cert, chain);
                        });

                        if (!sslStream.IsAuthenticated)
                            sslStream.AuthenticateAsClient(CurrentRequest.CurrentUri.Host);
                        Stream = sslStream;
#else
                        Stream = Client.GetStream();
#endif
                    }

                    #endregion
                }
            }
        }

        private bool Receive()
        {
            SupportedProtocols protocol = CurrentRequest.ProtocolHandler == SupportedProtocols.Unknown ? HTTPProtocolFactory.GetProtocolFromUri(CurrentRequest.CurrentUri) : CurrentRequest.ProtocolHandler;

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - Receive - protocol: {1}", this.CurrentRequest.CurrentUri.ToString(), protocol.ToString()));

            CurrentRequest.Response = HTTPProtocolFactory.Get(protocol, CurrentRequest, Stream, CurrentRequest.UseStreaming, false);

            if (!CurrentRequest.Response.Receive())
            {
                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - Receive - Failed! Response will be null, returning with false.", this.CurrentRequest.CurrentUri.ToString()));
                CurrentRequest.Response = null;
                return false;
            }

            if (CurrentRequest.Response.StatusCode == 304
#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                && !CurrentRequest.DisableCache
#endif
                )
            {
#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
                if (CurrentRequest.IsRedirected)
                {
                    if (!LoadFromCache(CurrentRequest.RedirectUri))
                        LoadFromCache(CurrentRequest.Uri);
                }
                else
                    LoadFromCache(CurrentRequest.Uri);
#else
                return false;
#endif
            }

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - Receive - Finished Successfully!", this.CurrentRequest.CurrentUri.ToString()));

            return true;
        }

        #endregion

        #region Helper Functions

#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)

        private bool LoadFromCache(Uri uri)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - LoadFromCache for Uri: {1}", this.CurrentRequest.CurrentUri.ToString(), uri.ToString()));

            var cacheEntity = HTTPCacheService.GetEntity(uri);
            if (cacheEntity == null)
            {
                HTTPManager.Logger.Warning("HTTPConnection", string.Format("{0} - LoadFromCache for Uri: {1} - Cached entity not found!", this.CurrentRequest.CurrentUri.ToString(), uri.ToString()));
                return false;
            }

            CurrentRequest.Response.CacheFileInfo = cacheEntity;

            int bodyLength;
            using (var cacheStream = cacheEntity.GetBodyStream(out bodyLength))
            {
                if (cacheStream == null)
                    return false;

                if (!CurrentRequest.Response.HasHeader("content-length"))
                    CurrentRequest.Response.Headers.Add("content-length", new List<string>(1) { bodyLength.ToString() });
                CurrentRequest.Response.IsFromCache = true;

                if (!CurrentRequest.CacheOnly)
                    CurrentRequest.Response.ReadRaw(cacheStream, bodyLength);
            }

            return true;
        }

        private bool TryLoadAllFromCache()
        {
            if (CurrentRequest.DisableCache || !HTTPCacheService.IsSupported)
                return false;

            // We will try read the response from the cache, but if something happens we will fallback to the normal way.
            try
            {
                //Unless specifically constrained by a cache-control (section 14.9) directive, a caching system MAY always store a successful response (see section 13.8) as a cache entity,
                //  MAY return it without validation if it is fresh, and MAY    return it after successful validation.
                // MAY return it without validation if it is fresh!
                if (HTTPCacheService.IsCachedEntityExpiresInTheFuture(CurrentRequest))
                {
                    if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                        HTTPManager.Logger.Verbose("HTTPConnection", string.Format("{0} - TryLoadAllFromCache - whole response loading from cache", this.CurrentRequest.CurrentUri.ToString()));

                    CurrentRequest.Response = HTTPCacheService.GetFullResponse(CurrentRequest);

                    if (CurrentRequest.Response != null)
                        return true;
                }
            }
            catch
            {
                HTTPCacheService.DeleteEntity(CurrentRequest.CurrentUri);
            }

            return false;
        }
#endif

#if !BESTHTTP_DISABLE_CACHING && (!UNITY_WEBGL || UNITY_EDITOR)
        private void TryStoreInCache()
        {
            // if UseStreaming && !DisableCache then we already wrote the response to the cache
            if (!CurrentRequest.UseStreaming &&
                !CurrentRequest.DisableCache &&
                CurrentRequest.Response != null &&
                HTTPCacheService.IsSupported &&
                HTTPCacheService.IsCacheble(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response))
            {
                if(CurrentRequest.IsRedirected)
                    HTTPCacheService.Store(CurrentRequest.Uri, CurrentRequest.MethodType, CurrentRequest.Response);
                else
                    HTTPCacheService.Store(CurrentRequest.CurrentUri, CurrentRequest.MethodType, CurrentRequest.Response);
            }
        }
#endif

        private Uri GetRedirectUri(string location)
        {
            Uri result = null;
            try
            {
                result = new Uri(location);

                if (result.IsFile || result.AbsolutePath == location)
                    result = null;
            }
#if !NETFX_CORE
            catch (UriFormatException)
#else
            catch
#endif
            {
                // Sometimes the server sends back only the path and query component of the new uri
                result = null;
            }

            if (result == null)
            {
                var uri = CurrentRequest.Uri;
                var builder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, location);
                result = builder.Uri;

            }

            return result;
        }

        internal override void Abort(HTTPConnectionStates newState)
        {
            State = newState;

            switch(State)
            {
                case HTTPConnectionStates.TimedOut: TimedOutStart = DateTime.UtcNow; break;
            }

            if (Stream != null)
                Stream.Dispose();
        }

        private void Close()
        {
            KeepAlive = null;
            LastProcessedUri = null;
            if (Client != null)
            {
                try
                {
                    Client.Close();
                }
                catch
                {

                }
                finally
                {
                    Stream = null;
                    Client = null;
                }
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            Close();

            base.Dispose(disposing);
        }
    }
}