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

## When is this library useful?

Most .NET code should reach for
[`HttpClient`](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) or
[Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel) - they manage
connections, TLS and newer protocol versions for you. HttpMachine solves a different problem: it
parses HTTP messages from **raw bytes**, with no socket, connection or transport attached. You hand
it a buffer, it hands you back the parsed message.

That makes it a good fit when:

- **The HTTP message doesn't arrive over an HTTP connection.** SSDP/UPnP discovery is the classic
  case - `NOTIFY` and `M-SEARCH` messages multicast over UDP, which no HTTP client will parse for you.
- **You need the handshake, not the protocol.** Reading a WebSocket upgrade request or response.
- **You are implementing the endpoint yourself** - a custom or embedded server, a proxy or gateway,
  or a device speaking HTTP over an unusual transport.
- **You are inspecting or testing traffic** - sniffers, protocol test harnesses, fuzzing, or
  replaying captured bytes.
- **Data arrives in fragments.** Messages may be split across buffers at any byte, and
  `Execute(ReadOnlySpan<byte>)` reads pooled or sliced buffers without copying.

Targeting .NET Standard 2.0 also means it runs where modern HTTP stacks don't - older runtimes,
Xamarin, Unity and constrained devices.

If you need HTTP/2 or HTTP/3, see [Why HTTP/1.x only?](#why-http1x-only) at the end.

## Built with HttpMachine

These libraries build on HttpMachine and are good examples of the "wrap the parser with a
transport" pattern described above:

- **[SimpleHttpListener.Rx](https://github.com/1iveowl/SimpleHttpListener.Rx)** - a lightweight,
  reactive (Rx) HTTP and HTTPU/SSDP listener. It feeds incoming bytes to HttpMachine and surfaces
  parsed requests and responses as an observable stream.
- **[SSDP.UPnP.PCL](https://github.com/1iveowl/SSDP.UPnP.PCL)** - an SSDP implementation for UPnP 2.0
  (discovery `NOTIFY` / `M-SEARCH` over UDP multicast), exactly the datagram-style HTTP this parser
  is built to handle.
- **[UPnP.rx](https://github.com/1iveowl/UPnP.rx)** - a modern Rx UPnP client for .NET 10:
  discover, describe, control.

## Background

This project began in May 2016 as a fork of the great work done by Benjamin van der Veen,
[the original HttpMachine](https://github.com/bvanderveen/httpmachine), which was no longer
being maintained. It has been developed independently ever since, and by now differs
substantially from its origin: a combined request/response parser, chunked transfer encoding,
zero-length header support, a span-based parsing API, and continuous modernization of the
target frameworks - see the [version history](#version-history) below.

The original HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com)
and is licensed under the MIT License, as is this fork. See [LICENCE.md](LICENCE.md).

## Features

- HTTP/1.1 and 1.0
- Parses requests and responses with one combined parser - filter on `MessageType` to see which one was parsed
- Supports pipelined messages
- Tells your server if it should keep the connection alive (`ShouldKeepAlive`)
- Extracts the length of the entity body (`Content-Length`)
- Supports chunked Transfer-Encoding
- Handles zero-length headers (for example `EXT:` as used in UPnP)
- Header values are collected in an `IEnumerable<string>` per header name (repeated headers accumulate)
- Recognises 73 known header names without allocating, using a generated matcher (see [`HeaderNameMode`](#header-names-and-allocation))
- No reflection or dynamic code: trim-safe and Native AOT compatible

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
- **Span support:** `Execute(ReadOnlySpan<byte>)` parses without requiring an array - useful with pooled buffers or `PipeReader` slices. Body callbacks mirror the input: array-based `Execute` overloads deliver `OnBody(ArraySegment<byte>)` over your buffer (as in 5.x); the span overload delivers `OnBody(ReadOnlySpan<byte>)` to delegates implementing `IHttpParserSpanDelegate` (which `HttpParserDelegate` does), or a pooled copy to plain `IHttpParserDelegate` implementors. Either way the buffer is only valid during the callback - copy it if you keep it.
- **End of stream:** when a message has no `Content-Length` and is not chunked, the body runs until the connection closes. Signal this to the parser by calling `Execute` with an empty buffer. See [Message framing](#message-framing-and-the-one-deliberate-deviation) for how bodyless responses are handled and the `UnframedResponseMode` option.
- **Header keys:** stored uppercased; the `Headers` dictionary uses a case-insensitive comparer, so `headers["content-type"]` works too.
- **Header names and allocation:** recognised header names are reported to `OnHeaderName` from a cached instance rather than a freshly allocated string. See [Header names and allocation](#header-names-and-allocation).
- **Delegate results:** `handler.HttpRequestResponse` is the snapshot of the most recently *completed* message. It is `null` until the first message has been fully parsed. When parsing pipelined messages, read it from `OnMessageEnd` (override it) if you need every message.
- **Customizing:** subclass `HttpParserDelegate` and override the `On...` methods you care about, or implement `IHttpParserCombinedDelegate` from scratch.
- The HTTP method accepts these additional four characters: `$ - , .`
- When both `Content-Length` and `Transfer-Encoding: chunked` are present, chunked wins (RFC 9112 6.3).

## Header names and allocation

The parser recognises 73 common header names - the HTTP core set, the WebSocket handshake headers
and the SSDP/UPnP ones - by matching the raw UTF-8 bytes against generated code, before any string
exists. When a name is recognised it is reported to `OnHeaderName` from a cached instance instead of
a freshly allocated string, which removes most of the parser's per-message allocations.

Which spelling you are handed is controlled by `HeaderNameMode`. HTTP field names are
case-insensitive ([RFC 9110 5.1](https://www.rfc-editor.org/rfc/rfc9110#section-5.1)), so the two
modes differ only in casing, never in which headers are recognised or how the message is framed:

```csharp
// Default: names arrive exactly as the sender wrote them, as in every earlier version.
using var parser = new HttpCombinedParser(handler);

// Opt in: recognised names arrive in one canonical spelling whatever the sender used.
using var parser = new HttpCombinedParser(handler, HeaderNameMode.Canonical);

// Both options together.
using var parser = new HttpCombinedParser(handler, UnframedResponseMode.CloseDelimited, HeaderNameMode.Canonical);
```

- **`AsReceived`** (default) - `CONTENT-LENGTH:` is reported as `"CONTENT-LENGTH"`. A recognised
  name whose bytes already match the canonical spelling still avoids the allocation, because the
  cached instance and the string it would have built are identical. Only a name sent in some other
  casing has to be materialised.
- **`Canonical`** - both `CONTENT-LENGTH:` and `content-length:` are reported as `"Content-Length"`,
  and no recognised name allocates whatever casing arrives. Worth selecting for SSDP/UPnP traffic,
  where devices commonly send `LOCATION:` and `CACHE-CONTROL:` in upper case.

`HttpRequestResponse.Headers` is unaffected either way: keys are uppercased into a case-insensitive
dictionary, so the mode is only visible to code reading the raw name from `OnHeaderName`.

Unrecognised headers are unchanged in both modes - reported with the sender's casing, exactly as
before.

Measured against 6.1.0 on a realistic request, response and SSDP `NOTIFY`: **21-37% fewer bytes
allocated** by the parser itself, or **8-12%** once the allocations made by `HttpParserDelegate` are
included. Parse time is unchanged. The benchmarks are in
[src/benchmarks/HeaderMatcher.Benchmarks](src/benchmarks/HeaderMatcher.Benchmarks).

## Message framing (and the one deliberate deviation)

This is a **pure byte parser with no connection model** - it sees bytes, not sockets. That single
fact explains most of its framing behaviour. Following
[RFC 9112 6.3](https://www.rfc-editor.org/rfc/rfc9112#section-6.3), the length of a message body is
determined, in order, by:

1. `Transfer-Encoding: chunked` - the body is delimited by the chunk framing.
2. `Content-Length` - exactly that many bytes.
3. Neither header present:
   - **A request has no body** (RFC 9112 6.3 rule 6).
   - **A response is delimited by connection close** - the body runs until the connection ends.

Rule 3 for responses is where "no connection model" bites: the parser can't observe a socket
closing. So closing is expressed *in bytes* - **you signal end of stream by calling `Execute` with
an empty buffer.** That is the parser's definition of "the connection closed", and it's what makes
close-delimited bodies work without the parser knowing anything about transports.

### Why bodyless responses have a mode

There is one case where strict HTTP and this library's main use case genuinely disagree: a
**response with no `Content-Length`, no `Transfer-Encoding`, and keep-alive** (e.g. a default
HTTP/1.1 response). Strict RFC 9112 says such a response is close-delimited. But the traffic this
library is most used for - **SSDP / UPnP discovery** (see the examples above) - is HTTPU: HTTP-shaped
messages carried one-per-datagram over UDP, with no body and no connection to close. There, the
message is simply *complete at the blank line*. The same bytes, two correct-but-opposite meanings.

Because no single behaviour serves both, the framing of that specific case is a choice:

| `UnframedResponseMode` | A keep-alive response with no framing headers… | Good for |
| --- | --- | --- |
| `CompleteAtHeaders` *(default)* | is complete at the end of its headers, with no body | HTTPU/SSDP, UPnP, datagram-style messages |
| `CloseDelimited` | is read until you signal EOF with an empty `Execute` (RFC 9112 6.3) | HTTP/1.x responses over a stream |

The default preserves the datagram-friendly behaviour the library has always had. Opt into strict
behaviour when you parse streamed responses that may omit `Content-Length`:

```cs
using var parser = new HttpCombinedParser(handler, UnframedResponseMode.CloseDelimited);
parser.Execute(buffer);         // consumes the response headers and any body bytes so far
parser.Execute(Array.Empty<byte>()); // signal connection close -> completes the message
```

Both modes are identical when the response *is* framed (Content-Length or chunked) or explicitly
carries `Connection: close`; the mode only affects the ambiguous keep-alive-without-framing case.

## Working on the parser (Ragel)

`src/main/HttpMachine.netstandard/HttpCombinedParser.cs` is **generated code** - do not edit it by hand. The sources are:

- [HttpParser2-chunked.cs.rl](src/main/HttpMachine.netstandard/rl/HttpParser2-chunked.cs.rl) - C# host file with the parser actions
- [http-chunked.rl](src/main/HttpMachine.netstandard/rl/http-chunked.rl) - the HTTP grammar
- [uri.rl](src/main/HttpMachine.netstandard/rl/uri.rl) - the URI grammar

To regenerate, install Ragel 6.x (`apt install ragel` on Debian/Ubuntu, available via Homebrew and Cygwin elsewhere) and run [generate.sh](src/main/HttpMachine.netstandard/rl/generate.sh) from the `rl` directory. The script also widens a few generated `sbyte[]` tables to `byte[]` - Ragel 6.x picks the element type by table length, not value range, and some tables contain values above 127.

Run the tests after regenerating:

```sh
cd src/tests/HttpMachine.Tests
dotnet test
```

## Adding a parser test (fixtures)

Most parser tests are **fixture files**: raw HTTP bytes in
[src/tests/HttpMachine.Tests/Fixtures](src/tests/HttpMachine.Tests/Fixtures). Each one becomes a test
case that feeds the bytes to the parser and compares the full sequence of delegate callbacks against
an approved snapshot.

That sequence is the parser's real contract - the order of `OnHeaderName`, `OnHeaderValue`,
`OnHeadersEnd` and `OnBody` is what anyone implementing `IHttpParserDelegate` depends on - and a
snapshot pins all of it, rather than the two or three properties a hand-written assertion usually
checks.

To add a case:

1. Drop the raw bytes into `Fixtures/Requests/` or `Fixtures/Responses/`, named `something.http`.
2. Run the tests. The new case fails with a `.received.txt` next to the fixture.
3. Read it. If it is what the parser should do, rename it to `.verified.txt` and commit both files.

Nothing else is edited - no test list, no registration.

Optional modifiers go between the name and the extension, and an unrecognised one is an error rather
than being silently ignored:

| Modifier | Effect |
|---|---|
| `.invalid` | The parser is expected to reject the input, by stopping short or raising `OnParserError` |
| `.close-delimited` | Parse with `UnframedResponseMode.CloseDelimited` |
| `.canonical` | Parse with `HeaderNameMode.Canonical` |
| `.eof` | Signal end of stream with a final empty `Execute` |

So `Responses/unframed-body.close-delimited.eof.http` reads as what it is.

Alongside the snapshot, two assertions record *intent*: the category folder must match the message
type the parser reported, and `.invalid` fixtures must actually be rejected. The snapshot says what
happened; these say what was meant, and they survive a snapshot being approved carelessly.

**Fixtures are byte-exact.** The bytes on disk are fed to the parser unmodified, and some fixtures
exist to pin down line-ending handling, so nothing may normalise them:

- the root [.gitattributes](.gitattributes) excludes them from git's end-of-line conversion
- [Fixtures/.editorconfig](src/tests/HttpMachine.Tests/Fixtures/.editorconfig) stops editors
  appending a final newline or trimming whitespace
- `FixtureByteExactnessTests` fails loudly if either guard ever stops working

Create fixtures with a tool that writes exact bytes rather than by typing into an editor:

```sh
printf 'HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nhi' > Fixtures/Responses/my-case.http
```

Tests about **how** the parser is fed - buffers split mid-message, span versus array versus
`MemoryStream`, pipelining, when end of stream is signalled - stay hand-written in
[HttpCombinedParserTests.cs](src/tests/HttpMachine.Tests/HttpCombinedParserTests.cs). Fixtures answer
"what does this message parse to"; those answer "how does the parser behave when fed unusually".

## Working on the known-header matcher (source generator)

`src/main/HttpMachine.netstandard/Generated/KnownHeaders.g.cs` is **generated code** - do not edit it
by hand. It maps a UTF-8 header name to a known-header enum value and a cached canonical string by
switching on the name's byte length, then on its case-folded first byte, then comparing 8 bytes at a
time. The parser uses it to recognise `Content-Length`, `Transfer-Encoding`, `Connection` and
`Upgrade` - recognition that used to live in the Ragel grammar - and to avoid allocating a string
for every recognised header name.

The source is a single declarative file:

- [KnownHeaders.txt](src/main/HttpMachine.netstandard/headers/KnownHeaders.txt) - one header per
  line, in its canonical casing, with an optional `Name = EnumMember` override

To regenerate, run [generate-known-headers.sh](src/tools/generate-known-headers.sh) from the `tools`
directory. It needs only the .NET SDK:

```sh
cd src/tools
./generate-known-headers.sh
cd ../tests/HttpMachine.Tests && dotnet test
```

The generator itself is an **author-time tool, exactly like Ragel**. It runs in this repository's
build, its output is committed, and it is never packed - so HttpMachine and IHttpMachine gain no
analyzer, no dependency, no SDK floor and no target-framework change from it, and neither do their
consumers.

It lives in its own solution so it stays out of the pack and publish path:

```sh
cd src/tools
dotnet test HeaderGen.sln
```

- [HttpMachine.HeaderGen](src/tools/HttpMachine.HeaderGen/) - the incremental source generator
- [HttpMachine.HeaderGen.Emit](src/tools/HttpMachine.HeaderGen.Emit/) - runs the generator as a real
  analyzer and is where the committed file comes from. It builds against every framework HttpMachine
  targets, so it also proves the matcher compiles down to .NET Standard 2.0, and the script checks
  all five produce a byte-identical file
- [HeaderGen.Tests](src/tests/HeaderGen.Tests/) - snapshot tests of the generated code, plus a test
  that the generator's pipeline outputs are cacheable (an incremental generator that is not
  cacheable still works, it just re-runs on every keystroke in the IDE)
- [HeaderGen.Integration.Tests](src/tests/HeaderGen.Integration.Tests/) - compiles and executes the
  real matcher, comparing it against a case-insensitive dictionary over the same header list

CI regenerates the file and fails if the result differs from what is committed, so the checked-in
code cannot drift from `KnownHeaders.txt`.

Benchmarks live in [HeaderMatcher.Benchmarks](src/benchmarks/HeaderMatcher.Benchmarks/):

```sh
cd src/benchmarks/HeaderMatcher.Benchmarks
dotnet run -c Release -- --filter '*'
```

[benchmark-against-head.sh](src/tools/benchmark-against-head.sh) runs the same corpus against a git
worktree of any earlier revision, for before-and-after comparisons.

## Version history

### 6.2.0 (unreleased)
- Known header names are now recognised by generated code rather than by the Ragel grammar, matching
  a UTF-8 name span against 73 known headers without allocating. Recognised header names are
  reported using a cached instance instead of a freshly allocated string, which removes roughly 30%
  of the parser's per-message allocations for typical HTTP traffic (about 12% once the allocations
  made by `HttpParserDelegate` itself are included). Parse time is unchanged to slightly slower -
  see the benchmarks in `src/benchmarks/HeaderMatcher.Benchmarks`.
- Added `HeaderNameMode` and `HttpCombinedParser` constructor overloads taking it. The default,
  `AsReceived`, reports header names exactly as the sender wrote them and is unchanged from 6.1.0.
  `Canonical` reports one canonical spelling for every recognised header regardless of the sender's
  casing, and is allocation-free for recognised names whatever casing arrives - worth selecting for
  SSDP/UPnP traffic, where `LOCATION:` and `CACHE-CONTROL:` are commonly sent in upper case.
- Marked `IsAotCompatible` for .NET 8 and .NET 10, so trimming and Native AOT analysis run as part of
  the build. The parser has no reflection or dynamic code and compiles with no trim or AOT warnings.
  Published under Native AOT the library costs about 172 KB, roughly 12 KB more than 6.1.0 - the
  price of the generated matcher, at about 161 bytes per known header.
- No change to `IHttpParserDelegate`, to the `Headers` dictionary, or to which headers the parser
  recognises for message framing. No new dependency and no target-framework change.

### 6.1.0
- Added `UnframedResponseMode` and a new `HttpCombinedParser(delegate, UnframedResponseMode)`
  constructor overload. `CloseDelimited` opts into strict
  [RFC 9112 6.3](https://www.rfc-editor.org/rfc/rfc9112#section-6.3) framing for a response with no
  `Content-Length` and no `Transfer-Encoding`, reading its body until end of stream (signalled by an
  empty `Execute`). The default, `CompleteAtHeaders`, is unchanged and keeps the HTTPU/SSDP behaviour
  where such a response is complete at the blank line - so existing code is unaffected. See
  [Message framing](#message-framing-and-the-one-deliberate-deviation) for the rationale. Addresses the
  scenario in issue #11.

### 6.0.1
- Fixed: a request with neither `Content-Length` nor `Transfer-Encoding` is now treated as having
  no body, per [RFC 9112 6.3](https://www.rfc-editor.org/rfc/rfc9112#section-6.3) rule 6
  ("If this is a request message and none of the above are true, then the message body length is
  zero"). The close-delimited "read until the connection closes" rule applies to responses only.
  Previously such requests - for example `GET / HTTP/1.0` or any request with `Connection: close` -
  never completed until `Execute` was called with an empty buffer, which could hang a server built
  on the parser, and any following pipelined request was silently consumed as body data.
- Known issue, unchanged in this release: a *response* with keep-alive but no framing headers is
  treated as an immediately complete zero-length message, where
  [RFC 9112 6.3](https://www.rfc-editor.org/rfc/rfc9112#section-6.3) rule 8 says it should be
  close-delimited. Correcting this changes behaviour for existing consumers, so it is deferred out
  of this patch release.

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
connection, socket or transport attached - which is what you need for HTTP-shaped traffic that
doesn't arrive over a normal HTTP connection: SSDP/UPnP discovery over UDP multicast, WebSocket
upgrade handshakes, embedded devices, protocol testing and the like.

HTTP/2 doesn't fit that model. It is a binary, multiplexed protocol that requires an ordered,
reliable connection (in practice TLS with ALPN), so the scenarios above can't use it in the first
place. It also can't be handled by a parser alone: an endpoint has to acknowledge SETTINGS and send
WINDOW_UPDATE frames or the peer stalls, so anything supporting it needs a write path and becomes a
connection implementation rather than a parser. Beyond that, HTTP/2 shares no grammar with HTTP/1.x -
the Ragel state machine buys you nothing, header compression (HPACK) needs connection-wide state, and
stream multiplexing would mean a stream ID on every delegate callback. That is a separate library,
not a feature. HTTP/3 adds QUIC on top of all of it.

If you need HTTP/2 or HTTP/3 over a regular connection, .NET already has excellent support:
[`System.Net.Http.HttpClient`](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient) for
clients and [Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel) for servers.
