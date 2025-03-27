using IHttpMachine.Model;
using System.Collections.Generic;

namespace HttpMachine.Model.Internal;

internal abstract class InternalHttpHeader : IParserHeader
{
    public IDictionary<string, IEnumerable<string>> Headers { get; internal set; }
    public bool IsTransferEncodingChunked { get; internal set; }
    public int ChunkSize { get; internal set; }
}
