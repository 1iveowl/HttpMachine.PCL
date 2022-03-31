using HttpMachine;
using System.IO;

namespace IHttpMachine.Model
{
    public interface IHttpRequestResponse : IParserControl
    {
        public MessageTypeKind MessageType { get; }
        public int StatusCode { get; }
        public string ResponseReason { get; }
        public int MajorVersion { get; }
        public int MinorVersion { get; }
        public bool ShouldKeepAlive { get; }
        public object UserContext { get; }
        public string Method { get; }
        public string RequestUri { get; }
        public string Path { get; }
        public string QueryString { get; }
        public string Fragment { get; }
        public MemoryStream Body { get;}
    }
}
