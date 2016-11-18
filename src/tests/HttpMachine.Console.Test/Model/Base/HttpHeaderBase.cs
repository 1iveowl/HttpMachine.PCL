using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpMachine.Console.Test.Model.Base
{
    public abstract class HttpHeaderBase : ParseControlBase
    {
        public IDictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
        public bool IsTransferEncodingChunked { get; internal set; }
        public int ChunkSize { get; internal set; }

    }
}
