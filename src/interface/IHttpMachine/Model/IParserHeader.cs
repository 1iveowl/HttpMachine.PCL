using System.Collections.Generic;

namespace IHttpMachine.Model
{
    public interface IParserHeader
    {
        IDictionary<string, IEnumerable<string>> Headers { get; }
        bool IsTransferEncodingChunked { get; }
        int ChunkSize { get; }
    }
}

