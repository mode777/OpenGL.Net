using AppHost;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace HelloTesseract
{
    public class SimpleApp : IGLApp
    {
        private static readonly string[] Es2_ShaderVertexSource = new string[] {
            "uniform mat4 uMVP;\n",
            "attribute vec2 aPosition;\n",
            "attribute vec3 aColor;\n",
            "varying vec3 vColor;\n",
            "void main() {\n",
            "	gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);\n",
            "	vColor = aColor;\n",
            "}\n"
        };

        private static readonly string[] Es2_ShaderFragmentSource = new string[] {
            "precision mediump float;\n",
            "varying vec3 vColor;\n",
            "void main() {\n",
            "	gl_FragColor = vec4(vColor, 1.0);\n",
            "}\n"
        };

        /// <summary>
        /// Vertex position array.
        /// </summary>
        private static readonly float[] ArrayPosition = new float[] {
            0.0f, 0.0f,
            0.5f, 1.0f,
            1.0f, 0.0f
        };

        /// <summary>
        /// Vertex color array.
        /// </summary>
        private static readonly float[] ArrayColor = new float[] {
            1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f
        };

        private uint _es2_Program;
        private int _es2_Program_Location_aPosition;
        private int _es2_Program_Location_aColor;
        private int _es2_Program_Location_uMVP;
        private float _angle;

        public void Initialize()
        {
            // Create resources
            StringBuilder infolog = new StringBuilder(1024);
            int infologLength;
            int compiled;

            infolog.EnsureCapacity(1024);

            // Vertex shader
            uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, Es2_ShaderVertexSource);
            Gl.CompileShader(vertexShader);
            Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(vertexShader, 1024, out infologLength, infolog);
            }

            // Fragment shader
            uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, Es2_ShaderFragmentSource);
            Gl.CompileShader(fragmentShader);
            Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(fragmentShader, 1024, out infologLength, infolog);
            }

            // Program
            _es2_Program = Gl.CreateProgram();
            Gl.AttachShader(_es2_Program, vertexShader);
            Gl.AttachShader(_es2_Program, fragmentShader);
            Gl.LinkProgram(_es2_Program);

            int linked;
            Gl.GetProgram(_es2_Program, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                Gl.GetProgramInfoLog(_es2_Program, 1024, out infologLength, infolog);
            }

            _es2_Program_Location_uMVP = Gl.GetUniformLocation(_es2_Program, "uMVP");
            _es2_Program_Location_aPosition = Gl.GetAttribLocation(_es2_Program, "aPosition");
            _es2_Program_Location_aColor = Gl.GetAttribLocation(_es2_Program, "aColor");
                        
            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        public void Render(int width, int height)
        {
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            Gl.UseProgram(_es2_Program);

            using (MemoryLock arrayPosition = new MemoryLock(ArrayPosition))
            using (MemoryLock arrayColor = new MemoryLock(ArrayColor))
            {
                Gl.VertexAttribPointer((uint)_es2_Program_Location_aPosition, 2, VertexAttribType.Float, false, 0, arrayPosition.Address);
                Gl.EnableVertexAttribArray((uint)_es2_Program_Location_aPosition);

                Gl.VertexAttribPointer((uint)_es2_Program_Location_aColor, 3, VertexAttribType.Float, false, 0, arrayColor.Address);
                Gl.EnableVertexAttribArray((uint)_es2_Program_Location_aColor);

                var mat = Matrix4x4f.Ortho(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f);
                mat.RotateZ(_angle);
                _angle += 0.1f;
                
                Gl.UniformMatrix4f(_es2_Program_Location_uMVP, 1, false, mat);
                
                Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
            }
        }

        public void Stop()
        {
            if (_es2_Program != 0)
                Gl.DeleteProgram(_es2_Program);
            _es2_Program = 0;
        }

        public void Update()
        {
        }
    }
}
