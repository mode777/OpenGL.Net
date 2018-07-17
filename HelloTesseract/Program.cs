using AppHost;
using System;

namespace HelloTesseract
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = GLHostFactory.CreateHost(new SimpleApp()))
            {               
            }           
        }
    }
}
