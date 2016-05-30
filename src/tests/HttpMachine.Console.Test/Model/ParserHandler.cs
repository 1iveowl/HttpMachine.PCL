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

        public void OnMessageBegin(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderName(HttpParser parser, string name)
        {
            //throw new NotImplementedException();
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
            //throw new NotImplementedException();
        }

        public void OnHeadersEnd(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            //throw new NotImplementedException();
        }

        public void OnMessageEnd(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnParserError()
        {
            HasError = true;
        }

        public void OnRequestType(HttpParser parser)
        {
            MessageType = MessageType.Request;
        }

        public void OnMethod(HttpParser parser, string method)
        {
            //throw new NotImplementedException();
        }

        public void OnRequestUri(HttpParser parser, string requestUri)
        {
            //throw new NotImplementedException();
        }

        public void OnPath(HttpParser parser, string path)
        {
            //throw new NotImplementedException();
        }

        public void OnFragment(HttpParser parser, string fragment)
        {
            //throw new NotImplementedException();
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
            //throw new NotImplementedException();
        }

        public void OnResponseType(HttpParser parser)
        {
            MessageType = MessageType.Response;
        }

        public void OnResponseCode(HttpParser parser, int statusCode, string statusReason)
        {
            //throw new NotImplementedException();
        }
    }
}
