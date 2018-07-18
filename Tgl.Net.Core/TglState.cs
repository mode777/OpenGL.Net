using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tgl.Net.Core
{
    public interface IAccessor<T>
    {
        event EventHandler<T> Changed;
        void Set(T value, bool cacheOnly = false);
        T Get();
    }

    public class TglState
    {
        public TglState()
        {           

            Gl.GetInteger<TextureUnit>(GetPName.ActiveTexture , out var textureUnit);
            ActiveTexture = new Accessor<uint>((uint)textureUnit - (uint)TextureUnit.Texture0, v => Gl.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + v)));

            Gl.GetFloat<Vertex4f>(GetPName.ColorClearValue, out var clearColor);
            ClearColor = new Accessor<Vertex4f>(clearColor, v => Gl.ClearColor(v.x, v.y, v.z, v.w));

            Gl.GetInteger<Vertex4i>(GetPName.Viewport, out var viewport);
            Viewport = new Accessor<Vertex4i>(viewport, v => Gl.Viewport(v.x, v.y, v.z, v.w));

            BlendingEnabled = new Accessor<bool>(Gl.IsEnabled(EnableCap.Blend), v => Gl.Enable(EnableCap.Blend));
            FaceCullingEnabled = new Accessor<bool>(Gl.IsEnabled(EnableCap.CullFace), v => Gl.Enable(EnableCap.CullFace));
            DepthTestEnabled = new Accessor<bool>(Gl.IsEnabled(EnableCap.DepthTest), v => Gl.Enable(EnableCap.DepthTest));
            ScissorTestEnabled = new Accessor<bool>(Gl.IsEnabled(EnableCap.ScissorTest), v => Gl.Enable(EnableCap.ScissorTest));
            StencilTestEnabled = new Accessor<bool>(Gl.IsEnabled(EnableCap.StencilTest), v => Gl.Enable(EnableCap.StencilTest));
            Texture = new TextureAccessor(ActiveTexture, 0, v => Gl.BindTexture(TextureTarget.Texture2d, v));
            Framebuffer = new Accessor<uint>(0, v => Gl.BindFramebuffer(FramebufferTarget.Framebuffer, v));
            VertexBuffer = new Accessor<uint>(0, v => Gl.BindBuffer(BufferTarget.ArrayBuffer, v));
            IndexBuffer = new Accessor<uint>(0, v => Gl.BindBuffer(BufferTarget.ElementArrayBuffer, v));
            Program = new Accessor<uint>(0, v => Gl.UseProgram(v));
            Renderbuffer = new Accessor<uint>(0, v => Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, v));
        }

        public IAccessor<uint> ActiveTexture { get; }
        public IAccessor<Vertex4f> ClearColor { get; }
        public IAccessor<Vertex4i> Viewport { get; }
        public IAccessor<bool> BlendingEnabled { get; }
        public IAccessor<bool> FaceCullingEnabled { get; }
        public IAccessor<bool> DepthTestEnabled { get; }
        public IAccessor<bool> ScissorTestEnabled { get; }
        public IAccessor<bool> StencilTestEnabled { get; }
        public IAccessor<uint> Texture { get; }
        public IAccessor<uint> Framebuffer { get; }
        public IAccessor<uint> VertexBuffer { get; }
        public IAccessor<uint> IndexBuffer { get; }
        public IAccessor<uint> Program { get; }
        public IAccessor<uint> Renderbuffer { get; }
        
        private class Accessor<T> : IAccessor<T>
        {
            private T _value;
            private readonly Action<T> _setter;

            public Accessor(T initialValue, Action<T> setter)
            {
                _value = initialValue;
                _setter = setter;
            }

            public event EventHandler<T> Changed;

            public void Set(T value, bool cacheOnly = false)
            {
                if (!value.Equals(_value))
                {
                    if (!cacheOnly)
                        _setter(value);

                    _value = value;
                    Changed?.Invoke(this, _value);
                }
            }

            public T Get() => _value;
        }

        private class TextureAccessor : IAccessor<uint>
        {
            private readonly IAccessor<uint> _unitAccessor;
            private readonly Action<uint> _setter;
            private uint[] _values;
            
            public TextureAccessor(IAccessor<uint> unitAccessor, uint initialValue, Action<uint> setter)
            {
                Gl.GetInteger<int>(GetPName.MaxCombinedTextureImageUnits, out var maxUnits);
                _values = new uint[maxUnits];

                _unitAccessor = unitAccessor;
                _values[unitAccessor.Get()] = initialValue;
                _setter = setter;
            }

            public event EventHandler<uint> Changed;

            public uint Get() => _values[_unitAccessor.Get()];

            public void Set(uint value, bool cacheOnly = false)
            {
                var active = _unitAccessor.Get(); 

                if (!value.Equals(_values[active]))
                {
                    if (!cacheOnly)
                        _setter(value);

                    _values[active] = value;
                    Changed?.Invoke(this, value);
                }
            }
        }
    }
}
