namespace HttpMachine
{
    public interface IHttpResponseParserDelegate : IHttpParserDelegate
    {
        void OnResponseType(HttpCombinedParser combinedParser);
        void OnResponseCode(HttpCombinedParser combinedParser, int statusCode, string statusReason);
    }
}
