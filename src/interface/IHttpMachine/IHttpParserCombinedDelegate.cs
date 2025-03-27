using System;

namespace IHttpMachine;

public interface IHttpParserCombinedDelegate : IHttpParserDelegate, IHttpRequestParserDelegate, IHttpResponseParserDelegate, IDisposable
{
}
