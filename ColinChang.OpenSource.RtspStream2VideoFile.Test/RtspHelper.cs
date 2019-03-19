using System;
using System.IO;
using System.Threading;
using Xunit;

namespace ColinChang.OpenSource.RtspStream2VideoFile.Test
{
    public class RtspHelperTest : IDisposable
    {
        const string url = "rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream";
        string path = @"C:\Temp\test.mp4";

        [Fact]
        public void SaveAsTest()
        {
            var rtsp = new RtspHelper(url);

            new Thread(() => rtsp.SaveAs(path)) {IsBackground = true}.Start();
            Thread.Sleep(5000);
            Assert.True(File.Exists(path));
        }

        public void Dispose()
        {
            File.Delete(path);
        }
    }
}