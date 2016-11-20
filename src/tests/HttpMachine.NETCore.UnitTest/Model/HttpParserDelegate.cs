using System;
using System.Collections.Generic;
using System.Text;

namespace HttpMachine.NETCore.UnitTest.Model
{
    internal class HttpParserDelegate : IHttpParserCombinedDelegate
    {
        private string _headerName;
        private bool _headerAlreadyExist;
        private readonly int[] _chunkSizeSequence;
        private int _chunkSequencePosition; 

        public bool[] IsChunkSizeSquenceOk;

        public readonly HttpRequestAndReponse HttpRequestReponse = new HttpRequestAndReponse();

        public MessageType MessageType { get; internal set; }

        public void OnMessageBegin(HttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnResponseCode(HttpCombinedParser combinedParser, int statusCode, string statusReason)
        {
            HttpRequestReponse.StatusCode = statusCode;
            HttpRequestReponse.ResponseReason = statusReason;
        }

        public void OnHeaderName(HttpCombinedParser combinedParser, string name)
        {
            if (HttpRequestReponse.Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = true;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(HttpCombinedParser combinedParser, string value)
        {
            if (_headerAlreadyExist)
            {
                // Join multiple message-header fields into one comma seperated list http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                HttpRequestReponse.Headers[_headerName] = $"{HttpRequestReponse.Headers[_headerName]}, {value}";
                _headerAlreadyExist = false;
            }
            else
            {
                HttpRequestReponse.Headers[_headerName] = value;
            }
        }

        public void OnHeadersEnd(HttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnTransferEncodingChunked(HttpCombinedParser combinedParser, bool isChunked)
        {
            HttpRequestReponse.IsChunked = isChunked;
        }

        public void OnChunkedLength(HttpCombinedParser combinedParser, int length)
        {
            HttpRequestReponse.ChunkSize = length;
        }

        public void OnChunkReceived(HttpCombinedParser combinedParser)
        {
            IsChunkSizeSquenceOk[_chunkSequencePosition] = HttpRequestReponse.ChunkSize == _chunkSizeSequence[_chunkSequencePosition];
            _chunkSequencePosition++;
        }

        public void OnBody(HttpCombinedParser combinedParser, ArraySegment<byte> data)
        {
            HttpRequestReponse.Body.Write(data.Array, data.Offset, data.Count);
        }

        public void OnMessageEnd(HttpCombinedParser combinedParser)
        {
            
        }

        public void OnParserError()
        {

        }

        public void OnRequestType(HttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Request;
            MessageType = MessageType.Request;
        }

        public void OnResponseType(HttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Response;
            MessageType = MessageType.Response;
        }

        public void OnMethod(HttpCombinedParser combinedParser, string method)
        {
            HttpRequestReponse.Method = method;
        }

        public void OnRequestUri(HttpCombinedParser combinedParser, string requestUri)
        {
            HttpRequestReponse.RequestUri = requestUri;
        }

        public void OnPath(HttpCombinedParser combinedParser, string path)
        {
            HttpRequestReponse.Path = path;
        }

        public void OnFragment(HttpCombinedParser combinedParser, string fragment)
        {
            HttpRequestReponse.Fragment = fragment;
        }

        public void OnQueryString(HttpCombinedParser combinedParser, string queryString)
        {
            HttpRequestReponse.QueryString = queryString;
        }

        public void Dispose()
        {

        }

        public HttpParserDelegate(int numberOfChunks)
        {
            _chunkSizeSequence = new int[numberOfChunks];
            IsChunkSizeSquenceOk = new bool[numberOfChunks];
        }
    }
}
