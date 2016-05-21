# HttpMachine.PCL

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

I've forked this project as Benjamin no longer is maintaining the work and I needed some updates for a project of mine.

Updates:
- I've updated the Nuget to the most recent code base provided by the original project.
- The HttpMachine.PCL Nuget 0.9.1. is now a PCL that can be used on Windows 8+, .NET 4.5+, Xamarin.Android, Xamarin.iOS and ASP.NET Core 1.0+
- From Nuget ver. 0.9.5 the Http Method now accepts these additional four characters: "$ - , .".


#The Original Readme

HttpMachine is a C# HTTP request parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C, D, Java, Ruby, it wouldn't be hard to port this library to those languages.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

## Features

- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 
- Support for parsing responses.

## Eminently-possible future features

- Support for decoding chunked transfers.
- Support for protocol upgrade.

## Usage

HttpMachine provides HTTP data through callbacks. To receive these callbacks, implement either the `IHttpRequestParserDelegate` or the `IHttpResponseParserDelegate` interface.


	// common interface for both requests and responses
	public interface IHttpParserDelegate
    {
        void OnMessageBegin(HttpParser parser);
        void OnHeaderName(HttpParser parser, string name);
        void OnHeaderValue(HttpParser parser, string value);
        void OnHeadersEnd(HttpParser parser);
        void OnBody(HttpParser parser, ArraySegment<byte> data);
        void OnMessageEnd(HttpParser parser);
    }
    
    public interface IHttpRequestParserDelegate : IHttpParserDelegate
    {
        void OnMethod(HttpParser parser, string method);
        void OnRequestUri(HttpParser parser, string requestUri);
        void OnPath(HttpParser parser, string path);
        void OnFragment(HttpParser parser, string fragment);
        void OnQueryString(HttpParser parser, string queryString);
    }

    public interface IHttpResponseParserDelegate : IHttpParserDelegate
    {
        void OnResponseCode(HttpParser parser, int statusCode, string statusReason); 
    }


Then, create an instance of `HttpParser`. Whenever you read data, execute the parser on the data. The `Execute` method returns the number of bytes successfully parsed. If the returned value is not the same as the length of the buffer you provided, an error occurred while parsing. Make sure you provide a zero-length buffer at the end of the stream, as some callbacks may still be pending.

    var handler = new MyHttpParserDelegate();
    var parser = new HttpParser(handler);
    
    var buffer = new byte[1024 /* or whatever you like */]
    
    int bytesRead;
    
    while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        if (bytesRead != parser.Execute(new ArraySegment<byte>(buffer, 0, bytesRead))
            goto error; /* or whatever you like */
    
    // ensure you get the last callbacks.
    parser.Execute(default(ArraySegment<byte>));
    
The parser has three public properties:

    // HTTP version provided in the request
    public int MajorVersion { get; }
    public int MinorVersion { get; }

    // inspects "Connection" header and HTTP version (if any) to recommend a connection behavior
    public bool ShouldKeepAlive { get; }

These properties are only guaranteed to be accurate in the `OnBody` and `OnMessageEnd` callbacks.

