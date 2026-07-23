using System;

namespace IHttpMachine;

/// <summary>
/// Callbacks raised while parsing an HTTP request line. All of them arrive between
/// <see cref="IHttpParserDelegate.OnMessageBegin"/> and the first header callback.
/// </summary>
public interface IHttpRequestParserDelegate : IHttpParserDelegate, IDisposable
{
    /// <summary>Called when the message has been identified as a request.</summary>
    void OnRequestType(IHttpCombinedParser combinedParser);

    /// <summary>Called with the request method (for example <c>GET</c> or <c>NOTIFY</c>).</summary>
    void OnMethod(IHttpCombinedParser combinedParser, string method);

    /// <summary>Called with the complete request target as it appeared on the request line.</summary>
    void OnRequestUri(IHttpCombinedParser combinedParser, string requestUri);

    /// <summary>Called with the path portion of the request target.</summary>
    void OnPath(IHttpCombinedParser combinedParser, string path);

    /// <summary>Called with the fragment portion of the request target, when present.</summary>
    void OnFragment(IHttpCombinedParser combinedParser, string fragment);

    /// <summary>Called with the query string portion of the request target (without the leading <c>?</c>), when present.</summary>
    void OnQueryString(IHttpCombinedParser combinedParser, string queryString);
}
