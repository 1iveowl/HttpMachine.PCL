using System;

namespace HttpMachine
{
    public interface IHttpRequestParserDelegate : IHttpParserDelegate, IDisposable
    {

        void OnRequestType(HttpCombinedParser combinedParser);
        void OnMethod(HttpCombinedParser combinedParser, string method);
        void OnRequestUri(HttpCombinedParser combinedParser, string requestUri);
        void OnPath(HttpCombinedParser combinedParser, string path);
        void OnFragment(HttpCombinedParser combinedParser, string fragment);
        void OnQueryString(HttpCombinedParser combinedParser, string queryString);
    }
}
