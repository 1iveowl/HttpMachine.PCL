namespace IHttpMachine.Model
{
    public interface IParserControl : IParserHeader
    {
        bool IsEndOfMessage { get; }
        bool IsRequestTimedOut { get; }
        bool IsUnableToParseHttp { get; }
        string RemoteAddress { get; }
        int RemotePort { get; }
    }
}
