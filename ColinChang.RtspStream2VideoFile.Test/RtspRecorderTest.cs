using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using Xunit;

namespace ColinChang.RtspStream2VideoFile.Test
{
    public class RtspRecorderTest
    {
        const string rtsp = "rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream";
        readonly string fileName = $@"C:\Temp\{DateTime.Now:yyyyMMddhhmmss}.mp4";

        /// <summary>
        /// record a 5 seconds video and stop recorder by itself instance
        /// </summary>
        [Fact]
        public void InternalContolTest()
        {
            var recorder = new RtspRecorder(rtsp);
            ThreadPool.QueueUserWorkItem(_ => recorder.Start(fileName));

            Thread.Sleep(5000);
            recorder.Stop();
            Assert.True(File.Exists(fileName));
        }

        /// <summary>
        /// record a 5 seconds video and stop recorder by changing its dependent prediction
        /// </summary>
        [Fact]
        public void ExternalControlTest()
        {
            const string key = "cameraTest";

            var recorder = new RtspRecorder(rtsp,
                addr => MemoryCache.Default[key]?.ToString() == addr,
                (s, e) =>
                {
                    //OnException
                    (s as RtspRecorder).Stop();
                    MemoryCache.Default[key] = null;
                }
            );
            MemoryCache.Default[key] = rtsp;
            ThreadPool.QueueUserWorkItem(state => recorder.Start(fileName));

            Thread.Sleep(5000);
            MemoryCache.Default[key] = null;
            Assert.True(File.Exists(fileName));
        }
    }
}