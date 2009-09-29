using System;
using System.IO;
using System.Text;
using HttpServer.Exceptions;

namespace HttpServer
{
    /// <summary>
    /// Parses a HTTP request directly from a stream
    /// </summary>
    public class HttpRequestParser
    {
        #region Delegates

        /// <summary>
        /// Invoked when a request have been completed.
        /// </summary>
        /// <param name="request"></param>
        public delegate void RequestCompletedHandler(IHttpRequest request);

        #endregion

        private readonly ILogWriter _log;
        private readonly RequestCompletedHandler RequestCompleted;

        private string _curHeaderName = string.Empty;
        private string _curHeaderValue = string.Empty;
        private State _curState;
        private readonly HttpRequest _request = new HttpRequest();

        /// <summary>
        /// Create a new request parser
        /// </summary>
        /// <param name="requestCompleted">delegate called when a complete request have been generated</param>
        /// <param name="logWriter">delegate receiving log entries.</param>
        public HttpRequestParser(RequestCompletedHandler requestCompleted, ILogWriter logWriter)
        {
            if (requestCompleted == null)
                throw new ArgumentNullException("requestCompleted");

            _log = logWriter ?? NullLogWriter.Instance;
            RequestCompleted = requestCompleted;
        }

        /// <summary>
        /// Current state in parser.
        /// </summary>
        public State CurrentState
        {
            get { return _curState; }
        }

        internal HttpRequest CurrentRequest
        {
            get { return _request; }
        }

        /// <summary>
        /// Add a number of bytes to the body
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int AddToBody(byte[] buffer, int offset, int count)
        {
            int bytesUsed = CurrentRequest.AddToBody(buffer, offset, count);
            if (CurrentRequest.BodyIsComplete)
            {
                int index = -1;
                for (int i = offset + bytesUsed; i < offset + count; ++i)
                    buffer[++index] = buffer[i];

                // Go to beginning of buffer since we are done.
                CurrentRequest.Body.Seek(0, SeekOrigin.Begin);

                // got a complete request.
                _log.Write(this, LogPrio.Trace, "Request parsed successfully.");
                RequestCompleted(CurrentRequest);
                Clear();
            }

            return offset + bytesUsed;
        }

        /// <summary>
        /// Remove all state information for the request.
        /// </summary>
        private void Clear()
        {
            CurrentRequest.Clear();
            _curHeaderName = string.Empty;
            _curHeaderValue = string.Empty;
            _curState = State.FirstLine;
        }

        /// <summary>
        /// Parse request line
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="BadRequestException">If line is incorrect</exception>
        /// <remarks>Expects the following format: "Method SP Request-URI SP HTTP-Version CRLF"</remarks>
        protected void OnFirstLine(string value)
        {
            //
            //todo: In the interest of robustness, servers SHOULD ignore any empty line(s) received where a Request-Line is expected. 
            // In other words, if the server is reading the protocol stream at the beginning of a message and receives a CRLF first, it should ignore the CRLF.
            //
            _log.Write(this, LogPrio.Debug, "Got request: " + value);

            //Request-Line   = Method SP Request-URI SP HTTP-Version CRLF
            int pos = value.IndexOf(' ');
            if (pos == -1 || pos + 1 >= value.Length)
            {
                _log.Write(this, LogPrio.Warning, "Invalid request line, missing Method. Line: " + value);
                throw new BadRequestException("Invalid request line, missing Method. Line: " + value);
            }

            CurrentRequest.Method = value.Substring(0, pos).ToUpper();
            int oldPos = pos + 1;
            pos = value.IndexOf(' ', oldPos);
            if (pos == -1)
            {
                _log.Write(this, LogPrio.Warning, "Invalid request line, missing URI. Line: " + value);
                throw new BadRequestException("Invalid request line, missing URI. Line: " + value);
            }
            CurrentRequest.UriPath = value.Substring(oldPos, pos - oldPos);
            if (CurrentRequest.UriPath.Length > 4196)
                throw new BadRequestException("Too long uri.");

            if (pos + 1 >= value.Length)
            {
                _log.Write(this, LogPrio.Warning, "Invalid request line, missing HTTP-Version. Line: " + value);
                throw new BadRequestException("Invalid request line, missing HTTP-Version. Line: " + value);
            }
            CurrentRequest.HttpVersion = value.Substring(pos + 1);
            if (CurrentRequest.HttpVersion.Length < 4 || string.Compare(CurrentRequest.HttpVersion.Substring(0, 4), "HTTP", true) != 0)
            {
                _log.Write(this, LogPrio.Warning, "Invalid HTTP version in request line. Line: " + value);
                throw new BadRequestException("Invalid HTTP version in Request line. Line: " + value);
            }
        }

