# NetCore 2.0 Cross Platform OpenGL Experiments

> Forked from [OpenGL.Net](https://github.com/luca-piccioni/OpenGL.Net) - please consider contributing.

## Goal
Provide a proof of concept for OpenGL (ES2) running inside .NET Core 2.0 app (**HelloTesseract**), that...
* ...contains only OpenGL and no platform specific code.
* ...runs on multiple platforms including the Raspberry Pi.

## Building & Running

### Windows
If you have installed the .NET Core 2.0 SDK, you can just...
```bash
cd ./HelloTesseract
dotnet run
```

### Raspberry PI
There is not yet an SDK for the Raspberry PI, but you can install the runtime as described [here (step 3)](https://blogs.msdn.microsoft.com/david/2017/07/20/setting_up_raspian_and_dotnet_core_2_0_on_a_raspberry_pi/).

Then you have to build the project on your dev machine via
```bash
dotnet publish -r linux-arm
```
Go to `./HelloTesseract/bin/netcore20/publish` and copy the contents to your raspberry pi (e.g. via WinSCP). On your Pi, cd into the folder an run:
```bash
dotnet ./HelloTesseract.dll
```



