using System;

namespace IHttpMachine;

/// <summary>
/// The delegate contract expected by the combined request/response parser: the union of
/// <see cref="IHttpParserDelegate"/>, <see cref="IHttpRequestParserDelegate"/> and
/// <see cref="IHttpResponseParserDelegate"/>. Check
/// <see cref="Model.IHttpRequestResponse.MessageType"/> on the result to see which kind
/// of message was parsed.
/// </summary>
public interface IHttpParserCombinedDelegate : IHttpParserDelegate, IHttpRequestParserDelegate, IHttpResponseParserDelegate, IDisposable
{
}
