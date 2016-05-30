namespace HttpMachine
{
    public interface IHttpRequestParserDelegate : IHttpParserDelegate
    {

        void OnRequestType(HttpParser parser);
        void OnMethod(HttpParser parser, string method);
        void OnRequestUri(HttpParser parser, string requestUri);
        void OnPath(HttpParser parser, string path);
        void OnFragment(HttpParser parser, string fragment);
        void OnQueryString(HttpParser parser, string queryString);
    }
}
