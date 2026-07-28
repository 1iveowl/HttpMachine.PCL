using System;
using System.Text;
using IHttpMachine;
using IHttpMachine.Model;

namespace HttpMachine.Tests.Fixtures;

/// <summary>
/// Records every parser callback, in order, as human-readable text for snapshotting.
/// </summary>
/// <remarks>
/// <para>
/// This exists because the parser's actual contract is the <em>sequence</em> of callbacks, and
/// almost nothing else in the suite tests it. Assertions on
/// <see cref="HttpParserDelegate.HttpRequestResponse"/> check the ready-made delegate's accumulated
/// result, which is a lossy projection: it cannot tell you that <c>OnHeadersEnd</c> preceded the
/// first <c>OnBody</c>, that names and values strictly alternated, or that <c>OnMessageEnd</c> fired
/// exactly once.
/// </para>
/// <para>
/// Deliberately implements the interfaces directly rather than deriving from
/// <see cref="HttpParserDelegate"/>, so nothing is filtered or accumulated on the way through - and
/// so this doubles as a test that the interface is implementable standalone, which is how
/// SimpleHttpListener.Rx and SSDP.UPnP.PCL consume the parser.
/// </para>
/// </remarks>
internal sealed class RecordingParserDelegate : IHttpParserCombinedDelegate, IHttpParserSpanDelegate
{
    // Wide enough for the longest callback name, OnTransferEncodingChunked.
    private const int PayloadColumn = 28;

    private readonly StringBuilder _log = new();

    /// <summary>Whether <see cref="OnRequestType"/> was raised, for the category assertion.</summary>
    public bool SawRequest { get; private set; }

    /// <summary>Whether <see cref="OnResponseType"/> was raised, for the category assertion.</summary>
    public bool SawResponse { get; private set; }

    /// <summary>Whether <see cref="OnParserError"/> was raised.</summary>
    /// <remarks>
    /// Only raised when an exception escapes a parser action - a duplicate <c>Content-Length</c>,
    /// for instance. Input the grammar simply rejects does not raise it; the state machine stops and
    /// <c>Execute</c> returns a short count. See <see cref="AcceptedAllInput"/>.
    /// </remarks>
    public bool SawParserError { get; private set; }

    /// <summary>
    /// Whether every <c>Execute</c> call consumed everything it was given.
    /// </summary>
    /// <remarks>
    /// This, not <see cref="SawParserError"/>, is how grammar rejection shows up - which is the more
    /// common shape of "malformed". Both together are what <c>.invalid</c> means.
    /// </remarks>
    public bool AcceptedAllInput { get; private set; } = true;

    private int _pendingByteCount;

    /// <inheritdoc/>
    public HttpRequestResponse HttpRequestResponse => null!;

    public string Log => _log.ToString();

    /// <summary>
    /// Brackets one <c>Execute</c> call in the log, so it is unambiguous which callbacks came from
    /// which call. That matters for fixtures using the <c>eof</c> modifier, where the callbacks
    /// raised by the second, empty call are the entire point of the fixture.
    /// </summary>
    public void BeginExecute(int byteCount, string note = "")
    {
        if (_log.Length > 0)
        {
            _log.AppendLine();
        }

        _pendingByteCount = byteCount;
        _log.Append("> Execute ").Append(byteCount).Append(" bytes").Append(note).AppendLine();
    }

    public void EndExecute(int consumed)
    {
        if (consumed != _pendingByteCount)
        {
            AcceptedAllInput = false;
        }

        _log.Append("< Execute returned ").Append(consumed).AppendLine();
    }

    public void RecordException(Exception exception) =>
        _log.Append("! ").Append(exception.GetType().FullName).Append(": ").AppendLine(exception.Message);

    public void RecordParserState(IHttpCombinedParser parser) =>
        _log.AppendLine()
            .Append("Parser: HTTP/").Append(parser.MajorVersion).Append('.').Append(parser.MinorVersion)
            .Append(", ShouldKeepAlive=").Append(parser.ShouldKeepAlive)
            .AppendLine();

    // -------------------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------------------

    private void Record(string callback) => _log.AppendLine(callback);

    private void Record(string callback, string payload)
    {
        // PadRight is a no-op once the name is longer than the column, which would run the payload
        // straight into the name. Guarantee a separator either way.
        _log.Append(callback.Length >= PayloadColumn ? callback + " " : callback.PadRight(PayloadColumn))
            .AppendLine(payload);
    }

