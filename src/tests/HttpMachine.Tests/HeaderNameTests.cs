using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HttpMachine;
using IHttpMachine;
using Xunit;

namespace HttpMachine.Tests;

/// <summary>
/// Covers the header-name path: what <see cref="IHttpParserDelegate.OnHeaderName"/> is handed, and
/// that the framing headers still drive framing now that they are recognised by generated code
/// rather than by the Ragel grammar.
/// </summary>
public class HeaderNameTests
{
    /// <summary>Records the header names exactly as the parser reports them.</summary>
    private sealed class RecordingDelegate : HttpParserDelegate
    {
        public List<string> HeaderNames { get; } = [];

        public bool ErrorReported { get; private set; }

        public override void OnHeaderName(IHttpCombinedParser combinedParser, string headerName)
        {
            HeaderNames.Add(headerName);
            base.OnHeaderName(combinedParser, headerName);
        }

        public override void OnParserError()
        {
            ErrorReported = true;
            base.OnParserError();
        }
    }

    /// <summary>
    /// The canonical spellings, read from the same file the matcher is generated from. Driving the
    /// tests from a hand-copied list would let the two drift apart silently.
    /// </summary>
    public static readonly string[] KnownHeaderNames = File.ReadAllLines("KnownHeaders.txt")
        .Select(line => line.Trim())
        .Where(line => line.Length > 0 && line[0] != '#')
        .Select(line => line.Contains('=') ? line.Split('=')[0].Trim() : line)
        .ToArray();

    public static TheoryData<string> AllKnownHeaders()
    {
        var data = new TheoryData<string>();

        foreach (var name in KnownHeaderNames)
        {
            data.Add(name);
        }

        return data;
    }

    private static List<string> ParseHeaderNames(string message, HeaderNameMode mode)
    {
        var handler = new RecordingDelegate();
        using var parser = new HttpCombinedParser(handler, mode);

        parser.Execute(Encoding.UTF8.GetBytes(message));

        return handler.HeaderNames;
    }

    private static string ResponseWith(string headerName) =>
        $"HTTP/1.1 200 OK\r\n{headerName}: v\r\nContent-Length: 0\r\n\r\n";

    private static IEnumerable<string> CasingVariants(string name)
    {
        yield return name;
        yield return name.ToUpperInvariant();
        yield return name.ToLowerInvariant();
    }

    // ---------------------------------------------------------------------------------------
    // Casing
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// The promise made to existing consumers: the default mode reports the name byte-for-byte as
    /// the sender wrote it. Asserted ordinally over every known header in every casing, because
    /// this is the regression that a cached canonical instance would quietly introduce.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllKnownHeaders))]
    public void AsReceivedReportsTheWireCasing(string canonical)
    {
        foreach (var variant in CasingVariants(canonical))
        {
            var names = ParseHeaderNames(ResponseWith(variant), HeaderNameMode.AsReceived);

            Assert.Equal(variant, names[0], StringComparer.Ordinal);
        }
    }

    /// <summary>The opt-in: one canonical spelling regardless of what the sender wrote.</summary>
    [Theory]
    [MemberData(nameof(AllKnownHeaders))]
    public void CanonicalReportsTheCanonicalSpelling(string canonical)
    {
        foreach (var variant in CasingVariants(canonical))
        {
            var names = ParseHeaderNames(ResponseWith(variant), HeaderNameMode.Canonical);

            Assert.Equal(canonical, names[0], StringComparer.Ordinal);
        }
    }

    [Theory]
    [InlineData("X-Vendor-Thing")]
    [InlineData("X-VENDOR-THING")]
    [InlineData("x-vendor-thing")]
    [InlineData("Weird")]
    public void UnknownHeadersKeepWireCasingInBothModes(string name)
    {
        Assert.Equal(name, ParseHeaderNames(ResponseWith(name), HeaderNameMode.AsReceived)[0], StringComparer.Ordinal);
        Assert.Equal(name, ParseHeaderNames(ResponseWith(name), HeaderNameMode.Canonical)[0], StringComparer.Ordinal);
    }

