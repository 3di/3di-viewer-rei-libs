using System;

namespace HttpServer
{
    /// <summary>
    /// Contains a signature pattern (for matching against incoming
    /// requests) and a callback for handling the request
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Signature}")]
    public struct HttpRequestHandler : IEquatable<HttpRequestHandler>
    {
        /// <summary>Signature pattern to match against incoming requests</summary>
        public HttpRequestSignature Signature;
        /// <summary>Callback for handling requests that match the signature</summary>
        public HttpRequestCallback Callback;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="signature">Signature pattern for matching against incoming requests</param>
        /// <param name="callback">Callback for handling the request</param>
        public HttpRequestHandler(HttpRequestSignature signature, HttpRequestCallback callback)
        {
            Signature = signature;
            Callback = callback;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="obj">Object to compare against for equality</param>
        /// <returns>True if the given object is equal to this object, otherwise false</returns>
        public override bool Equals(object obj)
        {
            return (obj is HttpRequestHandler) ? this.Signature == ((HttpRequestHandler)obj).Signature : false;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="handler">Object to compare against for equality</param>
        /// <returns>True if the given object is equal to this object, otherwise false</returns>
        public bool Equals(HttpRequestHandler handler)
        {
            return this.Signature == handler.Signature;
        }

        /// <summary>
        /// Returns the hash code for the signature in this handler
        /// </summary>
        /// <returns>The hash code for the signature in this handler</returns>
        public override int GetHashCode()
        {
            return Signature.GetHashCode();
        }
    }
}
