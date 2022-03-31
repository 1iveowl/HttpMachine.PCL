using System;
using System.IO;
using HttpMachine;

namespace IHttpMachine.Model
{
    public record HttpRequestResponse : ParserControlBase, IHttpRequestResponse
    {
        public MessageTypeKind MessageType { get; init; }
        public int StatusCode { get; init; }
        public string ResponseReason { get; init; }
        public int MajorVersion { get; init; }
        public int MinorVersion { get; init; }
        public bool ShouldKeepAlive { get; init; }
        public object UserContext { get; init; }
        public string Method { get; init; }
        public string RequestUri { get; init; }
        public string Path { get; init; }
        public string QueryString { get; init; }
        public string Fragment { get; init; }
        public MemoryStream Body { get; init; }

        public HttpRequestResponse(
            IHttpRequestResponse httpRequestResponse) : base(httpRequestResponse, httpRequestResponse)
        {
            MessageType = httpRequestResponse.MessageType;
            StatusCode = httpRequestResponse.StatusCode;
            ResponseReason = httpRequestResponse.ResponseReason;
            MajorVersion = httpRequestResponse.MajorVersion;
            MinorVersion = httpRequestResponse.MinorVersion;
            ShouldKeepAlive = httpRequestResponse.ShouldKeepAlive;
            UserContext = httpRequestResponse.UserContext;
            Method = httpRequestResponse.Method;
            RequestUri = httpRequestResponse.RequestUri;
            Path = httpRequestResponse.Path;
            QueryString = httpRequestResponse.QueryString;
            Fragment = httpRequestResponse.Fragment;
            Body = httpRequestResponse.Body;
        }
    }
}
