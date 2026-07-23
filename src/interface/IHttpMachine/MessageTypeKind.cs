namespace HttpMachine;

/// <summary>Identifies whether a parsed HTTP message was a request or a response.</summary>
public enum MessageTypeKind
{
    /// <summary>The message was an HTTP request.</summary>
    Request,

    /// <summary>The message was an HTTP response.</summary>
    Response
}
