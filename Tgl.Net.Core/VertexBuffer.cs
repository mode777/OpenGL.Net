using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tgl.Net.Core
{
    public class BufferOptions
    {
        public BufferOptions(object data, uint vertices, IEnumerable<VertexAttribute> attributes)
        {
            Data = data;
            Vertices = vertices;
            Attributes = attributes;            
        }

        public object Data { get; }
        public uint Vertices { get; }
        public IEnumerable<VertexAttribute> Attributes { get; }
        public BufferUsage Usage { get; set; } = BufferUsage.StaticDraw;

    }

    public class VertexAttribute
    {
        public VertexAttribute(string name, uint components)
        {
            Name = name;
            if (components > 4 || components < 1)
                throw new InvalidOperationException("Components has to be between 1 and 4");

            Components = components;
        }

        public VertexAttribute(string name, uint components, VertexAttribType type)
            : this(name, components)
        {
            DataType = type;
        }

        public string Name { get; }
        public uint Components { get; }
        public bool Normalized { get; set; } = false;
        public uint? Offset { get; set; } = null;
        public VertexAttribType DataType { get; set; } = VertexAttribType.Float;
    }

    public class VertexBuffer
    {
        private readonly TglContext _context;
        private readonly VertexAttribute[] _attributes;
        private readonly Dictionary<string, VertexAttribute> _attributesByName = new Dictionary<string, VertexAttribute>();
        private readonly BufferUsage _usage;
        private readonly uint _handle;
        private uint _byteSize;
        private uint _vertexSize;
        private uint _vertices;

        public VertexBuffer(TglContext context, BufferOptions options)
        {
            _context = context;
            _usage = options.Usage;
            
            uint offset = 0;

            _attributes = options.Attributes.Select(x => 
            {
                var res = new VertexAttribute(x.Name, x.Components)
                {
                    Normalized = x.Normalized,
                    Offset = x.Offset.HasValue ? x.Offset : offset
                };

                offset += GetComponentSize(x.DataType) * x.Components;

                _attributesByName[res.Name] = res;
                return res;
            }).ToArray();
            
            _handle = Gl.GenBuffer();

            Data(options.Data, options.Vertices);
        }

        public uint VertexCount => _vertices;
        public uint VertexSize => _vertexSize;
        public uint Size => _byteSize;

        public void Bind()
        {
            _context.State.VertexBuffer.Set(_handle);
        }

        private void SubData(object data, uint vertexOffset, uint vertexLength)
        {
            Bind();
            using (var pinned = new MemoryLock(data))
            {
                Gl.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)(_vertexSize * vertexOffset), _vertexSize * vertexLength, pinned.Address);
            }
        }

        public void Data(object data, uint vertexLength)
        {
            Bind();
            CalculateSize(vertexLength);
            using(var pinned = new MemoryLock(data))
            {
                Gl.BufferData(BufferTarget.ArrayBuffer, _byteSize, pinned.Address, _usage);
            }
        }

        public void EnableAttribute(string name, int location)
        {
            Bind();
            var a = _attributesByName[name];

            Gl.EnableVertexAttribArray((uint)location);
            Gl.VertexAttribPointer(
                (uint)location, 
                (int)a.Components, 
                a.DataType, 
                a.Normalized, 
                (int)_vertexSize, 
                (IntPtr)a.Offset);
        }

        private uint GetComponentSize(VertexAttribType type)
        {
            switch (type)
            {
                case VertexAttribType.Byte:
                case VertexAttribType.UnsignedByte:
                    return 1;
                case VertexAttribType.Short:
                case VertexAttribType.UnsignedShort:
                    return 2;
                case VertexAttribType.Int:
                case VertexAttribType.UnsignedInt:
                case VertexAttribType.Float:
                    return 4;      
                default:
                    throw new NotSupportedException($"DataType {type} is not supported.");
            }
        }

        private void CalculateSize(uint vertices)
        {
            _vertices = vertices;
            _vertexSize = (uint)_attributes.Sum(x => GetComponentSize(x.DataType) * x.Components);
            _byteSize = _vertexSize * _vertices;
        }


        
    }

}
