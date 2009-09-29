using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace HttpServer
{
    /// <summary>
    /// HTTP Listener waits for HTTP connections and provide us with <see cref="HttpListenerContext"/>s using the
    /// <see cref="RequestHandler"/> delegate.
    /// </summary>
    public class HttpListener
    {
        private readonly IPAddress _address;
        private readonly X509Certificate _certificate;
        private readonly X509Certificate _rootCA;
        private readonly bool _requireClientCerts;
        private readonly int _port;
        private readonly SslProtocols _sslProtocol = SslProtocols.Tls;
        private ClientDisconnectedHandler _disconnectHandler;
        private TcpListener _listener;
        private ILogWriter _logWriter = NullLogWriter.Instance;
        private RequestReceivedHandler _requestHandler;
        private bool _useTraceLogs;
        private int _pendingAccepts;
        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private bool _shutdown;

        /// <summary>
        /// A client has been accepted, but not handled, by the listener.
        /// </summary>
        public event EventHandler<ClientAcceptedEventArgs> Accepted = delegate{};

        /// <summary>
        /// Listen for regular HTTP connections
        /// </summary>
        /// <param name="address">IP Address to accept connections on</param>
        /// <param name="port">TCP Port to listen on, default HTTP port is 80.</param>
        /// <exception cref="ArgumentNullException"><c>address</c> is null.</exception>
        /// <exception cref="ArgumentException">Port must be a positive number.</exception>
        public HttpListener(IPAddress address, int port)
        {
            Check.Require(address, "address");
            Check.Between(1, UInt16.MaxValue, port, "port");

            _address = address;
            _port = port;
        }

        /// <summary>
        /// Launch HttpListener in SSL mode
        /// </summary>
        /// <param name="address">IP Address to accept connections on</param>
        /// <param name="port">TCP Port to listen on, default HTTPS port is 443</param>
        /// <param name="certificate">Certificate to use</param>
        /// <param name="rootCA">Certificate for the root certificate authority that signed the server
        /// certificate and/or any client certificates</param>
        /// <param name="protocol">which HTTPS protocol to use, default is TLS.</param>
        /// <param name="requireClientCerts">True to require client SSL certificates, otherwise false</param>
        public HttpListener(IPAddress address, int port, X509Certificate certificate, X509Certificate rootCA,
            SslProtocols protocol, bool requireClientCerts)
            : this(address, port)
        {
            Check.Require(certificate, "certificate");
            Check.Require(rootCA, "rootCA");

            _certificate = certificate;
            _rootCA = rootCA;
            _sslProtocol = protocol;
            _requireClientCerts = requireClientCerts;
        }

        /// <summary>
        /// Invoked when a client disconnects
        /// </summary>
        public ClientDisconnectedHandler DisconnectHandler
        {
            get { return _disconnectHandler; }
            set { _disconnectHandler = value; }
        }

        /// <summary>
        /// Gives you a chance to receive log entries for all internals of the HTTP library.
        /// </summary>
        /// <remarks>
        /// You may not switch log writer after starting the listener.
        /// </remarks>
        public ILogWriter LogWriter
        {
            get { return _logWriter; }
            set
            {
                _logWriter = value ?? NullLogWriter.Instance;
                if (_certificate != null)
                    _logWriter.Write(this, LogPrio.Info, String.Format("HTTPS({0}) listening on {1}:{2}",
                        _sslProtocol, _address, _port));
                else
                    _logWriter.Write(this, LogPrio.Info, String.Format("HTTP listening on {0}:{1}",
                        _address, _port));
            }
        }

        /// <summary>
        /// This handler will be invoked each time a new connection is accepted.
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>value</c> is null.</exception>
        public RequestReceivedHandler RequestHandler
        {
            get { return _requestHandler; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _requestHandler = value;
            }
        }

        /// <summary>
        /// True if we should turn on trace logs.
        /// </summary>
        public bool UseTraceLogs
        {
            get { return _useTraceLogs; }
            set { _useTraceLogs = value; }
        }


        /// <exception cref="Exception"><c>Exception</c>.</exception>
        private void OnAccept(IAsyncResult ar)
        {
            Socket socket = null;

            try
            {
                int count = Interlocked.Decrement(ref _pendingAccepts);
                if (_shutdown)
                {
                     if (count == 0)
                         _shutdownEvent.Set();
                    return;
                }

                Interlocked.Increment(ref _pendingAccepts);
                _listener.BeginAcceptSocket(OnAccept, null);
                socket = _listener.EndAcceptSocket(ar);

                ClientAcceptedEventArgs args = new ClientAcceptedEventArgs(socket);
                Accepted(this, args);
                if (args.Revoked)
                {
                    _logWriter.Write(this, LogPrio.Debug, "Socket was revoked by event handler.");
                    socket.Close();
                    return;
                }

                //_logWriter.Write(this, LogPrio.Debug, "Accepted connection from: " + socket.RemoteEndPoint);

                NetworkStream stream = new NetworkStream(socket, true);
                IPEndPoint remoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;

                if (_certificate != null)
                    CreateSecureContext(stream, remoteEndPoint);
                else
                    new HttpClientContextImp(false, remoteEndPoint, _requestHandler, _disconnectHandler, stream,
                                             LogWriter);
            }
            catch (Exception err)
            {
                // we can't really do anything but close the connection
                if (socket != null)
                    socket.Close();

                if (ExceptionThrown == null)
#if DEBUG
                    throw;
#else
                   _logWriter.Write(this, LogPrio.Error, err.Message);
#endif

                if (ExceptionThrown != null)
                    ExceptionThrown(this, err);
            }
        }

        private bool ValidateClientCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            // Search through the certificate chain looking for our root CA
            for (int i = 0; i < chain.ChainElements.Count; i++)
            {
                if (chain.ChainElements[i].Certificate.Equals(_rootCA))
                    return true;
            }

            return false;
        }

        private void CreateSecureContext(Stream stream, IPEndPoint remoteEndPoint)
        {
            SslStream sslStream = new SslStream(stream, false, ValidateClientCertificate);
            try
            {
                sslStream.AuthenticateAsServer(_certificate, _requireClientCerts, _sslProtocol, false); //todo: this may fail
                new HttpClientContextImp(true, remoteEndPoint, _requestHandler, _disconnectHandler, sslStream, LogWriter);
            }
            catch (IOException err)
            {
                if (UseTraceLogs)
                    _logWriter.Write(this, LogPrio.Trace, err.Message);
            }
            catch (ObjectDisposedException err)
            {
                if (UseTraceLogs)
                    _logWriter.Write(this, LogPrio.Trace, err.Message);
            }
        }

        /// <summary>
        /// Start listen for new connections
        /// </summary>
        /// <param name="backlog">Number of connections that can stand in a queue to be accepted.</param>
        /// <exception cref="InvalidOperationException">If <see cref="RequestHandler"/> have not been set.</exception>
        public void Start(int backlog)
        {
            if (_requestHandler == null)
                throw new InvalidOperationException("RequestHandler must be set before starting listener.");

            if (_listener == null)
            {
                _listener = new TcpListener(_address, _port);
                _listener.Start(backlog);
            }
            Interlocked.Increment(ref _pendingAccepts);
            _listener.BeginAcceptSocket(OnAccept, null);
        }

        /// <summary>
        /// Stop the listener
        /// </summary>
        /// <exception cref="SocketException"></exception>
        public void Stop()
        {
            _shutdown = true;
            _listener.Stop();
            if (!_shutdownEvent.WaitOne())
                _logWriter.Write(this, LogPrio.Error, "Failed to shutdown listener properly.");
            _listener = null;
        }

        /// <summary>
        /// Receives unhandled exceptions from the listening threads.
        /// </summary>
        /// <remarks>
        /// Exceptions will be thrown during debug mode if this event is not used,
        /// exceptions will be printed to console and suppressed during release mode.
        /// </remarks>
        public event ExceptionHandler ExceptionThrown;
    }
}