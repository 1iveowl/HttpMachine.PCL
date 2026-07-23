using System;
using System.IO;
using System.Linq;
using System.Text;
using HttpMachine;
using IHttpMachine;
using Xunit;

namespace HttpMachine.Tests;

public class HttpCombinedParserTests
{
    private static byte[] Bytes(string s) => Encoding.UTF8.GetBytes(s);

    private static string BodyOf(HttpParserDelegate handler)
    {
        var body = handler.HttpRequestResponse.Body;
        body.Position = 0;
        return new StreamReader(body).ReadToEnd();
    }

    private static (HttpParserDelegate handler, HttpCombinedParser parser, int parsed, int length) Parse(string message)
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);
        var data = Bytes(message);
        var parsed = parser.Execute(data);
        return (handler, parser, parsed, data.Length);
    }

    [Fact]
    public void ParsesSimpleResponse()
    {
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/plain\r\n" +
            "Content-Length: 14\r\n" +
            "\r\n" +
            "This is a test");

        Assert.Equal(length, parsed);
        Assert.Equal(MessageTypeKind.Response, handler.HttpRequestResponse.MessageType);
        Assert.Equal(200, handler.HttpRequestResponse.StatusCode);
        Assert.Equal("OK", handler.HttpRequestResponse.ResponseReason);
        Assert.Equal(1, handler.HttpRequestResponse.MajorVersion);
        Assert.Equal(1, handler.HttpRequestResponse.MinorVersion);
        Assert.Equal("text/plain", handler.HttpRequestResponse.Headers["CONTENT-TYPE"].Single());
        Assert.Equal("This is a test", BodyOf(handler));
    }

    [Fact]
    public void ParsesRequest()
    {
        var (handler, _, parsed, length) = Parse(
            "GET /path/file.html?foo=bar HTTP/1.1\r\n" +
            "Host: example.com\r\n" +
            "\r\n");

        Assert.Equal(length, parsed);
        Assert.Equal(MessageTypeKind.Request, handler.HttpRequestResponse.MessageType);
        Assert.Equal("GET", handler.HttpRequestResponse.Method);
        Assert.Equal("/path/file.html?foo=bar", handler.HttpRequestResponse.RequestUri);
        Assert.Equal("/path/file.html", handler.HttpRequestResponse.Path);
        Assert.Equal("foo=bar", handler.HttpRequestResponse.QueryString);
    }

    [Fact]
    public void ParsesChunkedResponse()
    {
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            "4\r\nWiki\r\n" +
            "6\r\npedia \r\n" +
            "E\r\nin \r\n\r\nchunks.\r\n" +
            "0\r\n\r\n");

        Assert.Equal(length, parsed);
        Assert.True(handler.HttpRequestResponse.IsTransferEncodingChunked);
        Assert.Equal("Wikipedia in \r\n\r\nchunks.", BodyOf(handler));
    }

    [Fact]
    public void CollectsAdjacentDuplicateHeaders()
    {
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "ST: first\r\n" +
            "ST: second\r\n" +
            "\r\n");

        Assert.Equal(length, parsed);
        Assert.Equal(["first", "second"], handler.HttpRequestResponse.Headers["ST"]);
    }

    [Fact]
    public void CollectsNonAdjacentDuplicateHeaders()
    {
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "ST: first\r\n" +
            "USN: something\r\n" +
            "ST: second\r\n" +
            "\r\n");

        Assert.Equal(length, parsed);
        Assert.Equal(["first", "second"], handler.HttpRequestResponse.Headers["ST"]);
    }

    [Fact]
    public void HeaderLookupIsCaseInsensitive()
    {
        var (handler, _, _, _) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/plain\r\n" +
            "\r\n");

        Assert.Equal("text/plain", handler.HttpRequestResponse.Headers["content-type"].Single());
    }

    [Fact]
    public void ParsesZeroLengthHeaderValue()
    {
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "EXT:\r\n" +
            "\r\n");

        Assert.Equal(length, parsed);
        Assert.True(handler.HttpRequestResponse.Headers.ContainsKey("EXT"));
    }

    [Fact]
    public void ParsesMessageSplitAcrossBuffers()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);

        var part1 = Bytes("HTTP/1.1 200 OK\r\nContent-Le");
        var part2 = Bytes("ngth: 5\r\n\r\nHello");

        Assert.Equal(part1.Length, parser.Execute(part1));
        Assert.Equal(part2.Length, parser.Execute(part2));
        Assert.Equal(200, handler.HttpRequestResponse.StatusCode);
        Assert.Equal("Hello", BodyOf(handler));
    }

    [Fact]
    public void ReportsErrorOnGarbage()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);
        var data = Bytes("\0garbage\0garbage\0\r\n\r\n");

        Assert.NotEqual(data.Length, parser.Execute(data));
    }

    [Theory]
    [InlineData("HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n", true)]
    [InlineData("HTTP/1.1 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n", false)]
    [InlineData("HTTP/1.0 200 OK\r\nContent-Length: 0\r\nConnection: keep-alive\r\n\r\n", true)]
    [InlineData("HTTP/1.0 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n", false)]
    public void ComputesShouldKeepAlive(string message, bool expected)
    {
        var (handler, _, parsed, length) = Parse(message);

        Assert.Equal(length, parsed);
        Assert.Equal(expected, handler.HttpRequestResponse.ShouldKeepAlive);
    }

    [Fact]
    public void ParsesPipelinedRequestsWithSameDelegate()
    {
        var (handler, _, parsed, length) = Parse(
            "GET /first HTTP/1.1\r\n" +
            "Host: example.com\r\n" +
            "\r\n" +
            "GET /second HTTP/1.1\r\n" +
            "Accept: text/html\r\n" +
            "\r\n");

        Assert.Equal(length, parsed);
        Assert.Equal("/second", handler.HttpRequestResponse.Path);
        Assert.False(handler.HttpRequestResponse.Headers.ContainsKey("HOST"));
        Assert.Equal("text/html", handler.HttpRequestResponse.Headers["ACCEPT"].Single());
    }

    [Fact]
    public void ParsesEofTerminatedBody()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);

        var data = Bytes(
            "HTTP/1.0 200 OK\r\n" +
            "Connection: close\r\n" +
            "\r\n" +
            "body until close");

        Assert.Equal(data.Length, parser.Execute(data));
        parser.Execute(Array.Empty<byte>());

        Assert.Equal("body until close", BodyOf(handler));
        Assert.True(handler.HttpRequestResponse.IsEndOfMessage);
    }

    [Fact]
    public void ExecuteAcceptsSpanInput()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);
        ReadOnlySpan<byte> span = Bytes(
            "GET /index.html HTTP/1.1\r\n" +
            "Host: example.com\r\n" +
            "\r\n");

        Assert.Equal(span.Length, parser.Execute(span));
        Assert.Equal(MessageTypeKind.Request, handler.HttpRequestResponse.MessageType);
        Assert.Equal("/index.html", handler.HttpRequestResponse.Path);
    }

    [Fact]
    public void SpanInputSplitAcrossExecutes()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);

        ReadOnlySpan<byte> part1 = Bytes("HTTP/1.1 200 OK\r\nContent-Le");
        ReadOnlySpan<byte> part2 = Bytes("ngth: 5\r\n\r\nHello");

        Assert.Equal(part1.Length, parser.Execute(part1));
        Assert.Equal(part2.Length, parser.Execute(part2));
        Assert.Equal(200, handler.HttpRequestResponse.StatusCode);
        Assert.Equal("Hello", BodyOf(handler));
    }

    [Fact]
    public void ParsesChunkedResponseFromSpanInput()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);

        ReadOnlySpan<byte> data = Bytes(
            "HTTP/1.1 200 OK\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            "4\r\nWiki\r\n" +
            "6\r\npedia \r\n" +
            "E\r\nin \r\n\r\nchunks.\r\n" +
            "0\r\n\r\n");

        Assert.Equal(data.Length, parser.Execute(data));
        Assert.True(handler.HttpRequestResponse.IsTransferEncodingChunked);
        Assert.Equal("Wikipedia in \r\n\r\nchunks.", BodyOf(handler));
    }

    [Fact]
    public void ExecuteAcceptsMemoryStream()
    {
        var handler = new HttpParserDelegate();
        var parser = new HttpCombinedParser(handler);
        var bytes = Bytes("HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nhi");
        using var stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);

        Assert.Equal(bytes.Length, parser.Execute(stream));
        Assert.Equal("hi", BodyOf(handler));
    }

    private sealed class ArraySegmentCapturingDelegate : HttpParserDelegate
    {
        public readonly MemoryStream Captured = new();

        public override void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data)
        {
            Captured.Write(data.Array, data.Offset, data.Count);
            base.OnBody(combinedParser, data);
        }
    }

    private sealed class SpanCapturingDelegate : HttpParserDelegate
    {
        public readonly MemoryStream Captured = new();

        public override void OnBody(IHttpCombinedParser combinedParser, ReadOnlySpan<byte> data)
        {
            Captured.Write(data);
            base.OnBody(combinedParser, data);
        }
    }

    // Implements only the 5.x interfaces - no IHttpParserSpanDelegate.
    private sealed class LegacyInterfaceDelegate : IHttpMachine.IHttpParserCombinedDelegate
    {
        public readonly MemoryStream Captured = new();

        public IHttpMachine.Model.HttpRequestResponse HttpRequestResponse => null;
        public void OnMessageBegin(IHttpMachine.IHttpCombinedParser p) { }
        public void OnHeaderName(IHttpMachine.IHttpCombinedParser p, string name) { }
        public void OnHeaderValue(IHttpMachine.IHttpCombinedParser p, string value) { }
        public void OnHeadersEnd(IHttpMachine.IHttpCombinedParser p) { }
        public void OnTransferEncodingChunked(IHttpMachine.IHttpCombinedParser p, bool isChunked) { }
        public void OnChunkedLength(IHttpMachine.IHttpCombinedParser p, int length) { }
        public void OnChunkReceived(IHttpMachine.IHttpCombinedParser p) { }
        public void OnBody(IHttpMachine.IHttpCombinedParser p, ArraySegment<byte> data) => Captured.Write(data.Array, data.Offset, data.Count);
        public void OnMessageEnd(IHttpMachine.IHttpCombinedParser p) { }
        public void OnParserError() { }
        public void OnRequestType(IHttpMachine.IHttpCombinedParser p) { }
        public void OnMethod(IHttpMachine.IHttpCombinedParser p, string method) { }
        public void OnRequestUri(IHttpMachine.IHttpCombinedParser p, string requestUri) { }
        public void OnPath(IHttpMachine.IHttpCombinedParser p, string path) { }
        public void OnFragment(IHttpMachine.IHttpCombinedParser p, string fragment) { }
        public void OnQueryString(IHttpMachine.IHttpCombinedParser p, string queryString) { }
        public void OnResponseType(IHttpMachine.IHttpCombinedParser p) { }
        public void OnResponseCode(IHttpMachine.IHttpCombinedParser p, int statusCode, string statusReason) { }
        public void Dispose() { }
    }

    private const string BodyMessage =
        "HTTP/1.1 200 OK\r\nContent-Length: 14\r\n\r\nThis is a test";

    [Fact]
    public void ArrayInputDeliversArraySegmentToOverride()
    {
        var handler = new ArraySegmentCapturingDelegate();
        var parser = new HttpCombinedParser(handler);
        var data = Bytes(BodyMessage);

        Assert.Equal(data.Length, parser.Execute(data));
        Assert.Equal("This is a test", Encoding.UTF8.GetString(handler.Captured.ToArray()));
    }

    [Fact]
    public void SpanInputDeliversSpanToOverride()
    {
        var handler = new SpanCapturingDelegate();
        var parser = new HttpCombinedParser(handler);
        ReadOnlySpan<byte> data = Bytes(BodyMessage);

        Assert.Equal(data.Length, parser.Execute(data));
        Assert.Equal("This is a test", Encoding.UTF8.GetString(handler.Captured.ToArray()));
    }

    [Fact]
    public void LegacyInterfaceDelegateWorksWithSpanInput()
    {
        var handler = new LegacyInterfaceDelegate();
        var parser = new HttpCombinedParser(handler);
        ReadOnlySpan<byte> data = Bytes(BodyMessage);

        Assert.Equal(data.Length, parser.Execute(data));
        Assert.Equal("This is a test", Encoding.UTF8.GetString(handler.Captured.ToArray()));
    }

    [Fact]
    public void ChunkedTakesPrecedenceOverContentLength()
    {
        // RFC 9112 6.3: when both are present, Transfer-Encoding wins.
        var (handler, _, parsed, length) = Parse(
            "HTTP/1.1 200 OK\r\n" +
            "Content-Length: 4\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            "1C\r\nlonger than content length!!\r\n" +
            "0\r\n\r\n");

        Assert.Equal(length, parsed);
        Assert.Equal("longer than content length!!", BodyOf(handler));
    }
}
