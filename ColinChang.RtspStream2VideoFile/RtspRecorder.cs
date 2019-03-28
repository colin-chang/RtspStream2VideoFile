using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace ColinChang.RtspStream2VideoFile
{
    public unsafe class RtspRecorder
    {
        //can containue to record video or not
        private Predicate<string> _canExecute;

        public string Rtsp { get; set; }

        public string DestinationFile { get; set; }

        public event EventHandler<RtspExceptionEventArgs> OnException;

        public RtspRecorder(string rtsp, string destinationFile) : this(rtsp, destinationFile, _ => true, null) { }

        public RtspRecorder(string rtsp, string destinationFile, Predicate<string> canExecute) : this(rtsp, destinationFile, canExecute, null) { }

        public RtspRecorder(string rtsp, string destinationFile, EventHandler<RtspExceptionEventArgs> exceptionHandler) : this(rtsp, destinationFile, _ => true, exceptionHandler) { }

        /// <summary>
        /// Constructor RTSP Processor Instance
        /// </summary>
        /// <param name="rtsp">rtsp address</param>
        /// <param name="canExecute">whether can continue to record video</param>
        /// <param name="exceptionHandler">evenhandler when exception occured</param>
        public RtspRecorder(string rtsp, string destinationFile, Predicate<string> canExecute, EventHandler<RtspExceptionEventArgs> exceptionHandler)
        {
            Rtsp = rtsp;
            DestinationFile = EnsureMp4File(destinationFile);
            _canExecute = canExecute;
            OnException = exceptionHandler;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                FFmpegBinariesHelper.RegisterFFmpegBinaries();

                #region ffmpeg 初始化

                ffmpeg.av_register_all();
                ffmpeg.avcodec_register_all();
                ffmpeg.avformat_network_init();

                #endregion

                #region ffmpeg 日志

                ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);

                void LogCallback(void* p0, int level, string format, byte* vl)
                {
                    if (level > ffmpeg.av_log_get_level()) return;

                    const int lineSize = 1024;
                    var lineBuffer = stackalloc byte[lineSize];
                    var printPrefix = 1;
                    ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                    var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                    Console.Write(line);
                }

                ffmpeg.av_log_set_callback((av_log_set_callback_callback)LogCallback);

                #endregion
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new RtspExceptionEventArgs(Rtsp, ex));
                throw;
            }
        }

        public unsafe void Start()
        {
            try
            {
                AVStream* i_video_stream = null;
                AVFormatContext* o_fmt_ctx;

                /* should set to null so that avformat_open_input() allocate a new one */
                AVFormatContext* i_fmt_ctx = null;
                if (ffmpeg.avformat_open_input(&i_fmt_ctx, Rtsp, null, null) != 0)
                    return;

                if (ffmpeg.avformat_find_stream_info(i_fmt_ctx, null) < 0)
                    return;

                //av_dump_format(i_fmt_ctx, 0, argv[1], 0);

                /* find first video stream */
                for (uint i = 0; i < i_fmt_ctx->nb_streams; i++)
                {
                    if (i_fmt_ctx->streams[i]->codec->codec_type != AVMediaType.AVMEDIA_TYPE_VIDEO)
                        continue;

                    i_video_stream = i_fmt_ctx->streams[i];
                    break;
                }

                ffmpeg.avformat_alloc_output_context2(&o_fmt_ctx, null, null, DestinationFile);

                /*
                * since all input files are supposed to be identical (framerate, dimension, color format, ...)
                * we can safely set output codec values from first input file
                */
                var o_video_stream = ffmpeg.avformat_new_stream(o_fmt_ctx, null);
                {
                    AVCodecContext* c;
                    c = o_video_stream->codec;
                    c->bit_rate = 400000;
                    c->codec_id = i_video_stream->codec->codec_id;
                    c->codec_type = i_video_stream->codec->codec_type;
                    c->time_base.num = i_video_stream->time_base.num;
                    c->time_base.den = i_video_stream->time_base.den;
                    //fprintf(stderr, "time_base.num = %d time_base.den = %d\n", c->time_base.num, c->time_base.den);
                    c->width = i_video_stream->codec->width;
                    c->height = i_video_stream->codec->height;
                    c->pix_fmt = i_video_stream->codec->pix_fmt;
                    //printf("%d %d %d", c->width, c->height, c->pix_fmt);
                    c->flags = i_video_stream->codec->flags;
                    c->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
                    c->me_range = i_video_stream->codec->me_range;
                    c->max_qdiff = i_video_stream->codec->max_qdiff;
                    c->qmin = i_video_stream->codec->qmin;
                    c->qmax = i_video_stream->codec->qmax;
                    c->qcompress = i_video_stream->codec->qcompress;
                }

                ffmpeg.avio_open(&o_fmt_ctx->pb, DestinationFile, ffmpeg.AVIO_FLAG_WRITE);
                ffmpeg.avformat_write_header(o_fmt_ctx, null);

                long last_pts = 0;
                long last_dts = 0;
                long pts = 0;
                long dts = 0;
                while (_canExecute(Rtsp))
                {
                    AVPacket i_pkt;
                    ffmpeg.av_init_packet(&i_pkt);
                    i_pkt.size = 0;
                    i_pkt.data = null;
                    if (ffmpeg.av_read_frame(i_fmt_ctx, &i_pkt) < 0)
                        break;
                    /*
                    * pts and dts should increase monotonically
                    * pts should be >= dts
                    */
                    i_pkt.flags |= ffmpeg.AV_PKT_FLAG_KEY;
                    pts = i_pkt.pts;
                    i_pkt.pts += last_pts;
                    dts = i_pkt.dts;
                    i_pkt.dts += last_dts;
                    i_pkt.stream_index = 0;

                    //printf("%lld %lld\n", i_pkt.pts, i_pkt.dts);
                    var num = 1;
                    //printf("frame %d\n", num++);
                    ffmpeg.av_interleaved_write_frame(o_fmt_ctx, &i_pkt);
                    //av_free_packet(&i_pkt);
                    //av_init_packet(&i_pkt);
                }

                last_dts += dts;
                last_pts += pts;
                ffmpeg.avformat_close_input(&i_fmt_ctx);
                ffmpeg.av_write_trailer(o_fmt_ctx);
                ffmpeg.avcodec_close(o_fmt_ctx->streams[0]->codec);
                ffmpeg.av_freep(&o_fmt_ctx->streams[0]->codec);
                ffmpeg.av_freep(&o_fmt_ctx->streams[0]);
                ffmpeg.avio_close(o_fmt_ctx->pb);
                ffmpeg.av_free(o_fmt_ctx);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new RtspExceptionEventArgs(Rtsp, ex));
                throw;
            }
        }

        private string EnsureMp4File(string fileName)
        {
            var file = $"{ Path.GetFileNameWithoutExtension(fileName)}.mp4";
            var dir = Path.GetDirectoryName(fileName);
            fileName = Path.Combine(dir, file);

            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                else if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new RtspExceptionEventArgs(Rtsp, ex));
                throw;
            }


            return fileName;
        }

        public void Stop()
        {
            _canExecute = _ => false;
        }
    }

    public class RtspExceptionEventArgs : EventArgs
    {
        public string Rtsp { get; set; }
        public Exception Exception { get; set; }
        public RtspExceptionEventArgs(string rtsp, Exception ex)
        {
            this.Rtsp = rtsp;
            this.Exception = ex;
        }
    }
}
