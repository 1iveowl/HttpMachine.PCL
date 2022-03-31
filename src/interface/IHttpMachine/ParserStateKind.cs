namespace IHttpMachine
{
    public enum ParserStateKind
    {
        Ready,
        StartingParsing,
        HeadersReceived,
        ChunkReceived,
        BodyReceived,
        RequestTimedOut,
        ParserError,
        EndOfRequest,
        Succeeded,
        Failed
    }
}
