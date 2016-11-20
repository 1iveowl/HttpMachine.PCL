using System;

namespace HttpMachine
{
    public interface IHttpResponseParserDelegate : IHttpParserDelegate, IDisposable
    {
        void OnResponseType(HttpCombinedParser combinedParser);
        void OnResponseCode(HttpCombinedParser combinedParser, int statusCode, string statusReason);
    }
}
