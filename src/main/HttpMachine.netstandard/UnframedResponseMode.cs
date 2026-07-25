namespace HttpMachine;

/// <summary>
/// Controls how the parser delimits a <em>response</em> body when the response carries neither a
/// <c>Content-Length</c> nor a <c>Transfer-Encoding</c> header — the one case where strict HTTP
/// framing and datagram-style (HTTPU/SSDP) framing genuinely disagree. See the README section
/// "Message framing" for the full rationale.
/// </summary>
public enum UnframedResponseMode
{
    /// <summary>
    /// A keep-alive response with no framing headers is treated as a complete, bodyless message
    /// at the end of its header section. This suits HTTPU/SSDP and UPnP discovery, where each UDP
    /// datagram is a self-contained, bodyless message and there is no connection to close.
    /// This is the default and matches the library's historical behaviour.
    /// (A response that is <c>Connection: close</c> is still read until end of stream in this mode.)
    /// </summary>
    CompleteAtHeaders = 0,

    /// <summary>
    /// A response with no framing headers is delimited by connection close, per
    /// <see href="https://www.rfc-editor.org/rfc/rfc9112#section-6.3">RFC 9112 6.3</see>: the body
    /// runs until you signal end of stream by calling <c>Execute</c> with an empty buffer. Use this
    /// when parsing HTTP/1.x responses off a stream where a dynamically sized body may arrive
    /// without a <c>Content-Length</c>.
    /// </summary>
    CloseDelimited = 1
}
