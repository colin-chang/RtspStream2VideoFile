using System;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ColinChang.RtspStream2VideoFile.Test
{
    public class RtspRecorderTest
    {
        const string rtsp = "rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream";

        /// <summary>
        /// record a 5 seconds video and stop recorder by itself instance
        /// </summary>
        [Fact]
        public void InternalContolTest()
        {
            var fileName = GetFileName(nameof(InternalContolTest));
            var recorder = new RtspRecorder(rtsp, fileName);


            ThreadPool.QueueUserWorkItem(_ => recorder.Start());
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
            var fileName = GetFileName(nameof(ExternalControlTest));

            var recorder = new RtspRecorder(rtsp, fileName,
                addr => MemoryCache.Default[key]?.ToString() == addr,
                (s, e) =>
                {
                    //OnException
                    (s as RtspRecorder).Stop();
                    MemoryCache.Default[key] = string.Empty;
                }
            );
            MemoryCache.Default[key] = rtsp;
            ThreadPool.QueueUserWorkItem(state => recorder.Start());

            Thread.Sleep(5000);
            MemoryCache.Default[key] = string.Empty;
            Assert.True(File.Exists(fileName));
        }

        private string GetFileName(string testMethodName) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testMethodName, $"{DateTime.Now:yyyyMMddhhmmss}.mp4");
    }
}