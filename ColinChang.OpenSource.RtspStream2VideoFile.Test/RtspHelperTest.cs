using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using Xunit;

namespace ColinChang.OpenSource.RtspStream2VideoFile.Test
{
    public class RtspHelperTest
    {
        const string url = "rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream";
        readonly string fileName = $@"C:\Temp\{DateTime.Now:yyyyMMddhhmmss}.mp4";

        [Fact]
        public void RecordTest()
        {
            var rtsp = new RtspHelper(url, _ => true);
            ThreadPool.QueueUserWorkItem(state => rtsp.Start(fileName));

            Thread.Sleep(5000);
            rtsp.Stop();
            Assert.True(File.Exists(fileName));
        }

        [Fact]
        public void ExternalControlTest()
        {
            const string key = "cameraTest";
            MemoryCache.Default[key] = url;

            var rtsp = new RtspHelper(url,
                addr => MemoryCache.Default.Contains(key),
                (s, e) =>
                {
                    //异常处理
                    (s as RtspHelper).Stop();
                    MemoryCache.Default.Remove(key);
                }
            );
            ThreadPool.QueueUserWorkItem(state => rtsp.Start(fileName));

            Thread.Sleep(5000);
            MemoryCache.Default.Remove(key);
            Assert.True(File.Exists(fileName));
        }
    }
}