        /// <summary>
        /// We've parsed a new header.
        /// </summary>
        /// <param name="name">Name in lower case</param>
        /// <param name="value">Value, unmodified.</param>
        /// <exception cref="BadRequestException">If content length cannot be parsed.</exception>
        protected void OnHeader(string name, string value)
        {
            CurrentRequest.AddHeader(name, value);
        }

        /// <summary>
        /// Parse a message
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">where in buffer that parsing should start</param>
        /// <param name="size">number of bytes to parse</param>
        /// <returns>Unparsed bytes left in buffer.</returns>
        public int ParseMessage(byte[] buffer, int offset, int size)
        {
            // add body bytes
            if (_curState == State.Body)
            {
                return AddToBody(buffer, 0, size);
            }

#if DEBUG
            string temp = Encoding.ASCII.GetString(buffer, offset, size);
            _log.Write(this, LogPrio.Trace, "\r\n\r\n HTTP MESSAGE: " + temp + "\r\n");
#endif

            int currentLine = 1;
            int startPos = -1;

            // set start pos since this is from an partial request
            if (_curState == State.HeaderValue)
                startPos = 0;

            int end = offset + size;

            //<summary>
            // Handled bytes are used to keep track of the number of bytes processed.
            // We do this since we can handle partial requests (to be able to check headers and abort
            // invalid requests directly without having to process the whole header / body).
            // </summary>
            int handledBytes = 0;


            for (int i = offset; i < end; ++i)
            {
                char ch = (char) buffer[i];
                char nextCh = end > i + 1 ? (char)buffer[i + 1] : char.MinValue;

                if (ch == '\r')
                    ++currentLine;

                if (_curState == State.FirstLine)
                {
                    if (i > 4196)
                    {
                        _log.Write(this, LogPrio.Warning, "HTTP Request is too large.");
                        throw new BadRequestException("Too large request line.");
                    }

                    if (char.IsLetterOrDigit(ch) && startPos == -1)
                        startPos = i;

                    // first line can be empty according to RFC, ignore it and move next.
                    if (startPos == -1 && (ch != '\r' || nextCh != '\n'))
                    {
                        _log.Write(this, LogPrio.Warning, "Request line is not found.");
                        throw new BadRequestException("Invalid request line.");
                    }

                    if (startPos != -1 && ch == '\r')
                    {
                        if (nextCh != '\n')
                        {
                            _log.Write(this, LogPrio.Warning, "RFC says that linebreaks should be \\r\\n, got only \\n.");
                            throw new BadRequestException("Invalid line break on request line, expected CrLf.");
                        }

                        OnFirstLine(Encoding.UTF8.GetString(buffer, startPos, i - startPos));
                        _curState = _curState + 1;
                        ++i;
                        handledBytes = i + 1;
                        startPos = -1;
                    }
                }
                    // are parsing header name
                else if (_curState == State.HeaderName)
                {
                    // Check if we got end of header
                    if (ch == '\r')
                    {
                        if (nextCh != '\n')
                        {
                            _log.Write(this, LogPrio.Warning, "Expected crlf, got only lf.");
                            throw new BadRequestException("Expected crlf on line " + currentLine);
                        }

                        ++i; //ignore \r
                        ++i; //ignore \n

                        if (CurrentRequest.ContentLength == 0)
                        {
                            _curState = State.FirstLine;
                            _log.Write(this, LogPrio.Trace, "Request parsed successfully (no content).");
                            RequestCompleted(CurrentRequest);
                            Clear();
                            return i;
                        }

                        _curState = State.Body;
                        if (i + 1 < end)
                        {
                            _log.Write(this, LogPrio.Trace, "Adding bytes to the body");
                            return AddToBody(buffer, i, end - i);
                        }

                        return i;
                    }

                    if (char.IsWhiteSpace(ch) || ch == ':')
                    {
                        if (startPos == -1)
                        {
                            _log.Write(this, LogPrio.Warning, "Expected header name, got colon on line " + currentLine);
                            throw new BadRequestException("Expected header name, got colon on line " + currentLine);
                        }
                        _curHeaderName = Encoding.UTF8.GetString(buffer, startPos, i - startPos);
                        handledBytes = i + 1;
                        startPos = -1;
                        _curState = _curState + 1;
                        if (ch == ':')
                            _curState = _curState + 1;
                    }
                    else if (startPos == -1)
                        startPos = i;
                    else if (!char.IsLetterOrDigit(ch) && ch != '-')
                    {
                        _log.Write(this, LogPrio.Warning, "Invalid character in header name on line " + currentLine);
                        throw new BadRequestException("Invalid character in header name on line " + currentLine);
                    }

                    if (startPos != -1 && i - startPos > 200)
                    {
                        _log.Write(this, LogPrio.Warning, "Invalid header name on line " + currentLine);
                        throw new BadRequestException("Invalid header name on line " + currentLine);
                    }
                }
                else if (_curState == State.AfterName)
                {
                    if (ch == ':')
                    {
                        handledBytes = i+1;
                        _curState = _curState + 1;
                    }
                }
                    // parsing chars between header and value
                else if (_curState == State.Between)
                {
                    if (ch == ' ' || ch == '\t')
                        continue;

                    // continue if we get a new line which is prepended with a whitespace
                    if (ch == '\r' && nextCh == '\n' && i + 3 < end &&
                             char.IsWhiteSpace((char) buffer[i + 2]))
                    {
                        ++i;
                        continue;
                    }

                    startPos = i;
                    _curState = _curState + 1;
                    handledBytes = i;
                    continue;
                }
                else if (_curState == State.HeaderValue)
                {
                    if (ch != '\r')
                        continue;
                    if (nextCh != '\n')
                    {
                        _log.Write(this, LogPrio.Warning, "Invalid linebreak on line " + currentLine);
                        throw new BadRequestException("Invalid linebreak on line " + currentLine);
                    }

                    if (startPos == -1)
                        continue; // allow new lines before start of value

                    //if (_curHeaderName == string.EmptyLanguageNode)
                    //    throw new BadRequestException("Missing header on line " + currentLine);
                    if (startPos == -1)
                    {
                        _log.Write(this, LogPrio.Warning, "Missing header value for '" + _curHeaderName);
                        throw new BadRequestException("Missing header value for '" + _curHeaderName);
                    }
                    if (i - startPos > 1024)
                    {
                        _log.Write(this, LogPrio.Warning, "Too large header value on line " + currentLine);
                        throw new BadRequestException("Too large header value on line " + currentLine);
                    }

                    // Header fields can be extended over multiple lines by preceding each extra line with at
                    // least one SP or HT.
                    if (end > i + 3 && (buffer[i + 2] == ' ' || buffer[i + 2] == buffer['\t']))
                    {
                        if (startPos != -1)
                            _curHeaderValue = Encoding.UTF8.GetString(buffer, startPos, i - startPos);

                        _log.Write(this, LogPrio.Trace, "Header value is on multiple lines.");
                        _curState = State.Between;
                        startPos = -1;
                        ++i;
                        handledBytes = i + 1;
                        continue;
                    }

                    _curHeaderValue += Encoding.UTF8.GetString(buffer, startPos, i - startPos);
                    _log.Write(this, LogPrio.Trace, "Header [" + _curHeaderName + ": " + _curHeaderValue + "]");
                    OnHeader(_curHeaderName, _curHeaderValue);

                    startPos = -1;
                    _curState = State.HeaderName;
                    _curHeaderValue = string.Empty;
                    _curHeaderName = string.Empty;
                    ++i;
                    handledBytes = i + 1;

                    // Check if we got a colon so we can cut header name, or crlf for end of header.
                    bool canContinue = false;
                    for (int j = i; j < end; ++j)
                    {
                        if (buffer[j] == ':' || buffer[j] == '\r')
                        {
                            canContinue = true;
                            break;
                        }
                    }
                    if (!canContinue)
                    {
                        _log.Write(this, LogPrio.Trace, "Cant continue, no colon.");
                        return i + 1;
                    }
                }
            }

            return handledBytes;
        }


        #region Nested type: State

        /// <summary>
        /// Current state in the parsing.
        /// </summary>
        public enum State
        {
            /// <summary>
            /// Should parse the request line
            /// </summary>
            FirstLine, 
            /// <summary>
            /// Searching for a complete header name
            /// </summary>
            HeaderName, 
            /// <summary>
            /// Searching for colon after header name (ignoring white spaces)
            /// </summary>
            AfterName,
            /// <summary>
            /// Searching for start of header value (ignoring white spaces)
            /// </summary>
            Between, 
            /// <summary>
            /// Searching for a complete header value (can span over multiple lines, as long as they are prefixed with one/more whitespaces)
            /// </summary>
            HeaderValue, 

            /// <summary>
            /// Adding bytes to body
            /// </summary>
            Body 
        }

        #endregion
    }
}