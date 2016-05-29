using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpMachine.Console.Test.Model
{
    internal class ResponseParserHandler : IHttpResponseParserDelegate
    {
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }
        public int StatusCode { get; internal set; }
        public string ResponseReason { get; internal set; }

        public MemoryStream Body { get; internal set; } = new MemoryStream();

        public IDictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();

        public void OnMessageBegin(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        private string _headerName;
        private bool _headerAlreadyExist;

        //http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
        public void OnHeaderName(HttpMachine.HttpParser parser, string name)
        {

            if (Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = true;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(HttpMachine.HttpParser parser, string value)
        {
            if (_headerAlreadyExist)
            {
                // Join multiple message-header fields into one comma seperated list http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                Headers[_headerName] = $"{Headers[_headerName]}, {value}";
                _headerAlreadyExist = false;
            }
            else
            {
                Headers[_headerName] = value;
            }
        }

        public void OnHeadersEnd(HttpMachine.HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnMessageEnd(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnResponseCode(HttpParser parser, int statusCode, string statusReason)
        {
            StatusCode = statusCode;
            ResponseReason = statusReason;
        }
    }
}
