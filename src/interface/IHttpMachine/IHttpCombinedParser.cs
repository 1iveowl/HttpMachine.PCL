using System;

namespace IHttpMachine;

public interface IHttpCombinedParser : IDisposable
{
     int MajorVersion { get;  }
     int MinorVersion { get;  }

     bool ShouldKeepAlive { get; }
}
