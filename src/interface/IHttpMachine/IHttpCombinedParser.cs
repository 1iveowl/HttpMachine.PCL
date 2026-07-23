using System;

namespace IHttpMachine;

/// <summary>
/// The combined HTTP request/response parser as seen from delegate callbacks.
/// </summary>
public interface IHttpCombinedParser : IDisposable
{
    /// <summary>HTTP major version of the current message (for example <c>1</c> for HTTP/1.1). <c>0</c> until the version has been parsed.</summary>
    int MajorVersion { get; }

    /// <summary>HTTP minor version of the current message (for example <c>1</c> for HTTP/1.1). <c>9</c> until the version has been parsed (HTTP/0.9).</summary>
    int MinorVersion { get; }

    /// <summary>
    /// Whether the connection should be kept open after the current message: for HTTP/1.1
    /// unless <c>Connection: close</c> was seen; for earlier versions only when
    /// <c>Connection: keep-alive</c> was seen.
    /// </summary>
    bool ShouldKeepAlive { get; }
}
