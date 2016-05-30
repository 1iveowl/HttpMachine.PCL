# HttpMachine.PCL

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

HttpMachine.PCL is a combined C# HTTP request/reponse parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C#, Java, Ruby, C++ and more.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

I've forked this project as Benjamin no longer is maintaining the work.

## Original Features

- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 
- Support for parsing responses and request in one combined parser.
 
## Updates Added to HttpMachine.PCL
- Updated the Nuget to the most recent code base provided by the original project.
- The HttpMachine.PCL a PCL that can be used on Windows 8+, .NET 4.5+, Xamarin.Android, Xamarin.iOS and ASP.NET Core 1.0+
- From Nuget ver. 1.0 the Http Method now accepts these additional four characters: "$ - , .".
- From Nuget ver 1.1.1 the parser has been combined into one Request/Reponse parser. Filter on `MessageType` to see what type was passed.
- Can now mange Zero Lenght Headers - for example EXT: as used in UPnP 

## How to use
### Implement a parser handler:
```cs
internal class ParserHandler : IHttpParserCombinedDelegate
    {
        public bool HasError { get; internal set; } = false;
        public MessageType MessageType { get; private set; }
        public void OnResponseType(HttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Response;
        }
        public void OnRequestType(HttpCombinedParser combinedParser)
        {
            MessageType = MessageType.Request;
        }
        public void OnMessageBegin(HttpCombinedParser combinedParser) {}
        public void OnHeaderName(HttpCombinedParser combinedParser, string name) {}
        public void OnHeaderValue(HttpCombinedParser combinedParser, string value) {}
        public void OnHeadersEnd(HttpCombinedParser combinedParser) {}
        public void OnMethod(HttpCombinedParser combinedParser, string method) {}
        public void OnRequestUri(HttpCombinedParser combinedParser, string requestUri) {}
        public void OnPath(HttpCombinedParser combinedParser, string path) {}
        public void OnFragment(HttpCombinedParser combinedParser, string fragment) {}
        public void OnQueryString(HttpCombinedParser combinedParser, string queryString) {}
        public void OnResponseCode(HttpCombinedParser combinedParser, int statusCode, string statusReason) {}
        public void OnBody(HttpCombinedParser combinedParser, ArraySegment<byte> data) {}
        public void OnMessageEnd(HttpCombinedParser combinedParser) {}
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
            stringBuilder.Append("SERVER: Synology/DSM/192.168.0.33\r\n");
            stringBuilder.Append("LOCATION: http://192.168.0.33:5000/ssdp/desc-DSM-eth1.xml\r\n");
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

