using System;

namespace IHttpMachine;

/// <summary>
/// Callbacks raised while parsing an HTTP response status line. They arrive between
/// <see cref="IHttpParserDelegate.OnMessageBegin"/> and the first header callback.
/// </summary>
public interface IHttpResponseParserDelegate : IHttpParserDelegate, IDisposable
{
    /// <summary>Called when the message has been identified as a response.</summary>
    void OnResponseType(IHttpCombinedParser combinedParser);

    /// <summary>Called with the status code and reason phrase from the status line.</summary>
    void OnResponseCode(IHttpCombinedParser combinedParser, int statusCode, string statusReason);
}
