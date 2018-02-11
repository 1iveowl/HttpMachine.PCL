# HttpMachine for .NET Standard 1.0
[![NuGet](https://img.shields.io/badge/nuget-3.1.0_(.Net_Standard_1.0)-brightgreen.svg)](https://www.nuget.org/packages/HttpMachine.PCL/)
[![NuGet](https://img.shields.io/badge/nuget-1.1.10_(Profile_111)-yellow.svg)](https://www.nuget.org/packages/HttpMachine.PCL/1.1.10)

*Please star this project if you find it useful. Thank you.*

## Background

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

HttpMachine.PCL is a combined C# HTTP request/reponse parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C#, Java, Ruby, C++ and more.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

I've forked this project as Benjamin is no longer is maintaining the work.


## IMPORTANT: Breaking changes from version 3.0.1 to 3.1.0
From version 3.1.0 and forward the ParserHandler must use `IHttpCombinedParser` instead of `HttpCombinedParser` (see example code below).

Also you must include `using IHttpMachine` when implementing the parser handler.

## Original features

- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 
- Support for parsing responses and request in one combined parser.
 
## Additional features
Additions to this version compared with the original HttpMachine by [Benjamin van der Veen](http://bvanderveen.com)
- .Net standard 1.0 support
- Support for Chunked Transfer-Encoding
- Can be used on Windows 8+, .NET 4.5+, Xamarin.Android, Xamarin.iOS and ASP.NET Core 1.0+
- The Http Method now accepts these additional four characters: $ - , .
- From Nuget ver 1.1.1 the parser has been combined into one Request/Reponse parser. Filter on `MessageType` to see what type was passed.
- Can now manage Zero Lenght Headers - for example EXT: as used in UPnP 

## How to use
### Implement a parser handler:
```cs
internal class ParserHandler : IHttpParserCombinedDelegate
    {
        public bool HasError { get; internal set; } = false;
        public MessageType MessageType { get; private set; }
        public void OnResponseType(IHttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Response;
        }
        public void OnRequestType(IHttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Request;
        }
        public void OnMessageBegin(IHttpCombinedParser combinedParser) {}
        public void OnHeaderName(IHttpCombinedParser combinedParser, string name) {}
        public void OnHeaderValue(IHttpCombinedParser combinedParser, string value) {}
        public void OnHeadersEnd(IHttpCombinedParser combinedParser) {}
        public void OnMethod(IHttpCombinedParser combinedParser, string method) {}
        public void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri) {}
        public void OnPath(IHttpCombinedParser combinedParser, string path) {}
        public void OnFragment(IHttpCombinedParser combinedParser, string fragment) {}
        public void OnQueryString(IHttpCombinedParser combinedParser, string queryString) {}
        public void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason) {}
        public void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data) {}
        public void OnMessageEnd(IHttpCombinedParser combinedParser) {}
        public void OnParserError()
        {
            HasError = true;
        }
    }
```

### Use the parser  like this: 

```cs
class Program
    {
        static void Main(string[] args)
        {
            var handler = new ParserHandler();
            var parser = new HttpCombinedParser(handler);
            
            // Test response data
            var bArray = Encoding.UTF8.GetBytes(TestReponse());
            
            System.Console.WriteLine(parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length
                ? $"Reponse test succeed. Type identified is; {handler.MessageType}"
                : $"Response test failed");
            
            // Test request data    
            bArray = Encoding.UTF8.GetBytes(TestRequest());
            
            System.Console.WriteLine(parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length
                ? $"Request test succeed. Type identified is; {handler.MessageType}"
                : $"Request test failed");
                
            System.Console.ReadKey();
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

