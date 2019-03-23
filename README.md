# RtspStream2VideoFile

RtspStream2VideoFile库可以实现从既定RTSP地址到网路流保存为本地视频文件。具体用法参见单元测试。

## [Nuget](https://www.nuget.org/packages/ColinChang.RtspStream2VideoFile/)
```sh
# Package Manager
Install-Package ColinChang.RtspStream2VideoFile

# .NET CLI
dotnet add package ColinChang.RtspStream2VideoFile
```
## 兼容性
不支持Asp.Net Core。与Web项目使用的Microsoft.NET.Sdk.Web中某个库存在兼容性问题，目前支持Microsoft.NET.Sdk。除了Web项目都可以正常使用。

![兼容性](compatibility.jpg)

## TODO
mac OS和Linux环境兼容性调整
