using System;
using System.Collections.Generic;
using System.Text;

namespace AppHost
{
    public interface IGLApp
    {
        void Initialize();
        void Update();
        void Render(int width, int height);
        void Stop();
    }
}
