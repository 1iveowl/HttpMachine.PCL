using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using HttpMachine.Model.Internal;
using IHttpMachine;
using IHttpMachine.Model;

namespace HttpMachine;

/// <summary>
/// Ready-made parser delegate that collects each message into a
/// <see cref="IHttpMachine.Model.HttpRequestResponse"/> snapshot. Use it as-is, or subclass it
/// and override individual callbacks. The same instance can parse a sequence of pipelined or
/// keep-alive messages; state resets automatically when a new message begins, and
/// <see cref="HttpRequestResponse"/> always holds the most recently completed message.
/// </summary>
public class HttpParserDelegate : IHttpParserCombinedDelegate, IHttpParserSpanDelegate
{
    private InternalHttpRequestResponse _httpRequestResponse;
    private string _readingHeader;
    private readonly Dictionary<string, IList<string>> _headers;

    /// <inheritdoc/>
    public HttpRequestResponse HttpRequestResponse { get; private set; }

    /// <summary>Creates a delegate ready to be passed to a <see cref="HttpCombinedParser"/>.</summary>
    public HttpParserDelegate()
    {
        _httpRequestResponse = new InternalHttpRequestResponse();
        _headers = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public virtual void OnMessageBegin(IHttpCombinedParser combinedParser)
    {
        // Reset per-message state so the same delegate can parse pipelined/keep-alive messages.
        _httpRequestResponse = new InternalHttpRequestResponse();
        _headers.Clear();
        _readingHeader = null;
    }

    /// <inheritdoc/>
    public virtual void OnHeaderName(IHttpCombinedParser combinedParser, string headerName)
    {
        // Only update _readingHeader if it has changed to minimize garbage collection from creating strings.
        if (!string.Equals(_readingHeader, headerName, StringComparison.OrdinalIgnoreCase))
        {
            // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
            _readingHeader = headerName.ToUpperInvariant();
        }

        // The same header may reappear later in the message; append to the existing list.
        if (!_headers.ContainsKey(_readingHeader))
        {
            _headers.Add(_readingHeader, []);
        }
    }

    /// <inheritdoc/>
    public virtual void OnHeaderValue(IHttpCombinedParser combinedParser, string value)
    {
        if (_headers.TryGetValue(_readingHeader, out var list))
        {
            list.Add(value);
        }
        else
        {
            throw new InvalidOperationException("Something unexpected happened. Trying to add header value to undefined header.");
        }
    }

    /// <inheritdoc/>
    public virtual void OnHeadersEnd(IHttpCombinedParser combinedParser)
    {
        _httpRequestResponse.Headers = _headers.ToDictionary(
            d => d.Key,
            d => d.Value.AsEnumerable(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public virtual void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked)
    {
        _httpRequestResponse.IsTransferEncodingChunked = isChunked;
    }

    /// <inheritdoc/>
    public virtual void OnChunkedLength(IHttpCombinedParser combinedParser, int length)
    {
        _httpRequestResponse.ChunkSize = length;
    }

    /// <inheritdoc/>
    public virtual void OnChunkReceived(IHttpCombinedParser combinedParser)
    {

    }

    /// <summary>
    /// Receives body data from the array-based <c>Execute</c> overloads and appends it to
    /// <see cref="IHttpRequestResponse.Body"/>. Override the overload matching how you feed
    /// the parser (this one for array input, the span overload for span input).
    /// </summary>
    public virtual void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data)
    {
        _httpRequestResponse.Body.Write(data.Array, data.Offset, data.Count);
    }

    /// <summary>
    /// Receives body data from the span-based <c>Execute</c> overload and appends it to
    /// <see cref="IHttpRequestResponse.Body"/>. Override the overload matching how you feed
    /// the parser (this one for span input, the ArraySegment overload for array input).
    /// </summary>
    public virtual void OnBody(IHttpCombinedParser combinedParser, ReadOnlySpan<byte> data)
    {
#if NETSTANDARD2_0
        var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            data.CopyTo(buffer);
            _httpRequestResponse.Body.Write(buffer, 0, data.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
#else
        _httpRequestResponse.Body.Write(data);
#endif
    }

    /// <inheritdoc/>
    public virtual void OnParserError()
    {
        _httpRequestResponse.IsUnableToParseHttp = true;
    }

    /// <inheritdoc/>
    public virtual void OnMethod(IHttpCombinedParser combinedParser, string method)
    {
        _httpRequestResponse.Method = method;
    }

    /// <inheritdoc/>
    public virtual void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri)
    {
        _httpRequestResponse.RequestUri = requestUri;
    }

    /// <inheritdoc/>
    public virtual void OnPath(IHttpCombinedParser combinedParser, string path)
    {
        _httpRequestResponse.Path = path;
    }

    /// <inheritdoc/>
    public virtual void OnFragment(IHttpCombinedParser combinedParser, string fragment)
    {
        _httpRequestResponse.Fragment = fragment;
    }

    /// <inheritdoc/>
    public virtual void OnQueryString(IHttpCombinedParser combinedParser, string queryString)
    {
        _httpRequestResponse.QueryString = queryString;
    }

    /// <inheritdoc/>
    public virtual void OnRequestType(IHttpCombinedParser combinedParser)
    {
        _httpRequestResponse.MessageType = MessageTypeKind.Request;
    }

    /// <inheritdoc/>
    public virtual void OnResponseType(IHttpCombinedParser combinedParser)
    {
        _httpRequestResponse.MessageType = MessageTypeKind.Response;
    }

    /// <inheritdoc/>
    public virtual void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason)
    {
        _httpRequestResponse.ResponseReason = statusReason;
        _httpRequestResponse.StatusCode = statusCode;
    }

    /// <summary>
    /// Publishes the completed message as a new <see cref="HttpRequestResponse"/> snapshot,
    /// unless a parser error occurred.
    /// </summary>
    public virtual void OnMessageEnd(IHttpCombinedParser combinedParser)
    {
        if (!_httpRequestResponse.IsUnableToParseHttp)
        {
            _httpRequestResponse.IsEndOfMessage = true;

            _httpRequestResponse.MajorVersion = combinedParser.MajorVersion;
            _httpRequestResponse.MinorVersion = combinedParser.MinorVersion;
            _httpRequestResponse.ShouldKeepAlive = combinedParser.ShouldKeepAlive;

            HttpRequestResponse = new(_httpRequestResponse);
        }
    }

    /// <summary>Releases resources. The base implementation holds none.</summary>
    public virtual void Dispose()
    {

    }
}
