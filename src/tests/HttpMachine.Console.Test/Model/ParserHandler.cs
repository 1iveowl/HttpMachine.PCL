using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpMachine.Console.Test.Model
{
    internal class ParserHandler : IHttpParserCombinedDelegate
    {
        public bool HasError { get; internal set; } = false;
        public MessageType MessageType { get; private set; }

        public void OnMessageBegin(HttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderName(HttpCombinedParser combinedParser, string name)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderValue(HttpCombinedParser combinedParser, string value)
        {
            //throw new NotImplementedException();
        }

        public void OnHeadersEnd(HttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnTransferEncodingChunked(HttpCombinedParser combinedParser, bool isChunked)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpCombinedParser combinedParser, ArraySegment<byte> data)
        {
            //throw new NotImplementedException();
        }

        public void OnMessageEnd(HttpCombinedParser combinedParser)
        {
            //throw new NotImplementedException();
        }

        public void OnParserError()
        {
            HasError = true;
        }

        public void OnRequestType(HttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Request;
        }

        public void OnMethod(HttpCombinedParser combinedParser, string method)
        {
            //throw new NotImplementedException();
        }

        public void OnRequestUri(HttpCombinedParser combinedParser, string requestUri)
        {
            //throw new NotImplementedException();
        }

        public void OnPath(HttpCombinedParser combinedParser, string path)
        {
            //throw new NotImplementedException();
        }

        public void OnFragment(HttpCombinedParser combinedParser, string fragment)
        {
            //throw new NotImplementedException();
        }

        public void OnQueryString(HttpCombinedParser combinedParser, string queryString)
        {
            //throw new NotImplementedException();
        }

        public void OnResponseType(HttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Response;
        }

        public void OnResponseCode(HttpCombinedParser combinedParser, int statusCode, string statusReason)
        {
            //throw new NotImplementedException();
        }
    }
}
