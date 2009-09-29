using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using HttpListener = HttpServer.HttpListener;

namespace HttpServer
{
    /// <summary>
    /// Delegate for handling incoming HTTP requests
    /// </summary>
    /// <param name="context">Client context</param>
    /// <param name="request">HTTP request</param>
    /// <param name="response">HTTP response</param>
    /// <returns>True to send the response and close the connection, false to leave the connection open</returns>
    public delegate bool HttpRequestCallback(IHttpClientContext context, IHttpRequest request, IHttpResponse response);

    /// <summary>
    /// HTTP server with regular expression path matching, client and server SSL
    /// certificates, and a simplified request callback
    /// </summary>
    public class WebServer
    {
        const int BACKLOG_SIZE = 100;

        HttpListener server = null;
        HttpRequestHandler[] requestHandlers = new HttpRequestHandler[0];
        HttpRequestCallback notFoundHandler;
        ILogWriter _logWriter = NullLogWriter.Instance;

        /// <summary>
        /// Sets or retrieves the current logging engine for the WebServer and
        /// HttpListener internals
        /// </summary>
        /// <remarks>Do not set the LogWriter while the WebServer is running</remarks>
        public ILogWriter LogWriter
        {
            get { return _logWriter; }
            set
            {
                _logWriter = value ?? NullLogWriter.Instance;
                server.LogWriter = value;
            }
        }

        /// <summary>
        /// Initialize the server in HTTP mode
        /// </summary>
        /// <param name="address">IP address to bind to</param>
        /// <param name="port">Port number to bind to</param>
        public WebServer(IPAddress address, int port)
        {
            server = new HttpListener(address, port);
            server.RequestHandler += RequestHandler;
        }

        /// <summary>
        /// Initialize the server in HTTPS (TLS 1.0) mode
        /// </summary>
        /// <param name="address">IP address to bind to</param>
        /// <param name="port">Port number to bind to</param>
        /// <param name="certificate">X.509 server certificate for SSL</param>
        /// <param name="rootCA">X.509 certificate for the root certificate authority
        /// that signed the server certificate and/or any client certificates</param>
        /// <param name="requireClientCerts">True if client SSL certificates are
        /// required, otherwise false</param>
        public WebServer(IPAddress address, int port, X509Certificate certificate, X509Certificate rootCA, bool requireClientCerts)
        {
            server = new HttpListener(address, port, certificate, rootCA, System.Security.Authentication.SslProtocols.Tls, requireClientCerts);
            server.RequestHandler += RequestHandler;
        }

        /// <summary>
        /// Add a request handler
        /// </summary>
        /// <param name="method">HTTP verb to match, or null to skip verb matching</param>
        /// <param name="contentType">Content-Type header to match, or null to skip Content-Type matching</param>
        /// <param name="path">Request URI path regular expression to match, or null to skip URI path matching</param>
        /// <param name="callback">Callback to fire when an incoming request matches the given pattern</param>
        public void AddHandler(string method, string contentType, string path, HttpRequestCallback callback)
        {
            HttpRequestSignature signature = new HttpRequestSignature();
            signature.Method = method;
            signature.ContentType = contentType;
            signature.Path = path;
            AddHandler(new HttpRequestHandler(signature, callback));
        }

        /// <summary>
        /// Add a request handler
        /// </summary>
        /// <param name="handler">Request handler to add</param>
        public void AddHandler(HttpRequestHandler handler)
        {
            HttpRequestHandler[] newHandlers = new HttpRequestHandler[requestHandlers.Length + 1];

            for (int i = 0; i < requestHandlers.Length; i++)
                newHandlers[i] = requestHandlers[i];
            newHandlers[requestHandlers.Length] = handler;

            // CLR guarantees this is an atomic operation
            requestHandlers = newHandlers;
        }

        /// <summary>
        /// Remove a request handler
        /// </summary>
        /// <param name="handler">Request handler to remove</param>
        public void RemoveHandler(HttpRequestHandler handler)
        {
            HttpRequestHandler[] newHandlers = new HttpRequestHandler[requestHandlers.Length - 1];

            int j = 0;
            for (int i = 0; i < requestHandlers.Length; i++)
                if (!requestHandlers[i].Signature.ExactlyEquals(handler.Signature))
                    newHandlers[j++] = handler;

            // CLR guarantees this is an atomic operation
            requestHandlers = newHandlers;
        }

        /// <summary>
        /// Set a callback to override the default 404 (Not Found) response
        /// </summary>
        /// <param name="callback">Callback that will be fired when an unhandled
        /// request is received, or null to reset to the default handler</param>
        public void Set404Handler(HttpRequestCallback callback)
        {
            notFoundHandler = callback;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            server.Start(BACKLOG_SIZE);
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            try { server.Stop(); }
            catch (SocketException ex)
            {
                _logWriter.Write(this, LogPrio.Error, ex.Message);
            }
        }

        void RequestHandler(IHttpClientContext client, IHttpRequest request)
        {
            HttpResponse response = new HttpResponse(client, request);

            // Create a request signature
            HttpRequestSignature signature = new HttpRequestSignature(request);

            // Look for a signature match in our handlers
            for (int i = 0; i < requestHandlers.Length; i++)
            {
                HttpRequestHandler handler = requestHandlers[i];

                if (handler.Signature != null && signature == handler.Signature)
                {
                    FireRequestCallback(client, request, response, handler.Callback);
                    return;
                }
            }

            // No registered handler matched this request's signature
            if (notFoundHandler != null)
            {
                FireRequestCallback(client, request, response, notFoundHandler);
            }
            else
            {
                // Send a default 404 response
                try
                {
                    response.Status = HttpStatusCode.NotFound;
                    response.Reason = String.Format("No request handler registered for Method=\"{0}\", Content-Type=\"{1}\", Path=\"{2}\"",
                        signature.Method, signature.ContentType, signature.Path);
                    string notFoundResponse = "<html><head><title>Page Not Found</title></head><body><h3>" + response.Reason + "</h3></body></html>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(notFoundResponse);
                    response.Body.Write(buffer, 0, buffer.Length);
                    response.Send();
                }
                catch (Exception) { }
            }
        }

        void FireRequestCallback(IHttpClientContext client, IHttpRequest request, IHttpResponse response, HttpRequestCallback callback)
        {
            bool closeConnection = true;

            try { closeConnection = callback(client, request, response); }
            catch (Exception ex) { _logWriter.Write(this, LogPrio.Error, "Exception in HTTP handler: " + ex.Message); }

            if (closeConnection)
            {
                try { response.Send(); }
                catch (Exception ex)
                {
                    _logWriter.Write(this, LogPrio.Error, String.Format("Failed to send HTTP response for request to {0}: {1}",
                        request.Uri, ex.Message));
                }
            }
        }
    }
}
