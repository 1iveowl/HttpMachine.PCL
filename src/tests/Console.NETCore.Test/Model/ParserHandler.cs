using System;
using HttpMachine;
using IHttpMachine;

namespace Console.NETCore.Test.Model
{
    internal class ParserHandler : IHttpParserCombinedDelegate
    {
        public readonly HttpRequestReponse HttpRequestReponse = new HttpRequestReponse();
        public bool HasError { get; internal set; } = false;
        public MessageType MessageType { get; private set; }

        public void OnMessageBegin(IHttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderName(IHttpCombinedParser combinedParser, string name)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderValue(IHttpCombinedParser combinedParser, string value)
        {
            //throw new NotImplementedException();
        }

        public void OnHeadersEnd(IHttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked)
        {
            HttpRequestReponse.IsTransferEncodingChunked = isChunked;
        }

        public void OnChunkedLength(IHttpCombinedParser combinedParser, int length)
        {
            HttpRequestReponse.ChunkSize = length;
        }

        public void OnChunkReceived(IHttpCombinedParser combinedParser)
        {

        }

        public void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data)
        {
            HttpRequestReponse.Body.Write(data.Array, data.Offset, data.Count);
        }

        public void OnMessageEnd(IHttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnParserError()
        {
            HasError = true;
        }

        public void OnRequestType(IHttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Request;
        }

        public void OnMethod(IHttpCombinedParser combinedParser, string method)
        {
            //throw new NotImplementedException();
        }

        public void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri)
        {
            //throw new NotImplementedException();
        }

        public void OnPath(IHttpCombinedParser combinedParser, string path)
        {
            //throw new NotImplementedException();
        }

        public void OnFragment(IHttpCombinedParser combinedParser, string fragment)
        {
            //throw new NotImplementedException();
        }

        public void OnQueryString(IHttpCombinedParser combinedParser, string queryString)
        {
            //throw new NotImplementedException();
        }

        public void OnResponseType(IHttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Response;
        }

        public void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason)
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
