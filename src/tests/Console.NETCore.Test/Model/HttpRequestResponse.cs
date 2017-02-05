using System.IO;
using Console.NETCore.Test.Model.Base;
using HttpMachine;

namespace Console.NETCore.Test.Model
{
    public class HttpRequestReponse : HttpHeaderBase
    {
        public MessageType MessageType { get; internal set; }
        public int StatusCode { get; internal set; }
        public string ResponseReason { get; internal set; }
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }
        public bool ShouldKeepAlive { get; internal set; }
        public object UserContext { get; internal set; }
        public string Method { get; internal set; }
        public string RequestUri { get; internal set; }
        public string Path { get; internal set; }
        public string QueryString { get; internal set; }
        public string Fragment { get; internal set; }
        public MemoryStream Body { get; internal set; } = new MemoryStream();
    }


}
