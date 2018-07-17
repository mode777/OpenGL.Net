using AppHost;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloTesseract
{
    public class TesseractApp : IGLApp
    {
        private static readonly string[] CubeEdgeProgramVertexSource = new string[] {
            "#version 330\n",
            "uniform mat4 uMVP;\n",
            "uniform float uScale4D = 1.0;\n",
            "in vec3 aPosition;\n",
            "out vec3 vColor;\n",
            "void main() {\n",
            "	gl_Position = uMVP * vec4(aPosition, uScale4D);\n",
            "}\n"
        };
        private static readonly string[] _CubeEdgeProgramFragmentSource = new string[] {
            "#version 330\n",
            "uniform vec4 uColor = vec4(1.0);\n",
            "void main() {\n",
            "	gl_FragColor = uColor;\n",
            "}\n"
        };

        private float _angle;
        private float _zoom = 1.0f;
        private uint _frameNo = 0;
        private uint _cubeEdgeProgram;
        private int _cubeEdgeProgram_Location_uMVP;
        private int _cubeEdgeProgram_Location_uScale4D;
        private int _cubeEdgeProgram_Location_uColor;
        private int _cubeEdgeProgram_Location_aPosition;       
        private uint _cubeVerticesBuffer;
        private uint _cubeEdgesBuffer;
        private uint _cubeVao;

        private Vertex3f[] _CubeVertices = new Vertex3f[] {
            new Vertex3f(-1.0f, -1.0f, -1.0f),
            new Vertex3f(+1.0f, -1.0f, -1.0f),
            new Vertex3f(+1.0f, +1.0f, -1.0f),
            new Vertex3f(-1.0f, +1.0f, -1.0f),
            new Vertex3f(-1.0f, -1.0f, +1.0f),
            new Vertex3f(+1.0f, -1.0f, +1.0f),
            new Vertex3f(+1.0f, +1.0f, +1.0f),
            new Vertex3f(-1.0f, +1.0f, +1.0f),
        };

        private ushort[] _CubeEdges = new ushort[] {
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        };

        public void Initialize()
        {
            //Gl.ReadBuffer(ReadBufferMode.Back);

            Gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Gl.LineWidth(2.5f);

            CreateResources();
        }

        public void Render(int width, int height)
        {
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(_cubeEdgeProgram);

            // Compute MVP
            Matrix4x4f proj = Matrix4x4f.Perspective(60.0f, width / (float)height, 0.5f, 1e6f);
            Matrix4x4f view = Matrix4x4f.LookAt(Vertex3f.One * 7.0f, Vertex3f.Zero, Vertex3f.UnitY);
            Matrix4x4f model = Matrix4x4f.RotatedY(_angle) * Matrix4x4f.RotatedZ(_angle);

            Gl.BindVertexArray(_cubeVao);

            Gl.UniformMatrix4f(_cubeEdgeProgram_Location_uMVP, 1, false, proj * view * model);

            foreach (float scale4d in new float[] { 64.0f, 32.0f, 16.0f, 8.0f, 4.0f, 2.0f, 1.0f, 0.5f, 0.25f, 0.125f })
            {
                Gl.Uniform1(_cubeEdgeProgram_Location_uScale4D, scale4d * _zoom);
                Gl.Uniform4(_cubeEdgeProgram_Location_uColor, 0.0f, 0.3f, 1.0f, Math.Min(1.0f, scale4d * _zoom / 2.0f));
                Gl.DrawElements(PrimitiveType.Lines, _CubeEdges.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }

            _angle += 360.0f / (25.0f * 5);
            _zoom -= 0.025f;

            if (_zoom < 0.5f)
                _zoom = 1.0f;
        }

        public void Update()
        {
        }

        public void Stop()
        {
            Gl.DeleteBuffers(_cubeVerticesBuffer, _cubeEdgesBuffer);
            Gl.DeleteVertexArrays(_cubeVao);
        }

        private void CreateResources()
        {
            CreateCubeEdgeProgram();
            CreateCubeEdgeVertexArray();
        }

        private void CreateCubeEdgeProgram()
        {
            StringBuilder infolog = new StringBuilder(1024);
            int infologLength;
            int compiled;

            infolog.EnsureCapacity(1024);

            // Vertex shader
            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, CubeEdgeProgramVertexSource);
            Gl.CompileShader(vertexShader);
            Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(vertexShader, 1024, out infologLength, infolog);
            }

            // Fragment shader
            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, _CubeEdgeProgramFragmentSource);
            Gl.CompileShader(fragmentShader);
            Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(fragmentShader, 1024, out infologLength, infolog);
            }

            // Program
            _cubeEdgeProgram = Gl.CreateProgram();
            Gl.AttachShader(_cubeEdgeProgram, vertexShader);
            Gl.AttachShader(_cubeEdgeProgram, fragmentShader);
            Gl.LinkProgram(_cubeEdgeProgram);

            int linked;
            Gl.GetProgram(_cubeEdgeProgram, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                Gl.GetProgramInfoLog(_cubeEdgeProgram, 1024, out infologLength, infolog);
            }

            _cubeEdgeProgram_Location_uMVP = Gl.GetUniformLocation(_cubeEdgeProgram, "uMVP");
            _cubeEdgeProgram_Location_uScale4D = Gl.GetUniformLocation(_cubeEdgeProgram, "uScale4D");
            _cubeEdgeProgram_Location_uColor = Gl.GetUniformLocation(_cubeEdgeProgram, "uColor");
            _cubeEdgeProgram_Location_aPosition = Gl.GetAttribLocation(_cubeEdgeProgram, "aPosition");
        }

        private void CreateCubeEdgeVertexArray()
        {
            _cubeVao = Gl.GenVertexArray();
            Gl.BindVertexArray(_cubeVao);

            _cubeVerticesBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _cubeVerticesBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(Vertex3f.Size * _CubeVertices.Length), _CubeVertices, BufferUsage.StaticDraw);

            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);

            _cubeEdgesBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _cubeEdgesBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(2 * _CubeEdges.Length), _CubeEdges, BufferUsage.StaticDraw);
        }
    }
}
