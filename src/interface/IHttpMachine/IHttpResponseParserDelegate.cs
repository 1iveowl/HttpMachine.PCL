using System;

namespace IHttpMachine
{
    public interface IHttpResponseParserDelegate : IHttpParserDelegate, IDisposable
    {
        void OnResponseType(IHttpCombinedParser combinedParser);
        void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason);
    }
}
