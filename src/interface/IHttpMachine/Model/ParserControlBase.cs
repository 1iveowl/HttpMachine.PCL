namespace IHttpMachine.Model;

/// <summary>Base record carrying the status flags of a parsed message. See <see cref="IParserControl"/>.</summary>
public abstract record ParserControlBase : HttpHeaderBase, IParserControl
{
    /// <inheritdoc/>
    public bool IsEndOfMessage { get; init; }

    /// <inheritdoc/>
    public bool IsRequestTimedOut { get; init; }

    /// <inheritdoc/>
    public bool IsUnableToParseHttp { get; init; }

    /// <inheritdoc/>
    public string RemoteAddress { get; init; }

    /// <inheritdoc/>
    public int RemotePort { get; init; }

    /// <summary>Copies the status flags from <paramref name="parserControl"/> and the headers from <paramref name="parserHeader"/>.</summary>
    protected ParserControlBase(IParserControl parserControl, IParserHeader parserHeader) : base(parserHeader)
    {
        IsEndOfMessage = parserControl.IsEndOfMessage;
        IsRequestTimedOut = parserControl.IsRequestTimedOut;
        IsUnableToParseHttp = parserControl.IsUnableToParseHttp;
        RemoteAddress = parserControl.RemoteAddress;
        RemotePort = parserControl.RemotePort;
    }
}
