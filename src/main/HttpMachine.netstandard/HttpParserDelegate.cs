using System;
using System.Collections.Generic;
using System.Linq;
using HttpMachine.Model.Internal;
using IHttpMachine;
using IHttpMachine.Model;

namespace HttpMachine
{
    public class HttpParserDelegate : IHttpParserCombinedDelegate
    {
        private readonly InternalHttpRequestResponse _httpRequestResponse;
        private string _readingHeader;
        private readonly IDictionary<string, IList<string>> _headers;

        public bool HasError { get; private set; } 

        public HttpRequestResponse HttpRequestResponse { get; private set; }

        public HttpParserDelegate()
        {
            _httpRequestResponse = new InternalHttpRequestResponse();
            _headers = new Dictionary<string, IList<string>>();
        }

        public virtual void OnMessageBegin(IHttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public virtual void OnHeaderName(IHttpCombinedParser combinedParser, string headerName)
        {
            // Only update _readingHeader if it has changed to minimize garbage collection from creating strings.
            if (!string.Equals(_readingHeader, headerName, StringComparison.OrdinalIgnoreCase))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _readingHeader = headerName.ToUpper();
                _headers.Add(_readingHeader, new List<string>());
            }
        }

        public virtual void OnHeaderValue(IHttpCombinedParser combinedParser, string value)
        {
            if (_headers.TryGetValue(_readingHeader, out var list))
            {
                list.Add(value);             
            }
            else
            {
                throw new InvalidOperationException("Something unexpected happened. Trying to add header value to undefined header.");
            }
        }

        public virtual void OnHeadersEnd(IHttpCombinedParser combinedParser)
        {
            _httpRequestResponse.Headers = _headers
                .Select(d => new { d.Key, enumerable = d.Value.AsEnumerable() } )
                .ToDictionary(d => d.Key, d => d.enumerable);
        }

        public virtual void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked)
        {
            _httpRequestResponse.IsTransferEncodingChunked = isChunked;
        }

        public virtual void OnChunkedLength(IHttpCombinedParser combinedParser, int length)
        {
            _httpRequestResponse.ChunkSize = length;
        }

        public virtual void OnChunkReceived(IHttpCombinedParser combinedParser)
        {

        }

        public virtual void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data)
        {
            _httpRequestResponse.Body.Write(data.Array, data.Offset, data.Count);
        }

        public virtual void OnMessageEnd(IHttpCombinedParser combinedParser)
        {
            if (!_httpRequestResponse.IsRequestTimedOut && !_httpRequestResponse.IsUnableToParseHttp)
            {
                HttpRequestResponse = new(_httpRequestResponse);
            }
        }

        public virtual void OnParserError()
        {
            HasError = true;
        }

        public virtual void OnMethod(IHttpCombinedParser combinedParser, string method)
        {
            _httpRequestResponse.Method = method;
        }

        public virtual void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri)
        {
            _httpRequestResponse.RequestUri = requestUri;
        }

        public virtual void OnPath(IHttpCombinedParser combinedParser, string path)
        {
            _httpRequestResponse.Path = path;
        }

        public virtual void OnFragment(IHttpCombinedParser combinedParser, string fragment)
        {
            _httpRequestResponse.Fragment = fragment;
        }

        public virtual void OnQueryString(IHttpCombinedParser combinedParser, string queryString)
        {
            _httpRequestResponse.QueryString = queryString;
        }

        public virtual void OnRequestType(IHttpCombinedParser combinedParser)
        {
            _httpRequestResponse.MessageType = MessageTypeKind.Request;
        }

        public virtual void OnResponseType(IHttpCombinedParser combinedParser)
        {
            _httpRequestResponse.MessageType = MessageTypeKind.Response;
        }

        public virtual void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason)
        {
            _httpRequestResponse.ResponseReason = statusReason;
            _httpRequestResponse.StatusCode = statusCode;
        }

        public virtual void Dispose()
        {

        }
    }
}
