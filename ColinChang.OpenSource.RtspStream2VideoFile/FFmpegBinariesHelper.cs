using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ColinChang.OpenSource.RtspStream2VideoFile
{
    class FFmpegBinariesHelper
    {
        private const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

        internal static void RegisterFFmpegBinaries()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    RegisterLibrariesSearchPath(Path.Combine(Environment.CurrentDirectory, "FFmpeg", "win", "bin",
                        Environment.Is64BitProcess ? "x64" : "x86"));
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    RegisterLibrariesSearchPath(Path.Combine(Environment.CurrentDirectory, "FFmpeg", "mac", "bin"));
                    break;
            }
        }

        private static void RegisterLibrariesSearchPath(string path)
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    SetDllDirectory(path);
                    break;
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, path);
                    break;
            }
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}