using IHttpMachine.Model;
using System.IO;

namespace HttpMachine.Model.Internal
{
    internal class InternalHttpRequestResponse : InternalParserControl, IHttpRequestResponse
    {
        private static readonly byte[] _body;

        public MessageTypeKind MessageType { get; set; }
        public int StatusCode { get; set; }
        public string ResponseReason { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public bool ShouldKeepAlive { get; set; }
        public object UserContext { get; set; }
        public string Method { get; set; }
        public string RequestUri { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public string Fragment { get; set; }
        public MemoryStream Body { get; set; } = new MemoryStream();
    }
}
