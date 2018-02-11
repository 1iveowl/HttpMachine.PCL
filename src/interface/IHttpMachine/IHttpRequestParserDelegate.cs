using System;

namespace IHttpMachine
{
    public interface IHttpRequestParserDelegate : IHttpParserDelegate, IDisposable
    {

        void OnRequestType(IHttpCombinedParser combinedParser);
        void OnMethod(IHttpCombinedParser combinedParser, string method);
        void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri);
        void OnPath(IHttpCombinedParser combinedParser, string path);
        void OnFragment(IHttpCombinedParser combinedParser, string fragment);
        void OnQueryString(IHttpCombinedParser combinedParser, string queryString);
    }
}