    /// <summary>The dictionary keys are uppercased, so neither mode changes what consumers see.</summary>
    [Theory]
    [InlineData(HeaderNameMode.AsReceived)]
    [InlineData(HeaderNameMode.Canonical)]
    public void HeadersDictionaryIsUnaffectedByMode(HeaderNameMode mode)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler, mode);

        parser.Execute(Encoding.UTF8.GetBytes(
            "HTTP/1.1 200 OK\r\nCONTENT-TYPE: text/plain\r\ncontent-length: 0\r\n\r\n"));

        Assert.Equal("text/plain", handler.HttpRequestResponse.Headers["CONTENT-TYPE"].Single());
        Assert.Equal("text/plain", handler.HttpRequestResponse.Headers["content-type"].Single());
    }

    // ---------------------------------------------------------------------------------------
    // Split buffers - the property the old StringBuilder accumulator got for free
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// A header name may be split across <c>Execute</c> calls at any byte. The byte accumulator
    /// that replaced the StringBuilder has to survive that, so every split point is exercised
    /// rather than a representative one.
    /// </summary>
    [Theory]
    [InlineData(HeaderNameMode.AsReceived)]
    [InlineData(HeaderNameMode.Canonical)]
    public void HeaderNameMaySplitAcrossExecuteCallsAtAnyOffset(HeaderNameMode mode)
    {
        const string Message = "HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\n4\r\nWiki\r\n0\r\n\r\n";
        var bytes = Encoding.UTF8.GetBytes(Message);

        for (var split = 0; split <= bytes.Length; split++)
        {
            var handler = new RecordingDelegate();
            using var parser = new HttpCombinedParser(handler, mode);

            parser.Execute(bytes.AsSpan(0, split));
            parser.Execute(bytes.AsSpan(split));

            Assert.Equal(["Transfer-Encoding"], handler.HeaderNames);
            Assert.True(handler.HttpRequestResponse.IsTransferEncodingChunked, $"split at {split}");

            handler.HttpRequestResponse.Body.Position = 0;
            Assert.Equal("Wiki", new StreamReader(handler.HttpRequestResponse.Body).ReadToEnd());
        }
    }

    /// <summary>
    /// Names longer than the accumulator's initial 64 bytes must grow it, including when the growth
    /// happens mid-name across two Execute calls.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(63)]
    [InlineData(64)]
    [InlineData(65)]
    [InlineData(200)]
    [InlineData(5000)]
    public void LongHeaderNamesGrowTheAccumulator(int length)
    {
        var name = new string('a', length);
        var bytes = Encoding.UTF8.GetBytes(ResponseWith(name));

        // Split inside the name, so the buffer grows across a call boundary as well as within one.
        var split = 20;

        var handler = new RecordingDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(bytes.AsSpan(0, split));
        parser.Execute(bytes.AsSpan(split));

        Assert.Equal(name, handler.HeaderNames[0], StringComparer.Ordinal);
    }

    /// <summary>The accumulator is reused across messages, so a long name must not leak into a short one.</summary>
    [Fact]
    public void AccumulatorIsResetBetweenHeadersAndMessages()
    {
        var handler = new RecordingDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(Encoding.UTF8.GetBytes(
            $"HTTP/1.1 200 OK\r\n{new string('a', 100)}: v\r\nContent-Length: 0\r\n\r\n"));
        parser.Execute(Encoding.UTF8.GetBytes(
            "HTTP/1.1 200 OK\r\nST: v\r\nContent-Length: 0\r\n\r\n"));

        Assert.Equal([new string('a', 100), "Content-Length", "ST", "Content-Length"], handler.HeaderNames);
    }

    // ---------------------------------------------------------------------------------------
    // Framing, now decided by the generated matcher rather than by the grammar
    // ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData("Content-Length")]
    [InlineData("content-length")]
    [InlineData("CONTENT-LENGTH")]
    [InlineData("CoNtEnT-LeNgTh")]
    public void ContentLengthIsRecognisedInAnyCasing(string name)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n{name}: 5\r\n\r\nHELLO"));

        handler.HttpRequestResponse.Body.Position = 0;
        Assert.Equal("HELLO", new StreamReader(handler.HttpRequestResponse.Body).ReadToEnd());
    }

    [Theory]
    [InlineData("Transfer-Encoding")]
    [InlineData("transfer-encoding")]
    [InlineData("TRANSFER-ENCODING")]
    public void TransferEncodingIsRecognisedInAnyCasing(string name)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(Encoding.UTF8.GetBytes(
            $"HTTP/1.1 200 OK\r\n{name}: chunked\r\n\r\n4\r\nWiki\r\n0\r\n\r\n"));

        Assert.True(handler.HttpRequestResponse.IsTransferEncodingChunked);
    }

    [Theory]
    [InlineData("Connection", "close", false)]
    [InlineData("CONNECTION", "close", false)]
    [InlineData("connection", "keep-alive", true)]
    public void ConnectionIsRecognisedInAnyCasing(string name, string value, bool shouldKeepAlive)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(Encoding.UTF8.GetBytes(
            $"HTTP/1.0 200 OK\r\n{name}: {value}\r\nContent-Length: 0\r\n\r\n"));

        Assert.Equal(shouldKeepAlive, handler.HttpRequestResponse.ShouldKeepAlive);
    }

    /// <summary>
    /// A header whose name merely starts with a framing header's name is not that header. Verified
    /// against the pre-change parser before the grammar was touched, so this locks in existing
    /// behaviour rather than asserting the new implementation's preference.
    /// </summary>
    [Theory]
    [InlineData("Content-LengthX")]
    [InlineData("Content-Lengt")]
    [InlineData("XContent-Length")]
    public void HeadersThatMerelyResembleContentLengthAreNotFraming(string name)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler);

        parser.Execute(Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n{name}: 5\r\n\r\n"));

        // No framing header means a bodyless message under the default CompleteAtHeaders mode.
        Assert.Equal(0, handler.HttpRequestResponse.Body.Length);
    }

    /// <summary>
    /// Two Content-Length headers remain a parse error rather than a silent overwrite - the guard
    /// moved from the Ragel action into ApplyKnownHeader, and it still fires on the second name
    /// regardless of casing. Note the message is abandoned, so no snapshot is ever published and
    /// the error is only observable through <c>OnParserError</c> and the consumed count.
    /// </summary>
    [Fact]
    public void DuplicateContentLengthIsAParserError()
    {
        var handler = new RecordingDelegate();
        using var parser = new HttpCombinedParser(handler);

        var message = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 5\r\nCONTENT-LENGTH: 6\r\n\r\nHELLO");
        var consumed = parser.Execute(message);

        Assert.True(handler.ErrorReported);
        Assert.True(consumed < message.Length);
        Assert.Null(handler.HttpRequestResponse);
    }

    // ---------------------------------------------------------------------------------------
    // SSDP / UPnP, the traffic this library is most used for
    // ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData(HeaderNameMode.AsReceived)]
    [InlineData(HeaderNameMode.Canonical)]
    public void SsdpNotifyParsesIdenticallyInBothModes(HeaderNameMode mode)
    {
        var handler = new HttpParserDelegate();
        using var parser = new HttpCombinedParser(handler, mode);

        var message = Encoding.UTF8.GetBytes(
            "NOTIFY * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "CACHE-CONTROL: max-age=1800\r\n" +
            "LOCATION: http://192.168.1.10:80/description.xml\r\n" +
            "NT: upnp:rootdevice\r\n" +
            "NTS: ssdp:alive\r\n" +
            "SERVER: Linux/5.10 UPnP/2.0 Example/1.0\r\n" +
            "USN: uuid:abcd::upnp:rootdevice\r\n" +
            "BOOTID.UPNP.ORG: 1\r\n" +
            "EXT:\r\n" +
            "\r\n");

        var consumed = parser.Execute(message);
        var headers = handler.HttpRequestResponse.Headers;

        Assert.Equal(message.Length, consumed);
        Assert.Equal(MessageTypeKind.Request, handler.HttpRequestResponse.MessageType);
        Assert.Equal("NOTIFY", handler.HttpRequestResponse.Method);
        Assert.Equal("ssdp:alive", headers["NTS"].Single());
        Assert.Equal("uuid:abcd::upnp:rootdevice", headers["USN"].Single());
        Assert.Equal("1", headers["BOOTID.UPNP.ORG"].Single());
        Assert.Equal("", headers["EXT"].Single());
    }
}
