using AppHost;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using Tgl.Net.Core;

namespace HelloTesseract
{
    class TestApp : IGLApp
    {
        private TglContext _context;
        private VertexBuffer _buffer;
        private Shader _shader;

        public void Initialize()
        {
            _context = new TglContext();

            _shader = new Shader(_context, new ShaderOptions(vertex, fragment));

            _buffer = new VertexBuffer(_context, new BufferOptions(
                new float[] { -0.5f, -0.5f, 0.5f, -0.5f, 0f, 0.5f },
                3,
                new VertexAttribute[] { new VertexAttribute("aPosition", 2) }));

            _buffer.EnableAttribute("aPosition", _shader.GetAttributeLocation("aPosition"));            
        }

        public void Render(int width, int height)
        {
            _context.Clear(ClearBufferMask.ColorBufferBit, new Vertex4f(1, 0, 0, 1));
            Gl.DrawArrays(PrimitiveType.Triangles, 0, (int)_buffer.VertexCount);
        }

        public void Stop()
        {
        }

        public void Update()
        {
        }

        private const string vertex = @"attribute vec2 aPosition;
        void main(void) {
            gl_Position = vec4(aPosition, 1.0, 1.0);
        }";

        private const string fragment = @"precision mediump float;
        void main(void) {
            gl_FragColor = vec4(1, 1, 1, 1);
        }";
    }
}
