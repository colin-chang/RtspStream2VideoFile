# RtspStream2VideoFile

This can help you to save a video file from a specify rtsp address,it works based on ffmpeg.Only windows is supported for current version.

## [Nuget](https://www.nuget.org/packages/ColinChang.RtspStream2VideoFile/)
```sh
# Package Manager
Install-Package ColinChang.RtspStream2VideoFile

# .NET CLI
dotnet add package ColinChang.RtspStream2VideoFile
```

## Important Note
* You must download the `FFmpeg` resource separately from [here](https://github.com/colin-chang/RtspStream2VideoFile/tree/master/ColinChang.RtspStream2VideoFile/FFmpeg). I can not pack it in the nuget package since it's too large.
* You must copy the `FFmpeg` folder to your project output path or set all of the files Copy to Output Directory be "Always Copy" which under `FFmpeg` folder.
* The target file extension should be **mp4**
* Only Windows is supported currently both of x86 and x64.

About the details of how to use this,please check the [unit test project](https://github.com/colin-chang/RtspStream2VideoFile/tree/master/ColinChang.RtspStream2VideoFile.Test).

## Compatibility
It can not work with `Microsoft.NET.Sdk.Web` but `Microsoft.NET.Sdk`.It means this can be used in an Asp.Net Core Application,fortunately any other applicaitons can work well with this.

![compatibility](compatibility.jpg)

## TODO
* Compatible with Mac OS and Linux.
* Other media types support
