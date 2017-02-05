namespace Console.NETCore.Test.Model.Base
{
    public abstract class ParseControlBase
    {
        public bool IsEndOfRequest { get; internal set; }

        public bool IsRequestTimedOut { get; internal set; } = false;

        public bool IsUnableToParseHttp { get; internal set; } = false;

        public string RemoteAddress { get; internal set; }

        public int RemotePort { get; internal set; }

    }
}
