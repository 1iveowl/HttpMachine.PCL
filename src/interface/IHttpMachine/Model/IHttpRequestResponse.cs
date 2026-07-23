using HttpMachine;
using System.IO;

namespace IHttpMachine.Model;

/// <summary>The complete result of parsing one HTTP request or response.</summary>
public interface IHttpRequestResponse : IParserControl
{
    /// <summary>Whether the message was a request or a response.</summary>
    public MessageTypeKind MessageType { get; }

    /// <summary>Status code from the status line; responses only, otherwise <c>0</c>.</summary>
    public int StatusCode { get; }

    /// <summary>Reason phrase from the status line; responses only, otherwise <c>null</c>.</summary>
    public string ResponseReason { get; }

    /// <summary>HTTP major version (for example <c>1</c> for HTTP/1.1).</summary>
    public int MajorVersion { get; }

    /// <summary>HTTP minor version (for example <c>1</c> for HTTP/1.1).</summary>
    public int MinorVersion { get; }

    /// <summary>Whether the connection should be kept open after this message.</summary>
    public bool ShouldKeepAlive { get; }

    /// <summary>Free-form slot for consumers to associate their own state with a message; not set by the parser.</summary>
    public object UserContext { get; }

    /// <summary>Request method (for example <c>GET</c>); requests only, otherwise <c>null</c>.</summary>
    public string Method { get; }

    /// <summary>The complete request target from the request line; requests only, otherwise <c>null</c>.</summary>
    public string RequestUri { get; }

    /// <summary>Path portion of the request target; requests only, otherwise <c>null</c>.</summary>
    public string Path { get; }

    /// <summary>Query string portion of the request target (without the leading <c>?</c>); requests only, otherwise <c>null</c>.</summary>
    public string QueryString { get; }

    /// <summary>Fragment portion of the request target; requests only, otherwise <c>null</c>.</summary>
    public string Fragment { get; }

    /// <summary>The message body. Rewind (<c>Position = 0</c>) before reading.</summary>
    public MemoryStream Body { get; }
}
