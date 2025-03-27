using System;
using IHttpMachine.Model;

namespace IHttpMachine;

public interface IHttpParserDelegate
{
    HttpRequestResponse HttpRequestResponse { get; }
    void OnMessageBegin(IHttpCombinedParser combinedParser);
    void OnHeaderName(IHttpCombinedParser combinedParser, string name);
    void OnHeaderValue(IHttpCombinedParser combinedParser, string value);
    void OnHeadersEnd(IHttpCombinedParser combinedParser);
    void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked);
    void OnChunkedLength(IHttpCombinedParser combinedParser, int length);
    void OnChunkReceived(IHttpCombinedParser combinedParser);
    void OnBody(IHttpCombinedParser combinedParser, ArraySegment<byte> data);
    void OnMessageEnd(IHttpCombinedParser combinedParser);
    void OnParserError();
}

