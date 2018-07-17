using AppHost.Hosts;
using System;
using System.Runtime.InteropServices;

namespace AppHost
{
    public static class GLHostFactory
    {
        public static IGLHost CreateHost(IGLApp app)
        {
            var os = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            var arch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
            Console.WriteLine(os);
            Console.WriteLine(arch);
            if(os.ToLower().Contains("windows"))
            {
                Console.WriteLine("Starting Win32 host...");
                return new Win32Host(app);
            }
            else if(os.ToLower().Contains("linux") && arch.HasFlag(Architecture.Arm))
            {
                Console.WriteLine("Starting VideoCore (RaspberryPi) host...");
                return new VideoCoreHost(app);
            }
            else
            {
                throw new NotImplementedException($"Unsuported OS: {os} on {arch}");
            }
        }
    }
}
