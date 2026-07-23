using System;
using IHttpMachine.Model;

namespace IHttpMachine;

/// <summary>
/// Receives callbacks from the parser as an HTTP message is parsed. For a single message the
/// callbacks arrive in this order: <see cref="OnMessageBegin"/>, the request or response line
/// callbacks (see <see cref="IHttpRequestParserDelegate"/> and <see cref="IHttpResponseParserDelegate"/>),
/// alternating <see cref="OnHeaderName"/>/<see cref="OnHeaderValue"/> pairs, <see cref="OnHeadersEnd"/>,
/// zero or more <see cref="OnBody"/> calls, and finally <see cref="OnMessageEnd"/>.
/// Instead of implementing this interface directly, consider subclassing the ready-made
/// <c>HttpMachine.HttpParserDelegate</c> and overriding only the callbacks you need.
/// </summary>
public interface IHttpParserDelegate
{
    /// <summary>
    /// Snapshot of the most recently completed message, or <c>null</c> until the first
    /// message has been fully parsed.
    /// </summary>
    HttpRequestResponse HttpRequestResponse { get; }

    /// <summary>Called when the first byte of a new message arrives.</summary>
    void OnMessageBegin(IHttpCombinedParser combinedParser);

    /// <summary>Called with each header field name, before the corresponding <see cref="OnHeaderValue"/>.</summary>
    void OnHeaderName(IHttpCombinedParser combinedParser, string name);

    /// <summary>Called with each header field value, after the corresponding <see cref="OnHeaderName"/>.</summary>
    void OnHeaderValue(IHttpCombinedParser combinedParser, string value);

    /// <summary>Called when the blank line ending the header section has been parsed.</summary>
    void OnHeadersEnd(IHttpCombinedParser combinedParser);

    /// <summary>Called when a <c>Transfer-Encoding: chunked</c> header has been parsed.</summary>
    void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked);

    /// <summary>Called with the size of each chunk when the body uses chunked transfer encoding.</summary>
    void OnChunkedLength(IHttpCombinedParser combinedParser, int length);

    /// <summary>Called when a chunk of a chunked body starts being delivered.</summary>
    void OnChunkReceived(IHttpCombinedParser combinedParser);

    /// <summary>
    /// Called with body data. The segment points into the buffer passed to <c>Execute</c> (or a
    /// pooled copy for span input) and is only valid for the duration of the callback — copy the
    /// bytes if you need to keep them. May be called multiple times per message.
    /// </summary>
    void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data);

    /// <summary>Called when the message is complete, including its body.</summary>
    void OnMessageEnd(IHttpCombinedParser combinedParser);

    /// <summary>Called when the parser fails to parse the input; parsing stops at the failing byte.</summary>
    void OnParserError();
}