    /// <summary>
    /// Renders a string payload with control characters escaped and the whole thing quoted.
    /// </summary>
    /// <remarks>
    /// Escaping rather than embedding real newlines keeps one callback on one line, and keeps the
    /// snapshot immune to the end-of-line normalisation that the fixtures themselves are protected
    /// from by .gitattributes. Quoting makes leading and trailing whitespace visible, which is
    /// exactly the sort of thing a header-value bug produces.
    /// </remarks>
    private static string Text(string value)
    {
        var builder = new StringBuilder(value.Length + 2).Append('"');

        foreach (var c in value)
        {
            switch (c)
            {
                case '\r': builder.Append("\\r"); break;
                case '\n': builder.Append("\\n"); break;
                case '\t': builder.Append("\\t"); break;
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                default:
                    if (c < 0x20 || c == 0x7F)
                    {
                        builder.Append("\\x").Append(((int)c).ToString("X2"));
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
            }
        }

        return builder.Append('"').ToString();
    }

    /// <summary>
    /// Text where it is text, hex where it is not. Body bytes are arbitrary, so a fixture with a
    /// binary or truncated-UTF8 body must still produce a readable, stable snapshot.
    /// </summary>
    private void RecordBody(ReadOnlySpan<byte> data)
    {
        var isText = true;

        foreach (var b in data)
        {
            if (b is not ((>= 0x20 and < 0x7F) or (byte)'\r' or (byte)'\n' or (byte)'\t'))
            {
                isText = false;
                break;
            }
        }

        if (isText)
        {
            var chars = new char[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                chars[i] = (char)data[i];
            }

            Record("OnBody", $"{data.Length} bytes  {Text(new string(chars))}");
            return;
        }

        Record("OnBody", $"{data.Length} bytes");
        AppendHexDump(data);
    }

    private void AppendHexDump(ReadOnlySpan<byte> data)
    {
        var indent = new string(' ', PayloadColumn);

        for (var offset = 0; offset < data.Length; offset += 16)
        {
            var line = data.Slice(offset, Math.Min(16, data.Length - offset));
            var hex = new StringBuilder();
            var ascii = new StringBuilder();

            for (var i = 0; i < 16; i++)
            {
                if (i < line.Length)
                {
                    hex.Append(line[i].ToString("X2")).Append(' ');
                    ascii.Append(line[i] is >= 0x20 and < 0x7F ? (char)line[i] : '.');
                }
                else
                {
                    hex.Append("   ");
                }
            }

            _log.Append(indent)
                .Append(offset.ToString("X8")).Append("  ")
                .Append(hex)
                .Append(" |").Append(ascii).AppendLine("|");
        }
    }

    // -------------------------------------------------------------------------------------
    // Callbacks
    // -------------------------------------------------------------------------------------

    public void OnMessageBegin(IHttpCombinedParser combinedParser) => Record("OnMessageBegin");

    public void OnRequestType(IHttpCombinedParser combinedParser)
    {
        SawRequest = true;
        Record("OnRequestType");
    }

    public void OnResponseType(IHttpCombinedParser combinedParser)
    {
        SawResponse = true;
        Record("OnResponseType");
    }

    public void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason) =>
        Record("OnResponseCode", $"{statusCode} {Text(statusReason ?? string.Empty)}");

    public void OnMethod(IHttpCombinedParser combinedParser, string method) => Record("OnMethod", Text(method));

    public void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri) => Record("OnRequestUri", Text(requestUri));

    public void OnPath(IHttpCombinedParser combinedParser, string path) => Record("OnPath", Text(path));

    public void OnFragment(IHttpCombinedParser combinedParser, string fragment) => Record("OnFragment", Text(fragment));

    public void OnQueryString(IHttpCombinedParser combinedParser, string queryString) => Record("OnQueryString", Text(queryString));

    public void OnHeaderName(IHttpCombinedParser combinedParser, string name) => Record("OnHeaderName", Text(name));

    public void OnHeaderValue(IHttpCombinedParser combinedParser, string value) => Record("OnHeaderValue", Text(value));

    public void OnHeadersEnd(IHttpCombinedParser combinedParser) => Record("OnHeadersEnd");

    public void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked) =>
        Record("OnTransferEncodingChunked", isChunked.ToString());

    public void OnChunkedLength(IHttpCombinedParser combinedParser, int length) =>
        Record("OnChunkedLength", length.ToString());

    public void OnChunkReceived(IHttpCombinedParser combinedParser) => Record("OnChunkReceived");

    public void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data) =>
        RecordBody(new ReadOnlySpan<byte>(data.Array!, data.Offset, data.Count));

    public void OnBody(IHttpCombinedParser combinedParser, ReadOnlySpan<byte> data) => RecordBody(data);

    public void OnMessageEnd(IHttpCombinedParser combinedParser) => Record("OnMessageEnd");

    public void OnParserError()
    {
        SawParserError = true;
        Record("OnParserError");
    }

    public void Dispose()
    {
    }
}
