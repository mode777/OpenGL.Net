using OpenGL.CoreUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppHost.Hosts
{
    internal class Win32Host : IGLHost
    {
        const int Width = 640;
        const int Height = 480;

        private NativeWindow _nativeWindow = NativeWindow.Create();
        private IGLApp _app;

        public Win32Host(IGLApp app)
        {
            _app = app ?? throw new NullReferenceException(nameof(app));

            _nativeWindow.ContextCreated += NativeWindow_ContextCreated;
            _nativeWindow.Render += NativeWindow_Render;
            _nativeWindow.KeyDown += (object obj, NativeWindowKeyEventArgs e) => {
                switch (e.Key)
                {
                    case KeyCode.Escape:
                        _nativeWindow.Stop();
                        break;

                    case KeyCode.F:
                        _nativeWindow.Fullscreen = !_nativeWindow.Fullscreen;
                        break;
                }
            };
            _nativeWindow.Animation = true;

            _nativeWindow.Create(0, 0, Width, Height, NativeWindowStyle.Overlapped);

            _nativeWindow.Show();
            _nativeWindow.Run();
        }

        public string Name => "CoreUI";

        private void NativeWindow_Render(object sender, NativeWindowEventArgs e)
        {
            _app.Render((int)_nativeWindow.Width, (int)_nativeWindow.Height);
            _app.Update();
        }

        private void NativeWindow_ContextCreated(object sender, NativeWindowEventArgs e)
        {
            _app.Initialize();
        }

        public void Dispose()
        {
            _app.Stop();
            _nativeWindow.Dispose();
        }

    }
}
