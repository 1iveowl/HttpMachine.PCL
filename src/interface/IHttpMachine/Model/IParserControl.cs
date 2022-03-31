namespace IHttpMachine.Model
{
    public interface IParserControl : IParserHeader
    {
        bool IsEndOfRequest { get; }
        bool IsRequestTimedOut { get; }
        bool IsUnableToParseHttp { get; }
        string RemoteAddress { get; }
        int RemotePort { get; }
    }
}
