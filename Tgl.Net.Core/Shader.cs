using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tgl.Net.Core
{
    public class ShaderOptions
    {
        public ShaderOptions(string vertexSource, string fragmentSource)
        {
            VertexSource = vertexSource;
            FragmentSource = fragmentSource;
        }

        public string VertexSource { get; }
        public string FragmentSource { get; }
    }

    public class Shader : IDisposable
    {
        private readonly TglContext _context;
        private readonly uint _handle;
        private readonly Dictionary<string, ShaderVariableInfo> _uniformsByName 
            = new Dictionary<string, ShaderVariableInfo>();
        private readonly Dictionary<string, ShaderVariableInfo> _attributesByName 
            = new Dictionary<string, ShaderVariableInfo>();

        public Shader(TglContext context, ShaderOptions options)
        {
            _context = context;

            StringBuilder infolog = new StringBuilder(1024);
            infolog.EnsureCapacity(1024);
            int infologLength;
            int compiled;
            
            var vertexShader = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(vertexShader, new string[] { options.VertexSource });
            Gl.CompileShader(vertexShader);
            Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(vertexShader, 1024, out infologLength, infolog);
                throw new InvalidProgramException(infolog.ToString());
            }

            var fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(fragmentShader, new string[] { options.FragmentSource });
            Gl.CompileShader(fragmentShader);
            Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out compiled);
            if (compiled == 0)
            {
                Gl.GetShaderInfoLog(fragmentShader, 1024, out infologLength, infolog);
                throw new InvalidProgramException(infolog.ToString());
            }
            
            _handle = Gl.CreateProgram();
            Gl.AttachShader(_handle, vertexShader);
            Gl.AttachShader(_handle, fragmentShader);
            Gl.LinkProgram(_handle);
            Gl.GetProgram(_handle, ProgramProperty.LinkStatus, out var linked);
            if (linked == 0)
            {
                Gl.GetProgramInfoLog(_handle, 1024, out infologLength, infolog);
                throw new InvalidProgramException(infolog.ToString());
            }

            Gl.DeleteShader(vertexShader);
            Gl.DeleteShader(fragmentShader);

            Use();
            CollectAttributeInformation();
            CollectUniformInformation();
        }

        public void Use()
        {
            _context.State.Program.Set(_handle);            
        }

        public int GetUniformLocation(string name)
        {
            return _uniformsByName[name].Location;
        }

        public int GetAttributeLocation(string name)
        {
            return _attributesByName[name].Location;
        }

        public void SetUniform(int location, float value)
        {
            Use();
            Gl.Uniform1f(location, 1, value);
        }
        public void SetUniform(string name, float value) => SetUniform(_uniformsByName[name].Location, value);

        public void SetUniform(int location, Vertex2f value)
        {
            Use();
            Gl.Uniform2f(location, 1, value);
        }
        public void SetUniform(string name, Vertex2f value) => SetUniform(_uniformsByName[name].Location, value);

        public void SetUniform(int location, Vertex3f value)
        {
            Use();
            Gl.Uniform3f(location, 1, value);
        }
        public void SetUniform(string name, Vertex3f value) => SetUniform(_uniformsByName[name].Location, value);

        public void SetUniform(int location, Vertex4f value)
        {
            Use();
            Gl.Uniform4f(location, 1, value);
        }
        public void SetUniform(string name, Vertex4f value) => SetUniform(_uniformsByName[name].Location, value);
        
        public void SetUniform(int location, Matrix2x2f value)
        {
            Use();
            Gl.UniformMatrix2f(location, 1, false, value);
        }
        public void SetUniform(string name, Matrix2x2f value) => SetUniform(_uniformsByName[name].Location, value);
        
        public void SetUniform(int location, Matrix3x3f value)
        {
            Use();
            Gl.UniformMatrix3f(location, 1, false, value);
        }
        public void SetUniform(string name, Matrix3x3f value) => SetUniform(_uniformsByName[name].Location, value);

        public void SetUniform(int location, Matrix4x4f value)
        {
            Use();
            Gl.UniformMatrix4f(location, 1, false, value);
        }
        public void SetUniform(string name, Matrix4x4f value) => SetUniform(_uniformsByName[name].Location, value);

        public void Dispose()
        {
            Gl.DeleteProgram(_handle);
        }

        private void CollectUniformInformation()
        {
            Gl.GetProgram(_handle, ProgramProperty.ActiveUniforms, out var uniforms);

            StringBuilder nameBuffer = new StringBuilder(1024);
            nameBuffer.EnsureCapacity(1024);
            int namebufferLength;
            int size;
            int type;
            int location;
            string name;

            for (uint i = 0; i < uniforms; i++)
            {
                Gl.GetActiveUniform(_handle, i, 1024, out namebufferLength, out size, out type, nameBuffer);
                name = nameBuffer.ToString();
                location = Gl.GetUniformLocation(_handle, name);
                _uniformsByName[nameBuffer.ToString()] = new ShaderVariableInfo
                {
                    Location = location,
                    Name = name,
                    Size = size,
                    Type = (AttributeType)type
                };
            }
        }

        private void CollectAttributeInformation()
        {
            Gl.GetProgram(_handle, ProgramProperty.ActiveAttributes, out var attributes);

            StringBuilder nameBuffer = new StringBuilder(1024);
            nameBuffer.EnsureCapacity(1024);
            int namebufferLength;
            int size;
            int type;
            int location;
            string name;

            for (uint i = 0; i < attributes; i++)
            {
                Gl.GetActiveAttrib(_handle, i, 1024, out namebufferLength, out size, out type, nameBuffer);
                name = nameBuffer.ToString();
                location = Gl.GetAttribLocation(_handle, name);
                _attributesByName[nameBuffer.ToString()] = new ShaderVariableInfo
                {
                    Location = location,
                    Name = name,
                    Size = size,
                    Type = (AttributeType)type
                };
            }
        }

        private class ShaderVariableInfo
        {
            public string Name { get; set; }
            public int Location { get; set; }
            public int Size { get; set; }
            public AttributeType Type { get; set; }
        }
    }
}
