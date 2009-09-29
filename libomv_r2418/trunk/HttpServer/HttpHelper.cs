using System;
using System.Globalization;
using System.Web;

namespace HttpServer
{
    /// <summary>
    /// Generic helper functions for Http
    /// </summary>
    public static class HttpHelper
    {
        /// <summary>
        /// Version string for HTTP v1.0
        /// </summary>
        public const string HTTP10 = "HTTP/1.0";

        /// <summary>
        /// Version string for HTTP v1.1
        /// </summary>
        public const string HTTP11 = "HTTP/1.1";

        /// <summary>
        /// An empty url
        /// </summary>
        public static readonly Uri EmptyUri = new Uri("http://localhost/");

        /// <summary>
        /// Parses a querystring.
        /// </summary>
        /// <param name="queryString">Querystring (url decoded)</param>
        /// <returns>A HttpInput object if successful; otherwise HttpInput.Empty</returns>
        public static HttpInput ParseQueryString(string queryString)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");
            if (queryString == String.Empty)
                return HttpInput.Empty;

            int state = 0;
            int startpos = 0;
            string name = null;
            HttpInput input = new HttpInput("QueryString");
            for (int i = 0; i < queryString.Length; ++i)
            {
                int newIndexPos;
                if (state == 0 && IsEqual(queryString, ref i, out newIndexPos))
                {
                    name = queryString.Substring(startpos, i - startpos);
                    i = newIndexPos;
                    startpos = i + 1;
                    ++state;
                }
                else if (state == 1 && IsAmp(queryString, ref i, out newIndexPos))
                {
                    Add(input, name, queryString.Substring(startpos, i - startpos));
                    i = newIndexPos;
                    startpos = i + 1;
                    state = 0;
                    name = null;
                }
            }

            if (state == 0 && !input.GetEnumerator().MoveNext())
                Add(input, queryString, String.Empty);
            else if (name != null && startpos <= queryString.Length)
                Add(input, name, queryString.Substring(startpos, queryString.Length - startpos));

            return input;
        }

        /// <summary>
        /// Parses an attribute from a header string. For example, the charset attribute
        /// can be parsed from the header Content-Type: text/html; charset=utf-8
        /// </summary>
        /// <param name="header">The raw header string containing the desired attribute</param>
        /// <param name="attribute">The name of the attribute to parse</param>
        /// <returns>The value of the parsed attribute, or null if the parsing failed</returns>
        public static string ParseHeaderAttribute(string header, string attribute)
        {
            if (header == null)
                return null;

            int i;
            int headerLength = header.Length;
            int attrLength = attribute.Length;
            int start = 1;

            while (start < headerLength)
            {
                start = CultureInfo.InvariantCulture.CompareInfo.IndexOf(header, attribute, start, CompareOptions.IgnoreCase);
                
                if ((start < 0) || ((start + attrLength) >= headerLength))
                    break;

                char c = header[start - 1];
                char c2 = header[start + attrLength];

                if ((((c == ';') || (c == ',')) || char.IsWhiteSpace(c)) && ((c2 == '=') || char.IsWhiteSpace(c2)))
                    break;

                start += attrLength;
            }

            if ((start < 0) || (start >= headerLength))
                return null;

            start += attrLength;

            while ((start < headerLength) && char.IsWhiteSpace(header[start]))
                start++;

            if ((start >= headerLength) || (header[start] != '='))
                return null;

            start++;

            while ((start < headerLength) && char.IsWhiteSpace(header[start]))
                start++;

            if (start >= headerLength)
                return null;

            if ((start < headerLength) && (header[start] == '"'))
            {
                if (start == (headerLength - 1))
                    return null;

                i = header.IndexOf('"', start + 1);

                if ((i < 0) || (i == (start + 1)))
                    return null;

                return header.Substring(start + 1, (i - start) - 1).Trim();
            }
            
            i = start;

            while (i < headerLength)
            {
                if ((header[i] == ' ') || (header[i] == ','))
                    break;

                i++;
            }

            if (i == start)
                return null;

            return header.Substring(start, i - start).Trim();
        }

        private static bool IsEqual(string queryStr, ref int index, out int outIndex)
        {
            outIndex = index;
            if (queryStr[index] == '=')
                return true;
            if (queryStr[index] == '%' && queryStr.Length > index + 2 && queryStr[index + 1] == '3'
                && (queryStr[index + 2] == 'd' || queryStr[index + 2] == 'D'))
            {
                outIndex += 2;
                return true;
            }
            return false;
        }

        private static bool IsAmp(string queryStr, ref int index, out int outIndex)
        {
            outIndex = index;
            if (queryStr[index] == '%' && queryStr.Length > index + 2 && queryStr[index + 1] == '2' &&
                queryStr[index + 2] == '6')
                outIndex += 2;
            else if (queryStr[index] == '&')
            {
                if (queryStr.Length > index + 4
                    && (queryStr[index + 1] == 'a' || queryStr[index + 1] == 'A')
                    && (queryStr[index + 2] == 'm' || queryStr[index + 2] == 'M')
                    && (queryStr[index + 3] == 'p' || queryStr[index + 3] == 'P')
                    && queryStr[index + 4] == ';')
                    outIndex += 4;
            }
            else
                return false;

            return true;
        }

        private static void Add(IHttpInput input, string name, string value)
        {
            input.Add(HttpUtility.UrlDecode(name), HttpUtility.UrlDecode(value));
        }
    }
}
