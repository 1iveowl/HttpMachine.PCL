# HttpMachine

[![NuGet](https://img.shields.io/nuget/v/HttpMachine.PCL?logo=nuget&label=HttpMachine.PCL)](https://www.nuget.org/packages/HttpMachine.PCL)
[![NuGet](https://img.shields.io/nuget/v/IHttpMachine?logo=nuget&label=IHttpMachine)](https://www.nuget.org/packages/IHttpMachine)
[![Downloads](https://img.shields.io/nuget/dt/HttpMachine.PCL?logo=nuget&color=blue)](https://www.nuget.org/packages/HttpMachine.PCL)
[![Build and Test](https://github.com/1iveowl/HttpMachine.PCL/actions/workflows/test.yml/badge.svg)](https://github.com/1iveowl/HttpMachine.PCL/actions/workflows/test.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENCE.md)

[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%2010.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0%20%7C%202.1-5C2D91?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/standard/net-standard)

A combined C# HTTP request/response parser, implemented as a state machine built with
[Adrian Thurston](https://www.colm.net/)'s excellent state machine compiler,
[Ragel](https://www.colm.net/open-source/ragel/).

*Please star this project if you find it useful. Thank you.*

## Background

This project began in May 2016 as a fork of the great work done by Benjamin van der Veen,
[the original HttpMachine](https://github.com/bvanderveen/httpmachine), which was no longer
being maintained. It has been developed independently ever since, and by now differs
substantially from its origin: a combined request/response parser, chunked transfer encoding,
zero-length header support, a span-based parsing API, and continuous modernization of the
target frameworks — see the [version history](#version-history) below.

The original HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com)
and is licensed under the MIT License, as is this fork. See [LICENCE.md](LICENCE.md).

## Features

- HTTP/1.1 and 1.0
- Parses requests and responses with one combined parser — filter on `MessageType` to see which one was parsed
- Supports pipelined messages
- Tells your server if it should keep the connection alive (`ShouldKeepAlive`)
- Extracts the length of the entity body (`Content-Length`)
- Supports chunked Transfer-Encoding
- Handles zero-length headers (for example `EXT:` as used in UPnP)
- Header values are collected in an `IEnumerable<string>` per header name (repeated headers accumulate)

## Usage

```cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using HttpMachine;

var data = Encoding.UTF8.GetBytes(
    "HTTP/1.1 200 OK\r\n" +
    "Content-Type: text/plain\r\n" +
    "Content-Length: 14\r\n" +
    "\r\n" +
    "This is a test");

using var handler = new HttpParserDelegate();
using var parser = new HttpCombinedParser(handler);

if (parser.Execute(data) != data.Length)
{
    Console.WriteLine("Parse failed.");
    return;
}

var result = handler.HttpRequestResponse;

Console.WriteLine($"Type: {result.MessageType}");     // Response
Console.WriteLine($"Status: {result.StatusCode}");    // 200
Console.WriteLine($"Keep-alive: {result.ShouldKeepAlive}");

foreach (var header in result.Headers)
{
    Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
}

result.Body.Position = 0;
Console.WriteLine($"Body: {new StreamReader(result.Body).ReadToEnd()}");
```

See [the console test project](src/tests/Console.NETCore.Test/Program.cs) for a complete example including chunked transfer encoding, and [the test suite](src/tests/HttpMachine.Tests) for more usage patterns.

### Things worth knowing

- **Return value of `Execute`:** the number of bytes consumed. If it is less than the length you passed in, the parser hit a parse error at that position.
- **Feeding data incrementally:** call `Execute` repeatedly as data arrives; messages may be split across buffers at any point.
- **Span support:** `Execute(ReadOnlySpan<byte>)` parses without requiring an array — useful with pooled buffers or `PipeReader` slices. Body callbacks mirror the input: array-based `Execute` overloads deliver `OnBody(ArraySegment<byte>)` over your buffer (as in 5.x); the span overload delivers `OnBody(ReadOnlySpan<byte>)` to delegates implementing `IHttpParserSpanDelegate` (which `HttpParserDelegate` does), or a pooled copy to plain `IHttpParserDelegate` implementors. Either way the buffer is only valid during the callback — copy it if you keep it.
- **End of stream:** when a message has no `Content-Length` and is not chunked, the body runs until the connection closes. Signal this to the parser by calling `Execute` with an empty buffer.
- **Header keys:** stored uppercased; the `Headers` dictionary uses a case-insensitive comparer, so `headers["content-type"]` works too.
- **Delegate results:** `handler.HttpRequestResponse` is the snapshot of the most recently *completed* message. It is `null` until the first message has been fully parsed. When parsing pipelined messages, read it from `OnMessageEnd` (override it) if you need every message.
- **Customizing:** subclass `HttpParserDelegate` and override the `On...` methods you care about, or implement `IHttpParserCombinedDelegate` from scratch.
- The HTTP method accepts these additional four characters: `$ - , .`
- When both `Content-Length` and `Transfer-Encoding: chunked` are present, chunked wins (RFC 9112 6.3).

## Working on the parser (Ragel)

`src/main/HttpMachine.netstandard/HttpCombinedParser.cs` is **generated code** — do not edit it by hand. The sources are:

- [HttpParser2-chunked.cs.rl](src/main/HttpMachine.netstandard/rl/HttpParser2-chunked.cs.rl) — C# host file with the parser actions
- [http-chunked.rl](src/main/HttpMachine.netstandard/rl/http-chunked.rl) — the HTTP grammar
- [uri.rl](src/main/HttpMachine.netstandard/rl/uri.rl) — the URI grammar

To regenerate, install Ragel 6.x (`apt install ragel` on Debian/Ubuntu, available via Homebrew and Cygwin elsewhere) and run [generate.sh](src/main/HttpMachine.netstandard/rl/generate.sh) from the `rl` directory. The script also widens a few generated `sbyte[]` tables to `byte[]` — Ragel 6.x picks the element type by table length, not value range, and some tables contain values above 127.

Run the tests after regenerating:

```sh
cd src/tests/HttpMachine.Tests
dotnet test
```

## Version history

### 6.0
- **Breaking:** removed the unused `ParserStatus` enum.
- Added `Execute(ReadOnlySpan<byte>)` and the optional `IHttpParserSpanDelegate` interface for zero-copy body delivery; `IHttpParserDelegate` is unchanged, so existing delegates keep working.
- Targets .NET 10 instead of .NET 9 (C# 14); .NET Standard 2.0/2.1, .NET 6 and .NET 8 unchanged (`System.Memory` dependency added for .NET Standard 2.0).
- Performance: `Execute(MemoryStream)` no longer copies the stream when its buffer is exposable; chunk sizes and status codes are parsed without intermediate string allocations.
- Fixed `ShouldKeepAlive` for HTTP/1.0 messages (`Connection: keep-alive`/`close` were inverted).
- Fixed bodies delimited by connection close (no `Content-Length`, not chunked); signal EOF with an empty `Execute` call.
- Fixed repeated headers separated by other headers (previously failed the whole parse).
- The same `HttpParserDelegate` now works for pipelined/keep-alive message sequences.
- `Transfer-Encoding: chunked` now takes precedence over `Content-Length` per RFC 9112.
- Header dictionary lookups are now case-insensitive.
- Added an xunit test suite and CI; repository cleanup (removed bundled `ragel.exe` and stale build scripts).

### 4.0+
- .NET Standard 2.0 is now the required minimum version.
- Refactoring and simplification of use has caused some namespaces to change.
- It is no longer necessary to implement the interface `IHttpCombinedParser`; instead simply new up `HttpParserDelegate`. Methods can be overridden if needed.
- Header values are collected in an `IEnumerable<string>`.

### 3.1.0
- The parser handler must implement `IHttpCombinedParser` instead of using `HttpCombinedParser`.

### 1.1.1
- The parser was combined into one request/response parser. Filter on `MessageType` to see what type was passed.

## Why HTTP/1.x only?

HttpMachine parses HTTP/1.0 and HTTP/1.1, and there are no plans to support HTTP/2 or HTTP/3.

What makes this library useful is that it parses HTTP messages straight off a byte stream, with no
connection, socket or transport attached — which is what you need for HTTP-shaped traffic that
doesn't arrive over a normal HTTP connection: SSDP/UPnP discovery over UDP multicast, WebSocket
upgrade handshakes, embedded devices, protocol testing and the like.

HTTP/2 doesn't fit that model. It is a binary, multiplexed protocol that requires an ordered,
reliable connection (in practice TLS with ALPN), so the scenarios above can't use it in the first
place. It also can't be handled by a parser alone: an endpoint has to acknowledge SETTINGS and send
WINDOW_UPDATE frames or the peer stalls, so anything supporting it needs a write path and becomes a
connection implementation rather than a parser. Beyond that, HTTP/2 shares no grammar with HTTP/1.x —
the Ragel state machine buys you nothing, header compression (HPACK) needs connection-wide state, and
stream multiplexing would mean a stream ID on every delegate callback. That is a separate library,
not a feature. HTTP/3 adds QUIC on top of all of it.

If you need HTTP/2 or HTTP/3 over a regular connection, .NET already has excellent support:
[`System.Net.Http.HttpClient`](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) for
clients and [Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel) for servers.
