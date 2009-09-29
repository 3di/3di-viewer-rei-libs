using System;
using System.Net;
using System.Text.RegularExpressions;

namespace HttpServer
{
    /// <summary>
    /// Used to match incoming HTTP requests against registered handlers.
    /// Matches based on any combination of HTTP Method, Content-Type header,
    /// and URL path. URL path matching supports the * wildcard character
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{method} {path} Content-Type: {contentType}")]
    public struct HttpRequestSignature : IEquatable<HttpRequestSignature>
    {
        private string method;
        private string contentType;
        private string path;

        /// <summary>HTTP method</summary>
        public string Method
        {
            get { return method; }
            set
            {
                if (!String.IsNullOrEmpty(value)) method = value.ToUpper();
                else method = String.Empty;
            }
        }
        /// <summary>HTTP Content-Type</summary>
        public string ContentType
        {
            get { return contentType; }
            set
            {
                if (!String.IsNullOrEmpty(value)) contentType = value.ToLower();
                else contentType = String.Empty;
            }
        }
        /// <summary>Relative URL path</summary>
        public string Path
        {
            get { return path; }
            set
            {
                if (!String.IsNullOrEmpty(value)) path = value;
                else path = String.Empty;
            }
        }

        /// <summary>
        /// Builds an HttpRequestSignature from an incoming request
        /// </summary>
        /// <param name="request">Incoming request to build the signature from</param>
        public HttpRequestSignature(IHttpRequest request)
        {
            method = contentType = path = String.Empty;

            Method = request.Method;
            ContentType = request.Headers["content-type"];
            Path = request.Uri.PathAndQuery;
        }

        /// <summary>
        /// Test if two HTTP request signatures contain exactly the same data
        /// </summary>
        /// <param name="signature">Signature to test against</param>
        /// <returns>True if the contents of both signatures are identical, 
        /// otherwise false</returns>
        public bool ExactlyEquals(HttpRequestSignature signature)
        {
            return (
                ((method == null && signature.Method == null) || (method != null && method.Equals(signature.Method))) &&
                ((contentType == null && signature.ContentType == null) || (contentType != null && contentType.Equals(signature.ContentType))) &&
                ((path == null && signature.Path == null) || (path != null && path.Equals(signature.Path))));
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. Equals can only be called on an incoming
        /// request; the pattern to match against is the parameter
        /// </summary>
        /// <param name="obj">The pattern to test against this request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public override bool Equals(object obj)
        {
            return (obj is HttpRequestSignature) ? this == (HttpRequestSignature)obj : false;
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. Equals can only be called on an incoming
        /// request; the pattern to match against is the parameter
        /// </summary>
        /// <param name="pattern">The pattern to test against this request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public bool Equals(HttpRequestSignature pattern)
        {
            return (this == pattern);
        }

        /// <summary>
        /// Returns the hash code for this signature
        /// </summary>
        /// <returns>Hash code for this signature</returns>
        public override int GetHashCode()
        {
            int hash = (method != null) ? method.GetHashCode() : 0;
            hash ^= (contentType != null) ? contentType.GetHashCode() : 0;
            hash ^= (path != null) ? path.GetHashCode() : 0;
            return hash;
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. The incoming request must be on the
        /// left-hand side, and the pattern to match against must be on the
        /// right-hand side
        /// </summary>
        /// <param name="request">The incoming HTTP request signature</param>
        /// <param name="pattern">The pattern to test against the incoming request</param>
        /// <returns>True if the request matches the given pattern, otherwise
        /// false</returns>
        public static bool operator ==(HttpRequestSignature request, HttpRequestSignature pattern)
        {
            bool methodMatch = (String.IsNullOrEmpty(pattern.Method) || request.Method.Equals(pattern.Method));
            bool contentTypeMatch = (String.IsNullOrEmpty(pattern.ContentType) || request.ContentType.Equals(pattern.ContentType));
            bool pathMatch = true;

            if (methodMatch && contentTypeMatch && !String.IsNullOrEmpty(pattern.Path))
                pathMatch = Regex.IsMatch(request.Path, pattern.Path, RegexOptions.IgnoreCase);

            return (methodMatch && contentTypeMatch && pathMatch);
        }

        /// <summary>
        /// Does pattern matching to determine if an incoming HTTP request
        /// matches a given pattern. The incoming request must be on the
        /// left-hand side, and the pattern to match against must be on the
        /// right-hand side
        /// </summary>
        /// <param name="request">The incoming HTTP request signature</param>
        /// <param name="pattern">The pattern to test against the incoming request</param>
        /// <returns>True if the request does not match the given pattern, otherwise
        /// false</returns>
        public static bool operator !=(HttpRequestSignature request, HttpRequestSignature pattern)
        {
            return !(request == pattern);
        }
    }
}
