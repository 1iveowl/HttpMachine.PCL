using IHttpMachine.Model;

namespace HttpMachine.Model.Internal;

internal abstract class InternalParserControl : InternalHttpHeader, IParserControl
{
    public bool IsEndOfMessage { get; internal set; }

    public bool IsRequestTimedOut { get; internal set; }

    public bool IsUnableToParseHttp { get; internal set; }

    public string RemoteAddress { get; internal set; }

    public int RemotePort { get; internal set; }
}
