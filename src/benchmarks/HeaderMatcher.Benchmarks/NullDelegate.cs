using System;
using IHttpMachine;
using IHttpMachine.Model;

namespace HeaderMatcher.Benchmarks;

/// <summary>
/// A delegate that does nothing at all, so a parse benchmark reflects the parser's own cost.
/// </summary>
/// <remarks>
/// <para>
/// The ready-made <c>HttpParserDelegate</c> allocates a dictionary, a list per header and a
/// snapshot object per message. That cost is identical whichever parser is running and would swamp
/// the difference being measured, so the benchmarks report both: this delegate isolates the change,
/// and the real one shows what a consumer actually experiences.
/// </para>
/// <para>
/// This file is also compiled into the HEAD baseline project by
/// <c>tools/benchmark-against-head.sh</c>, so it must not reference anything added by this change.
/// </para>
/// </remarks>
internal sealed class NullDelegate : IHttpParserCombinedDelegate, IHttpParserSpanDelegate
{
    public HttpRequestResponse HttpRequestResponse => null!;

    public void OnMessageBegin(IHttpCombinedParser combinedParser) { }
    public void OnHeaderName(IHttpCombinedParser combinedParser, string name) { }
    public void OnHeaderValue(IHttpCombinedParser combinedParser, string value) { }
    public void OnHeadersEnd(IHttpCombinedParser combinedParser) { }
    public void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked) { }
    public void OnChunkedLength(IHttpCombinedParser combinedParser, int length) { }
    public void OnChunkReceived(IHttpCombinedParser combinedParser) { }
    public void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data) { }
    public void OnBody(IHttpCombinedParser combinedParser, ReadOnlySpan<byte> data) { }
    public void OnMessageEnd(IHttpCombinedParser combinedParser) { }
    public void OnParserError() { }
    public void OnMethod(IHttpCombinedParser combinedParser, string method) { }
    public void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri) { }
    public void OnPath(IHttpCombinedParser combinedParser, string path) { }
    public void OnFragment(IHttpCombinedParser combinedParser, string fragment) { }
    public void OnQueryString(IHttpCombinedParser combinedParser, string queryString) { }
    public void OnRequestType(IHttpCombinedParser combinedParser) { }
    public void OnResponseType(IHttpCombinedParser combinedParser) { }
    public void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason) { }
    public void Dispose() { }
}
