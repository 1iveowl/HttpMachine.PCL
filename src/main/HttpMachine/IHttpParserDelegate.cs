using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMachine
{
    public interface IHttpParserDelegate
    {
        MessageType MessageType { get; }
        void OnMessageBegin(HttpCombinedParser combinedParser);
        void OnHeaderName(HttpCombinedParser combinedParser, string name);
        void OnHeaderValue(HttpCombinedParser combinedParser, string value);
        void OnHeadersEnd(HttpCombinedParser combinedParser);
        void OnBody(HttpCombinedParser combinedParser, ArraySegment<byte> data);
        void OnMessageEnd(HttpCombinedParser combinedParser);
        void OnParserError();
    }

}

