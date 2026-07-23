using System.Collections.Generic;

namespace IHttpMachine.Model;

/// <summary>Header-related results of parsing an HTTP message.</summary>
public interface IParserHeader
{
    /// <summary>
    /// The parsed headers. Keys are uppercased and the dictionary uses a case-insensitive
    /// comparer; repeated headers accumulate their values in order of appearance.
    /// </summary>
    IDictionary<string, IEnumerable<string>> Headers { get; }

    /// <summary>Whether the message used <c>Transfer-Encoding: chunked</c>.</summary>
    bool IsTransferEncodingChunked { get; }

    /// <summary>Size in bytes of the most recently parsed chunk when the body was chunked; otherwise <c>0</c>.</summary>
    int ChunkSize { get; }
}
