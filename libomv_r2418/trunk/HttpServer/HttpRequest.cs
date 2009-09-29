using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using HttpServer.FormDecoders;
using HttpServer.Exceptions;

namespace HttpServer
{
    /// <summary>
    /// Contains serverside http request information.
    /// </summary>
    internal class HttpRequest : IHttpRequest
    {
        /// <summary>
        /// Chars used to split an url path into multiple parts.
        /// </summary>
        public static readonly char[] UriSplitters = new char[] { '/' };

        private readonly NameValueCollection _headers = new NameValueCollection();
        private readonly HttpParam _param = new HttpParam(HttpInput.Empty, HttpInput.Empty);
        private bool _secure;
        private string[] _acceptTypes;
        private Stream _body = new MemoryStream();
        private int _bodyBytesLeft;
        private ConnectionType _connection = ConnectionType.Close;
        private int _contentLength;
        private RequestCookies _cookies;
        private HttpForm _form = HttpForm.EmptyForm;
        private string _httpVersion = string.Empty;
        private bool _isAjax;
        private string _method = string.Empty;
        private HttpInput _queryString = HttpInput.Empty;
        private Uri _uri = HttpHelper.EmptyUri;
        private string[] _uriParts;
        private string _uriPath;
        private bool _respondTo100Continue = true;
        internal IPEndPoint _remoteEndPoint;

        /// <summary>
        /// Have all body content bytes been received?
        /// </summary>
        public bool BodyIsComplete
        {
            get { return _bodyBytesLeft == 0; }
        }

        /// <summary>
        /// Kind of types accepted by the client.
        /// </summary>
        public string[] AcceptTypes
        {
            get { return _acceptTypes; }
        }

        /// <summary>
        /// Submitted body contents
        /// </summary>
        public Stream Body
        {
            get { return _body; }
            set { _body = value; }
        }

        /// <summary>
        /// Kind of connection used for the session.
        /// </summary>
        public ConnectionType Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Body data encoding, from the Content-Type header
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                Encoding encoding = null;
                string contentType = _headers["content-type"];

                if (contentType != null)
                {
                    string charset = HttpHelper.ParseHeaderAttribute(contentType, "charset");
                    
                    try { encoding = Encoding.GetEncoding(charset); }
                    catch (ArgumentException) { }
                }

