using System;

namespace IHttpMachine;

/// <summary>
/// Optional companion to <see cref="IHttpParserDelegate"/>. When a delegate also implements
/// this interface, body data from the span-based <c>Execute</c> overload is delivered directly
/// as a <see cref="ReadOnlySpan{T}"/> without copying. Delegates that only implement
/// <see cref="IHttpParserDelegate"/> still work with span input via a pooled copy.
/// The span (or segment) is only valid for the duration of the callback; copy it if you
/// need to keep the data.
/// </summary>
public interface IHttpParserSpanDelegate
{
    /// <summary>
    /// Called with body data parsed from span input. The span is only valid for the duration
    /// of the callback — copy the bytes if you need to keep them. May be called multiple
    /// times per message.
    /// </summary>
    void OnBody(IHttpCombinedParser combinedParser, ReadOnlySpan<byte> data);
}
