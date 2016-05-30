namespace HttpMachine
{
    public interface IHttpResponseParserDelegate : IHttpParserDelegate
    {
        void OnResponseType(HttpParser parser);
        void OnResponseCode(HttpParser parser, int statusCode, string statusReason);
    }
}
