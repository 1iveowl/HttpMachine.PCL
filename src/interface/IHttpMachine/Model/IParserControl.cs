namespace IHttpMachine.Model;

/// <summary>Status flags describing how parsing of a message concluded.</summary>
public interface IParserControl : IParserHeader
{
    /// <summary>Whether the message was parsed to completion, including its body.</summary>
    bool IsEndOfMessage { get; }

    /// <summary>Whether the request timed out. Not set by the parser itself; available for consumers implementing timeouts.</summary>
    bool IsRequestTimedOut { get; }

    /// <summary>Whether the parser failed to parse the input as HTTP.</summary>
    bool IsUnableToParseHttp { get; }

    /// <summary>Remote address the message came from. Not set by the parser itself; available for consumers tracking connections.</summary>
    string RemoteAddress { get; }

    /// <summary>Remote port the message came from. Not set by the parser itself; available for consumers tracking connections.</summary>
    int RemotePort { get; }
}
