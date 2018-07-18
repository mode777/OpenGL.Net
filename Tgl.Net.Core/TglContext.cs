using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tgl.Net.Core
{
    public class TglContext
    {
        private readonly TglState _state = new TglState();

        public TglContext()
        {
        }

        public TglState State => _state;

        public void Clear(ClearBufferMask flags)
        {
            Gl.Clear(flags);
        }

        public void Clear(ClearBufferMask flags, Vertex4f color)
        {
            _state.ClearColor.Set(color);
            Clear(flags);
        }
    }
}
