using System.IO;
using HttpMachine;

namespace IHttpMachine.Model;

/// <summary>
/// Immutable snapshot of one fully parsed HTTP request or response, exposed via
/// <see cref="IHttpParserDelegate.HttpRequestResponse"/> once the message is complete.
/// </summary>
public record HttpRequestResponse : ParserControlBase, IHttpRequestResponse
{
    /// <inheritdoc/>
    public MessageTypeKind MessageType { get; init; }

    /// <inheritdoc/>
    public int StatusCode { get; init; }

    /// <inheritdoc/>
    public string ResponseReason { get; init; }

    /// <inheritdoc/>
    public int MajorVersion { get; init; }

    /// <inheritdoc/>
    public int MinorVersion { get; init; }

    /// <inheritdoc/>
    public bool ShouldKeepAlive { get; init; }

    /// <inheritdoc/>
    public object UserContext { get; init; }

    /// <inheritdoc/>
    public string Method { get; init; }

    /// <inheritdoc/>
    public string RequestUri { get; init; }

    /// <inheritdoc/>
    public string Path { get; init; }

    /// <inheritdoc/>
    public string QueryString { get; init; }

    /// <inheritdoc/>
    public string Fragment { get; init; }

    /// <inheritdoc/>
    public MemoryStream Body { get; init; }

    /// <summary>Creates a snapshot by copying all values from <paramref name="httpRequestResponse"/>.</summary>
    public HttpRequestResponse(
        IHttpRequestResponse httpRequestResponse) : base(httpRequestResponse, httpRequestResponse)
    {
        MessageType = httpRequestResponse.MessageType;
        StatusCode = httpRequestResponse.StatusCode;
        ResponseReason = httpRequestResponse.ResponseReason;
        MajorVersion = httpRequestResponse.MajorVersion;
        MinorVersion = httpRequestResponse.MinorVersion;
        ShouldKeepAlive = httpRequestResponse.ShouldKeepAlive;
        UserContext = httpRequestResponse.UserContext;
        Method = httpRequestResponse.Method;
        RequestUri = httpRequestResponse.RequestUri;
        Path = httpRequestResponse.Path;
        QueryString = httpRequestResponse.QueryString;
        Fragment = httpRequestResponse.Fragment;
        Body = httpRequestResponse.Body;
    }
}
