namespace HttpMachine;

/// <summary>
/// Controls the header-name string the parser reports to
/// <see cref="IHttpMachine.IHttpParserDelegate.OnHeaderName"/>. HTTP field names are
/// case-insensitive
/// (<see href="https://www.rfc-editor.org/rfc/rfc9110#section-5.1">RFC 9110 5.1</see>), so the two
/// modes differ only in which spelling you are handed - never in which headers are recognised.
/// </summary>
public enum HeaderNameMode
{
    /// <summary>
    /// Report the name exactly as it appeared on the wire, so <c>CONTENT-LENGTH:</c> is reported as
    /// <c>"CONTENT-LENGTH"</c>. This is the default and matches the library's historical behaviour.
    /// </summary>
    /// <remarks>
    /// Recognised headers whose bytes already match the canonical spelling are still reported
    /// without allocating, using a cached instance - the string content is identical either way, so
    /// this costs nothing and is not observable. Only a header sent in some other casing has to be
    /// materialised.
    /// </remarks>
    AsReceived = 0,

    /// <summary>
    /// Report a canonical, cached spelling for every recognised header regardless of how the sender
    /// wrote it, so both <c>CONTENT-LENGTH:</c> and <c>content-length:</c> are reported as
    /// <c>"Content-Length"</c>. Unrecognised headers are still reported as received.
    /// </summary>
    /// <remarks>
    /// Allocation-free for every recognised header, which the default cannot be for senders that
    /// use non-canonical casing - UPnP devices, for example, commonly send <c>LOCATION:</c> and
    /// <c>CACHE-CONTROL:</c>. Choose this unless you need to see the sender's exact casing.
    /// </remarks>
    Canonical = 1
}
