# HttpMachine.PCL
[![NuGet Badge](https://buildstats.info/nuget/HttpMachine.PCL)](https://www.nuget.org/packages/HttpMachine.PCL)

[![.NET Standard](http://img.shields.io/badge/.NET_Standard-v1.0-green.svg)](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library)

For a PCL Profile111 legacy edition see:

[![NuGet](https://img.shields.io/badge/nuget-1.1.10_(Profile_111)-yellow.svg)](https://www.nuget.org/packages/HttpMachine.PCL/1.1.10)


## History Note 

This is a fork of the great work done by Benjamin van der Veen. [The Original HttpMachine](https://github.com/bvanderveen/httpmachine)

HttpMachine.PCL is a combined C# HTTP request/reponse parser. It implements a state machine with [Adrian Thurston](http://www.complang.org/thurston/)'s excellent state machine compiler, [Ragel](http://www.complang.org/ragel/). Because Ragel supports C#, Java, Ruby, C++ and more.

HttpMachine is Copyright (c) 2011 [Benjamin van der Veen](http://bvanderveen.com). HttpMachine is licensed under the 
MIT License. See LICENSE.txt.

I've forked this project as Benjamin is no longer is maintaining the work.

## Compatibility
- iOS
- Android
- Universal Windows (UWP/Windows 10)
- .NET 4.5+
- Mono/Xamarin Platforms
- .NET Core (macOS, Windows, Linux)
- Windows 8.0+
- Windows Phone 8.0+

## Original Features

- HTTP/1.1 and 1.0
- Supports pipelined requests
- Tells your server if it should keep-alive
- Extracts the length of the entity body 
- Support for parsing responses and request in one combined parser.
 
## Updates Added to HttpMachine.PCL
Changes to this version
- Now supports [Transfer-Encoding: chunked](https://en.wikipedia.org/wiki/Chunked_transfer_encoding "Transfer-Encoding: chunked")
- Now supports [.NET Standard 1.0](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library ".NET Standard 1.0") and can be used in [.NET Core](https://www.microsoft.com/net/core/platform ".NET Core").
- Updated the Nuget to the most recent code base provided by the original project.
- The Http Method now accepts these additional four characters: "$ - , .".
- From Nuget v1.1.1 the parser has been combined into one Request/Reponse parser. Filter on `MessageType` to see what type was passed.
- From Nuget v3.0.1+ this library is .NET Standard 1.0 compliant
- Can now manage Zero Lenght Headers - for example EXT: as used in UPnP 

## How to use
For an example of using HttpMachine see the included [test](https://github.com/1iveowl/HttpMachine.PCL/tree/master/src/tests/HttpMachine.Console.NETCore.Test "test") or see [Simple Http Listener](https://github.com/1iveowl/Simple-Http-Listener-PCL "Simple Http Listener").

The principal is easy. 

1. To use HttpMachine.PCL start by creating a "parser delegate" class that implements the interface `IHttpParserCombinedDelegate`. 

2. This delegate is then passed (i.e. using [Dependency Injection](https://msdn.microsoft.com/en-us/library/ff921152.aspx "Dependency Injection")) into an instance of the HttpCombinedParser class provided by HttpMachine. It looks something like this:

```csharp
using HttpMachine;

class Program
{
	static void Main(string[] args)
	{
		using (var handler = new ParserHandler())
		using (var parser = new HttpCombinedParser(handler))
		{
			// Your code goes here...
		}
	}
}

```

3. The HttpMachine parser expects an [ArraySegment](http://stackoverflow.com/questions/4600024/what-is-the-use-of-arraysegmentt-class "ArraySegment") of bytes: `ArraySegment<byte>`.

4. If you already have an array of bytes (`bArray`) then creating an ArraySegment is easy: `new ArraySegment<byte>(bArray, 0, bArray.Length)`

5. The parser returns the number of bytes parses and a simple way to check for success is see if all bytes have been parsed - i.e.: `parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) == bArray.Length`

### Simple Example: 

```cs
class Program
    {
	static void Main(string[] args)
	{
		var handler1 = new ParserHandler();
		var parser1 = new HttpCombinedParser(handler1);

		// Test response data
		var bArray1 = Encoding.UTF8.GetBytes(TestReponse());

		System.Console.WriteLine(parser1.Execute(new ArraySegment<byte>(bArray1, 0, bArray1.Length)) == bArray1.Length
		? $"Reponse test succeed. Type identified is; {handler1.MessageType}"
		: $"Response test failed");

// ------------------------------ part 2 -------------------------------------- //
		var handler2 = new ParserHandler();
		var parser2 = new HttpCombinedParser(handler2);
		
		// Test request data    
		var handler2 = new Parser
		var bArray2 = Encoding.UTF8.GetBytes(TestRequest());

		System.Console.WriteLine(parser2.Execute(new ArraySegment<byte>(bArray2, 0, bArray2.Length)) == bArray2.Length
		? $"Request test succeed. Type identified is; {handler2.MessageType}"
		: $"Request test failed");

		System.Console.ReadKey();
	}
        
	// Test Response
	private static string TestReponse()
	{
		var stringBuilder = new StringBuilder();
		stringBuilder.Append("HTTP/1.1 200 OK\r\n");

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

		return stringBuilder.ToString();
	}
}
```

