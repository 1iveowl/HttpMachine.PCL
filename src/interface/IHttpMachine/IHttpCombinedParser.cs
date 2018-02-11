using System;
using System.Collections.Generic;
using System.Text;

namespace IHttpMachine
{
    public interface IHttpCombinedParser : IDisposable
    {
         int MajorVersion { get;  }
         int MinorVersion { get;  }

         bool ShouldKeepAlive { get; }
    }
}
