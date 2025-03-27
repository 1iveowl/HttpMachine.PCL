using System.Collections.Generic;

namespace IHttpMachine.Model;

public abstract record HttpHeaderBase : IParserHeader
{
    public IDictionary<string, IEnumerable<string>> Headers { get; init; }
    public bool IsTransferEncodingChunked { get; init; }
    public int ChunkSize { get; internal set; }

    protected HttpHeaderBase(IParserHeader parserHeader)
    {
        Headers = parserHeader.Headers;
        IsTransferEncodingChunked = parserHeader.IsTransferEncodingChunked;
        ChunkSize = parserHeader.ChunkSize;
    }
}
