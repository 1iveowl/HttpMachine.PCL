using System;
using HttpMachine;

namespace IHttpMachine
{
    public interface IHttpParserCombinedDelegate : IHttpParserDelegate, IHttpRequestParserDelegate, IHttpResponseParserDelegate, IDisposable
    {
    }
}
