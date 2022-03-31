namespace IHttpMachine.Model
{
    public abstract record ParserControlBase : HttpHeaderBase, IParserControl
    {
        public bool IsEndOfRequest { get; init; }
        public bool IsRequestTimedOut { get; init; }
        public bool IsUnableToParseHttp { get; init; }
        public string RemoteAddress { get; init; }
        public int RemotePort { get; init; }

        protected ParserControlBase(IParserControl parserControl, IParserHeader parserHeader) : base(parserHeader)
        {
            IsEndOfRequest = parserControl.IsEndOfRequest;
            IsRequestTimedOut = parserControl.IsRequestTimedOut;
            IsUnableToParseHttp = parserControl.IsUnableToParseHttp;
            RemoteAddress = parserControl.RemoteAddress;
            RemotePort = parserControl.RemotePort;
        }
    }
}
