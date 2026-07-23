# HttpMachine

[![NuGet Badge](https://img.shields.io/nuget/v/HttpMachine.PCL)](https://www.nuget.org/packages/HttpMachine.PCL)

Targets: .NET Standard 2.0 & 2.1, .NET 6, .NET 8 and .NET 9.

*Please star this project if you find it useful. Thank you.*

## Background

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

HttpMachine.PCL is a combined C# HTTP request/response parser. It implements a state machine built with [Adrian Thurston](https://www.colm.net/)'s excellent state machine compiler, [Ragel](https://www.colm.net/open-source/ragel/).

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the MIT License. See [LICENCE.md](LICENCE.md).

I've forked this project as Benjamin is no longer maintaining the work.

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