                return encoding ?? Encoding.Default;
            }
        }

        /// <summary>
        /// Number of bytes in the body
        /// </summary>
        public int ContentLength
        {
            get { return _contentLength; }
            set
            {
                _contentLength = value;
                _bodyBytesLeft = value;
            }
        }

        /// <summary>
        /// Headers sent by the client. All names are in lower case.
        /// </summary>
        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        /// <summary>
        /// Version of http. 
        /// Probably HttpHelper.HTTP10 or HttpHelper.HTTP11
        /// </summary>
        /// <seealso cref="HttpHelper"/>
        public string HttpVersion
        {
            get { return _httpVersion; }
            set { _httpVersion = value; }
        }

        /// <summary>
        /// Requested method, always upper case.
        /// </summary>
        /// <see cref="Method"/>
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>
        /// Variables sent in the query string
        /// </summary>
        public HttpInput QueryString
        {
            get { return _queryString; }
        }

        /// <summary>
        /// Requested URI (url)
        /// </summary>
        /// <seealso cref="UriPath"/>
        public Uri Uri
        {
            get { return _uri; }
            set
            {
                _uri = value ?? HttpHelper.EmptyUri;
                _uriParts = _uri.AbsolutePath.Split(UriSplitters, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Remote client's IP address and port
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get { return _remoteEndPoint; }
        }

        internal bool ShouldReplyTo100Continue()
        {
            string expectHeader = _headers["expect"];
            if (expectHeader != null && expectHeader.Contains("100-continue"))
            {
                if (_respondTo100Continue)
                {
                    _respondTo100Continue = false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Uri absolute path splitted into parts.
        /// </summary>
        /// <example>
        /// // uri is: http://gauffin.com/code/tiny/
        /// Console.WriteLine(request.UriParts[0]); // result: code
        /// Console.WriteLine(request.UriParts[1]); // result: tiny
        /// </example>
        /// <remarks>
        /// If you're using controllers than the first part is controller name,
        /// the second part is method name and the third part is Id property.
        /// </remarks>
        /// <seealso cref="Uri"/>
        public string[] UriParts
        {
            get { return _uriParts; }
        }

        /// <summary>
        /// Path and query (will be merged with the host header) and put in Uri
        /// </summary>
        /// <see cref="Uri"/>
        public string UriPath
        {
            get { return _uriPath; }
            set
            {
                _uriPath = value;
                int pos = _uriPath.IndexOf('?');
                if (pos != -1)
                {
                    _queryString = HttpHelper.ParseQueryString(_uriPath.Substring(pos + 1));
                    _param.SetQueryString(_queryString);
                    _uriParts = value.Substring(0, pos).Split(UriSplitters, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                    _uriParts = value.Split(UriSplitters, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Check's both QueryString and Form after the parameter.
        /// </summary>
        public HttpParam Param
        {
            get { return _param; }
        }

        /// <summary>
        /// Form parameters.
        /// </summary>
        public HttpForm Form
        {
            get { return _form; }
        }

        /// <summary>
        /// Assign a form.
        /// </summary>
        /// <param name="form"></param>
        internal void AssignForm(HttpForm form)
        {
            _form = form;
        }

        /// <summary>Returns true if the request was made by Ajax (Asyncronous Javascript)</summary>
        public bool IsAjax
        {
            get { return _isAjax; }
        }

        /// <summary>Returns set cookies for the request</summary>
        public RequestCookies Cookies
        {
            get { return _cookies; }
        }

        /// <summary>
        /// Current request is sent over secure protocol
        /// </summary>
        public bool Secure
        {
            get { return _secure; }
            internal set { _secure = value; }
        }

        #region ICloneable Members

        ///<summary>
        ///Creates a new object that is a copy of the current instance.
        ///</summary>
        ///
        ///<returns>
        ///A new object that is a copy of this instance.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public object Clone()
        {
            // this method was mainly created for testing.
            // dont use it that much...
            HttpRequest request = new HttpRequest();
            request.Method = _method;
            if (_acceptTypes != null)
            {
                request._acceptTypes = new string[_acceptTypes.Length];
                _acceptTypes.CopyTo(request._acceptTypes, 0);
            }
            request._httpVersion = _httpVersion;
            request._queryString = _queryString;
            request.Uri = _uri;
            
            byte[] buffer = new byte[_body.Length];
            _body.Read(buffer, 0, (int)_body.Length);
            request.Body = new MemoryStream();
            request.Body.Write(buffer, 0, buffer.Length);
            request.Body.Seek(0, SeekOrigin.Begin);
            request.Body.Flush();

            request._headers.Clear();
            foreach (string key in _headers)
            {
                string[] values = _headers.GetValues(key);
                if (values != null)
                    foreach (string value in values)
                        request.AddHeader(key, value);
            }
            Clear();
            return request;
        }

        #endregion

        /// <summary>
        /// Decode body into a form.
        /// </summary>
        /// <param name="providers">A list with form decoders.</param>
        /// <exception cref="InvalidDataException">If body contents is not valid for the chosen decoder.</exception>
        /// <exception cref="InvalidOperationException">If body is still being transferred.</exception>
        public void DecodeBody(FormDecoderProvider providers)
        {
            if (_bodyBytesLeft > 0)
                throw new InvalidOperationException("Body have not yet been completed.");

            _form = providers.Decode(_headers["content-type"], _body, Encoding.UTF8);
            if (_form != HttpInput.Empty)
                _param.SetForm(_form);
        }

        ///<summary>
        /// Cookies
        ///</summary>
        ///<param name="cookies">the cookies</param>
        public void SetCookies(RequestCookies cookies)
        {
            _cookies = cookies;
        }

        /// <summary>
        /// Called during parsing of a IHttpRequest.
        /// </summary>
        /// <param name="name">Name of the header, should not be url encoded</param>
        /// <param name="value">Value of the header, should not be url encoded</param>
        /// <exception cref="BadRequestException">If a header is incorrect.</exception>
        public void AddHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                throw new BadRequestException("Invalid header name: " + name ?? "<null>");
            if (string.IsNullOrEmpty(value))
                throw new BadRequestException("Header '" + name + "' do not contain a value.");

            switch (name.ToLower())
            {
                case "http_x_requested_with":
                case "x-requested-with":
                    if (string.Compare(value, "XMLHttpRequest", true) == 0)
                        _isAjax = true;
                    break;
                case "accept":
                    _acceptTypes = value.Split(',');
                    for (int i = 0; i < _acceptTypes.Length; ++i)
                        _acceptTypes[i] = _acceptTypes[i].Trim();
                    break;
                case "content-length":
                    int t;
                    if (!int.TryParse(value, out t))
                        throw new BadRequestException("Invalid content length.");
                    ContentLength = t;
                    break; //todo: mayby throw an exception
                case "host":
                    try
                    {
                        _uri = new Uri(Secure ? "https://" : "http://" + value + _uriPath);
                        _uriParts = _uri.AbsolutePath.Split(UriSplitters, StringSplitOptions.RemoveEmptyEntries);
                    }
                    catch (UriFormatException err)
                    {
                        throw new BadRequestException("Failed to parse uri: " + value + _uriPath, err);
                    }
                    break;
                case "connection":
                    if (string.Compare(value, "close", true) == 0)
                        Connection = ConnectionType.Close;
                    else if (value.StartsWith("keep-alive", StringComparison.CurrentCultureIgnoreCase))
                        Connection = ConnectionType.KeepAlive;
                    else
                        throw new BadRequestException("Unknown 'Connection' header type.");
                    break;

                default:
                    _headers.Add(name, value);
                    break;
            }
        }

        /// <summary>
        /// Add bytes to the body
        /// </summary>
        /// <param name="bytes">buffer to read bytes from</param>
        /// <param name="offset">where to start read</param>
        /// <param name="length">number of bytes to read</param>
        /// <returns>Number of bytes actually read (same as length unless we got all body bytes).</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException">If body is not writable</exception>
        public int AddToBody(byte[] bytes, int offset, int length)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");
            if (offset + length > bytes.Length)
                throw new ArgumentOutOfRangeException("offset");
            if (length == 0)
                return 0;
            if (!_body.CanWrite)
                throw new InvalidOperationException("Body is not writable.");

            if (length > _bodyBytesLeft)
            {
                length = _bodyBytesLeft;
            }

            _body.Write(bytes, offset, length);
            _bodyBytesLeft -= length;

            return length;
        }

        /// <summary>
        /// Clear everything in the request
        /// </summary>
        public void Clear()
        {
			_body.Dispose();
        	_body = new MemoryStream();
            _contentLength = 0;
            _method = string.Empty;
            _uri = HttpHelper.EmptyUri;
            _queryString = HttpInput.Empty;
            _bodyBytesLeft = 0;
            _headers.Clear();
            _connection = ConnectionType.Close;
        	_isAjax = false;
            _form.Clear();
        }
    }
}
