// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Febris.PCScreenRecorderV3.Utilities
{
    class ScreenRecorderStaticDetails
    {
        //video vars
        //internal static Rectangle bounds=new Rectangle(50, 50, 200, 100);
        internal static Rectangle bounds = new Rectangle(0, 0, 1920, 1080);
        //File varibales
        internal static string audioName = "mic.wav"; // should not be needed
        internal static string videoName = "DefaultVideoName.mp4"; // should be comming in from a json input
        internal static string finalName = "DefaultFinalVideoName.mp4"; // or this should be the name...

        internal static string outputPath = System.IO.Path.Combine(PCFileSystem.RecordingsFilePath, finalName);
        internal static string tempPath = System.IO.Path.Combine(PCFileSystem.TempRecordingsFilePath, videoName);
        internal static int fileCount = 1;
        internal static List<string> inputImageSequence = new List<string>();
        internal static int frameRate = 20;
        internal static int TimerPeriod = 50;

        //ffmpeg encode settings. Replaced the OpenCvSharp VideoWriter, which wrote the
        //MP42 (Microsoft MPEG-4 v2) fourcc into a .mp4 container. That pairing is an
        //AVI-era codec in an MP4 box and many players, Android especially, reject it.
        //libx264 in yuv420p is the universally playable combination.
        //Explicit override. When set to a full path it wins over every probe below. Left empty so
        //ResolveFfmpegPath can find the copy bundled next to the executable, which is what an
        //installed launcher uses. Air-gapped sites are in scope, so ffmpeg is never downloaded.
        internal static string ffmpegPath = string.Empty;
        internal static string bundledFfmpegFolder = "ffmpeg";
        internal static string ffmpegExecutable = "ffmpeg.exe";
        internal static string frameListName = "frames.ffconcat";
        internal static string videoCodec = "libx264";
        internal static string encodePreset = "veryfast";
        internal static int encodeCrf = 23;


        internal class VideoVariables
        {
            public string VideoName { get; set; }
        }

        //Time variable;
        internal static Stopwatch watch = new Stopwatch();
        internal static Timer timer;// = new Timer(tmrRecord_Tick, null, 0, 100);

        //Audio variables:
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

        //process info

        //internal static Timer unityCheckTimer = new Timer(tmrRecord_Tick, null, 0, 1000);;
        internal static int simulationProcessId;
        internal static string simulationProcessName = string.Empty;
        internal static Timer processCheckTimer;
        internal static bool simulationIsRunning = false;
    }
}
