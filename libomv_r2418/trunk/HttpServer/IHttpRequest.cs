using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using HttpServer.Exceptions;
using HttpServer.FormDecoders;

namespace HttpServer
{

    /// <summary>
    /// Contains serverside http request information.
    /// </summary>
    public interface IHttpRequest : ICloneable
    {
        /// <summary>
        /// Have all body content bytes been received?
        /// </summary>
        bool BodyIsComplete { get; }

        /// <summary>
        /// Kind of types accepted by the client.
        /// </summary>
        string[] AcceptTypes { get; }

        /// <summary>
        /// Submitted body contents
        /// </summary>
        Stream Body { get; set; }

        /// <summary>
        /// Kind of connection used for the session.
        /// </summary>
        ConnectionType Connection { get; set; }

        /// <summary>
        /// Body data encoding, from the Content-Type header
        /// </summary>
        Encoding ContentEncoding { get; }

        /// <summary>
        /// Number of bytes in the body
        /// </summary>
        int ContentLength { get; set; }

        /// <summary>
        /// Headers sent by the client. All names are in lower case.
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Version of http. 
        /// Probably HttpHelper.HTTP10 or HttpHelper.HTTP11
        /// </summary>
        /// <seealso cref="HttpHelper"/>
        string HttpVersion { get; set; }

        /// <summary>
        /// Requested method, always upper case.
        /// </summary>
        /// <see cref="Method"/>
        string Method { get; set; }

        /// <summary>
        /// Variables sent in the query string
        /// </summary>
        HttpInput QueryString { get; }

        /// <summary>
        /// Requested URI (url)
        /// </summary>
        Uri Uri { get; set; }

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
        string[] UriParts { get; }

        /// <summary>
        /// Check's both QueryString and Form after the parameter.
        /// </summary>
        HttpParam Param { get; }

        /// <summary>
        /// Form parameters.
        /// </summary>
        HttpForm Form { get; }

        /// <summary>Returns true if the request was made by Ajax (Asyncronous Javascript)</summary>
        bool IsAjax { get; }

        /// <summary>Returns set cookies for the request</summary>
        RequestCookies Cookies { get; }

        /// <summary>Returns the requesting client's IP address and port</summary>
        System.Net.IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Decode body into a form.
        /// </summary>
        /// <param name="providers">A list with form decoders.</param>
        /// <exception cref="InvalidDataException">If body contents is not valid for the chosen decoder.</exception>
        /// <exception cref="InvalidOperationException">If body is still being transferred.</exception>
        void DecodeBody(FormDecoderProvider providers);

        /// <summary>
        /// Sets the cookies.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        void SetCookies(RequestCookies cookies);

        /// <summary>
        /// Called during parsing of a IHttpRequest.
        /// </summary>
        /// <param name="name">Name of the header, should not be url encoded</param>
        /// <param name="value">Value of the header, should not be url encoded</param>
        /// <exception cref="BadRequestException">If a header is incorrect.</exception>
        void AddHeader(string name, string value);

        /// <summary>
        /// Add bytes to the body
        /// </summary>
        /// <param name="bytes">buffer to read bytes from</param>
        /// <param name="offset">where to start read</param>
        /// <param name="length">number of bytes to read</param>
        /// <returns>Number of bytes actually read (same as length unless we got all body bytes).</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException">If body is not writable</exception>
        int AddToBody(byte[] bytes, int offset, int length);

        /// <summary>
        /// Clear everything in the request
        /// </summary>
        void Clear();
    }
}