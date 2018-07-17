using Khronos;
using OpenGL;
using OpenGL.CoreUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AppHost.Hosts
{
    public class VideoCoreHost : IGLHost
    {
        private readonly IGLApp _app;
        private readonly DeviceContext _eglContext;
        private readonly VideoCoreWindow _nativeWindow;
        private readonly IntPtr _glContext;

        public VideoCoreHost(IGLApp app)
        {
            _app = app;

            Khronos.KhronosApi.Log += (s, e) => Console.WriteLine(e.ToString());
            Khronos.KhronosApi.LogEnabled = true;

            // RPi runs on EGL
            Egl.IsRequired = true;

            if (Egl.IsAvailable == false)
                throw new InvalidOperationException("EGL is not available. Aborting.");

            _nativeWindow = new VideoCoreWindow();
            _eglContext = DeviceContext.Create(_nativeWindow.Display, _nativeWindow.Handle);
            _eglContext.ChoosePixelFormat(new DevicePixelFormat(32));
            _glContext = _eglContext.CreateContext(IntPtr.Zero);
            _eglContext.MakeCurrent(_glContext);

            Run();            
        }

        public string Name => "VideoCore";

        public void Dispose()
        {
            _app.Stop();
            _eglContext.DeleteContext(_glContext);
            _nativeWindow.Dispose();
            _eglContext.Dispose();
        }

        private void Run()
        {
            _app.Initialize();
            while (true)
            {
                _app.Render(_nativeWindow.Width, _nativeWindow.Height);
                _eglContext.SwapBuffers();
                Thread.Sleep(15);
            }
        }


    }
}
