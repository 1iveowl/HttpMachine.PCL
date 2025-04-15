# HttpMachine for .NET Standard 2.0

[![NuGet Badge](https://img.shields.io/nuget/v/HttpMachine.PCL)](https://www.nuget.org/packages/HttpMachine.PCL)

[![.NET Standard](http://img.shields.io/badge/.NET_Standard-v2.0-red.svg)](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library)[![.NET Standard](http://img.shields.io/badge/.NET_Standard-v2.1-red.svg)](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library)
[![.NET 6](http://img.shields.io/badge/.NET-v6.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/6.0)[![.NET 8](http://img.shields.io/badge/.NET-v8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)[![.NET 9](http://img.shields.io/badge/.NET-v9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

![CI/CD](https://github.com/1iveowl/HttpMachine.PCL/actions/workflows/build.yml/badge.svg)

*Please star this project if you find it useful. Thank you.*

## Background

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

HttpMachine.PCL is a combined C# HTTP request/reponse parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C#, Java, Ruby, C++ and more.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

I've forked this project as Benjamin is no longer maintaining the work.

## Breaking changes for version 4.0+
.NET Standard 2.0+ is now required minimun version.

Refactoring and simplification of use, have caused some namespaces to change. See example below for guidance for usage. 

For instance, while still possible, it's no longer necessary to implement the interface `IHttpCombinedParser`, instead simply new up the `HttpParserDelegate`. Methods can be overrided if needed.

## Breaking changes from version 3.0.1 to 3.1.0
From version 3.1.0 and forward the ParserHandler must implement `IHttpCombinedParser` instead of using `HttpCombinedParser` (see example code below).

## Original features
- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 
- Support for parsing responses and request in one combined parser.
 
## Additional features
Additions to this version compared with the original HttpMachine by [Benjamin van der Veen](http://bvanderveen.com)
- .NET Standard 1.0 support (after version 4.0 only .NET Standard 2.0 is supported).
- Support for Chunked Transfer-Encoding
- Can be used on Windows 8+, .NET 4.5+, Xamarin.Android, Xamarin.iOS and ASP.NET Core 1.0+
- The Http Method now accepts these additional four characters: $ - , .
- From library ver 1.1.1 the parser has been combined into one Request/Reponse parser. Filter on `MessageType` to see what type was passed.
- Can now manage Zero Lenght Headers - for example EXT: as used in UPnP.
- From version 4.0 and onwards header values are collected in an `IEnumerable<string>`;

### Use like this: 

```cs
class Program
    {
        static void Main(string[] args)
        {
            byte[] bArray;

            using (var handler = new HttpParserDelegate())
            using (var parser = new HttpCombinedParser(handler))
            {
                bArray = TestReponse();
                Console.WriteLine(parser.Execute(bArray) == bArray.Length
                    ? $"Reponse test succeed. Type identified is; {handler.HttpRequestResponse.MessageType} \r\n" +
                        $"Headers: \r\n" +
                        $"{string.Join("\r\n", handler.HttpRequestResponse.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)} "))}"
                    : $"Response test failed");

                handler.HttpRequestResponse.Body.Position = 0;
                var reader = new StreamReader(handler.HttpRequestResponse.Body);
                var body = reader.ReadToEnd();
            }

            Console.WriteLine("\r\n\r\n");

            using (var handler = new HttpParserDelegate())
            using (var parser = new HttpCombinedParser(handler))
            {
                bArray = TestChunkedResponse();
                Console.WriteLine(parser.Execute(bArray) == bArray.Length
                    ? $"Chunked Response test succeed. Type identified is; {handler.HttpRequestResponse.MessageType}." +
                    $"Headers: \r\n" +
                    $"{string.Join("\r\n", handler.HttpRequestResponse.Headers.Select(h => $"{h.Key}: {string.Join(",", h.Value)} "))}"
                    : $"Chunked Response test failed");

                handler.HttpRequestResponse.Body.Position = 0;
                var reader = new StreamReader(handler.HttpRequestResponse.Body);
                var body = reader.ReadToEnd();
            }
        }
        
        // Test Response
        private static string TestReponse()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("HTTP/1.1 200 OK\r\n");
            stringBuilder.Append("CACHE-CONTROL: max-age = 10\r\n");
            stringBuilder.Append("ST: upnp:rootdevice\r\n");
            stringBuilder.Append("ST: upnp:rootdevice\r\n");
            stringBuilder.Append("USN: uuid:73796E6F-6473-6D00-0000-0011322fe5f0::upnp:rootdevice\r\n");
            stringBuilder.Append("EXT:\r\n");
            stringBuilder.Append("SERVER: Synology/DSM/200.200.200.200\r\n");
            stringBuilder.Append("LOCATION: http://200.200.200.101:5000/ssdp/desc-DSM-eth1.xml\r\n");
            stringBuilder.Append("OPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n");
            stringBuilder.Append("01-NLS: 1\r\n");
            stringBuilder.Append("BOOTID.UPNP.ORG: 1\r\n");
            stringBuilder.Append("CONFIGID.UPNP.ORG: 1337\r\n");
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }
        
        // Test request
        private static string TestRequest()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("NOTIFY * HTTP/1.1\r\n");
            stringBuilder.Append("HOST: 239.255.255.250:1900\r\n");
            stringBuilder.Append("CACHE-CONTROL: max-age = 10\r\n");
            stringBuilder.Append("LOCATION: http://www.bing.com\r\n");
            stringBuilder.Append("NT: \"upnp:rootdevice\"\r\n");
            stringBuilder.Append("NTS: ssdp:alive\r\n");
            stringBuilder.Append("EXT:\r\n");
            stringBuilder.Append("SERVER: Synology/DSM/200.200.200.100 UPnP/2.0 Test/1.0\r\n");
            stringBuilder.Append("BOOTID.UPNP.ORG: 1\r\n");
            stringBuilder.Append("CONFIGID.UPNP.ORG: 121212\r\n");
            stringBuilder.Append("\r\n");
            return stringBuilder.ToString();
        }
    }
```

