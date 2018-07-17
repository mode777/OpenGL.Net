using System;
using System.Collections.Generic;
using System.Text;

namespace AppHost
{
    public interface IGLHost : IDisposable
    {
        string Name { get; }
    }
}
