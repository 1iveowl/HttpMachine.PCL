using System.Collections.Generic;

namespace IHttpMachine.Model;

/// <summary>Base record carrying the header-related results of a parsed message. See <see cref="IParserHeader"/>.</summary>
public abstract record HttpHeaderBase : IParserHeader
{
    /// <inheritdoc/>
    public IDictionary<string, IEnumerable<string>> Headers { get; init; }

    /// <inheritdoc/>
    public bool IsTransferEncodingChunked { get; init; }

    /// <inheritdoc/>
    public int ChunkSize { get; internal set; }

    /// <summary>Copies the header results from <paramref name="parserHeader"/>.</summary>
    protected HttpHeaderBase(IParserHeader parserHeader)
    {
        Headers = parserHeader.Headers;
        IsTransferEncodingChunked = parserHeader.IsTransferEncodingChunked;
        ChunkSize = parserHeader.ChunkSize;
    }
}
