// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary.LauncherEnums;
using Febris.PCScreenRecorderV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Febris.PCScreenRecorderV3.CoreOperations
{
    class ScreenRecorder
    {
        //private Microsoft.Extensions.Logging.ILogger _log;
        //private IConfiguration _config;
        private Serilog.ILogger _log;

        //public ScreenRecorder(Rectangle b, string outPath)
        //{
        //    CreateTempFolder(StaticDetails.tempPath);

        //    StaticDetails.bounds = b;
        //    StaticDetails.outputPath = outPath;
        //}

        //public ScreenRecorder(Microsoft.Extensions.Logging.ILogger log, IConfiguration config)
        //{
        //    _log = log;
        //    _config = config;
        //    //CreateTempFolder(StaticDetails.tempPath);

        //    //StaticDetails.bounds = b;
        //    //StaticDetails.outputPath = outPath;
        //}

        public ScreenRecorder(Serilog.ILogger log)
        {
            _log = log;
        }

        public void CreateTempFolder(string name)
        {
            try
            {
                string pathName = $"{name}";//make sure to put this on the right path
                Directory.CreateDirectory(ScreenRecorderStaticDetails.tempPath);
                ScreenRecorderStaticDetails.tempPath = pathName;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        private void DeletePath(string targetDir)
        {
            try
            {
                string[] files = Directory.GetFiles(targetDir);
                string[] dirs = Directory.GetDirectories(targetDir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeletePath(dir);
                }
                Directory.Delete(targetDir, false);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        private void DeleteFileExcept(string targetFile, string excFile)
        {
            try
            {
                string[] files = Directory.GetFiles(targetFile);

                foreach (string file in files)
                {
                    if (file != excFile)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        public void CleanUp()
        {
            try
            {
                if (Directory.Exists(ScreenRecorderStaticDetails.tempPath))
                {
                    DeletePath(ScreenRecorderStaticDetails.tempPath);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        public string GetElapsed()
        {
            try
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", ScreenRecorderStaticDetails.watch.Elapsed.Hours, ScreenRecorderStaticDetails.watch.Elapsed.Minutes, ScreenRecorderStaticDetails.watch.Elapsed.Seconds);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return string.Empty;
            }
        }

        public void RecordFrame()
        {
            try
            {
                ScreenRecorderStaticDetails.watch.Start();
                //This is working to get bitmaps
                using (Bitmap bitmap = new Bitmap(ScreenRecorderStaticDetails.bounds.Width, ScreenRecorderStaticDetails.bounds.Height))
                {
                    //Console.WriteLine("Getting bitmap \n");
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(new System.Drawing.Point(ScreenRecorderStaticDetails.bounds.Left, ScreenRecorderStaticDetails.bounds.Top), System.Drawing.Point.Empty, ScreenRecorderStaticDetails.bounds.Size);
                        //g.Flush();
                    }
                    string name = ScreenRecorderStaticDetails.tempPath + @"\" + ScreenRecorderStaticDetails.videoName + "-" + ScreenRecorderStaticDetails.fileCount + ".png";//change this
                                                                                                                                   //Console.WriteLine("writing:"+name+"\n");
                    bitmap.Save(name, ImageFormat.Png);
                    ScreenRecorderStaticDetails.inputImageSequence.Add(name);
                    //Console.WriteLine("Added to Image Sequence:" + StaticDetails.inputImageSequence.ToString() + "\n");
                    ScreenRecorderStaticDetails.fileCount++;
                    //Console.WriteLine("fileCount:" + StaticDetails.fileCount.ToString() + "\n");
                    bitmap.Dispose();
                    //}
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }
        //this should not be needed
        public void RecordAudio()
        {
            try
            {
                ScreenRecorderStaticDetails.NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
                ScreenRecorderStaticDetails.NativeMethods.record("record recsound", "", 0, 0);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Returns the captured frames in capture order, read from disk rather than from
        /// ScreenRecorderStaticDetails.inputImageSequence. Per PC-B11 the capture timer has no
        /// reentrancy guard, so that List and fileCount are written from overlapping threads and
        /// the List can silently lose entries. The files on disk are the ground truth.
        /// Frames are named "{videoName}-{n}.png", so they sort numerically, not lexically.
        /// </summary>
        private List<string> GetCapturedFrames()
        {
            string[] files = Directory.GetFiles(ScreenRecorderStaticDetails.tempPath, "*.png");
            return files
                .Select(f => new { Path = f, Index = ParseFrameIndex(f) })
                .Where(f => f.Index >= 0)
                .OrderBy(f => f.Index)
                .Select(f => f.Path)
                .ToList();
        }

        private static int ParseFrameIndex(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            int dash = name.LastIndexOf('-');
            if (dash < 0 || dash == name.Length - 1)
            {
                return -1;
            }
            return int.TryParse(name.Substring(dash + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int index)
                ? index
                : -1;
        }

        /// <summary>
        /// Writes an ffconcat list for the frame sequence. The concat demuxer is used instead of an
        /// image2 "%d" pattern because it tolerates gaps in the numbering, which the unguarded
        /// capture timer can produce. The final entry is repeated because the concat demuxer drops
        /// the trailing frame otherwise.
        /// </summary>
        private string WriteFrameList(List<string> frames, double frameDurationSeconds)
        {
            string listPath = Path.Combine(ScreenRecorderStaticDetails.tempPath, ScreenRecorderStaticDetails.frameListName);
            StringBuilder list = new StringBuilder();
            list.Append("ffconcat version 1.0\n");
            foreach (string frame in frames)
            {
                list.Append("file '").Append(ToConcatPath(frame)).Append("'\n");
                list.Append("duration ").Append(frameDurationSeconds.ToString("0.000000", CultureInfo.InvariantCulture)).Append('\n');
            }
            list.Append("file '").Append(ToConcatPath(frames[frames.Count - 1])).Append("'\n");
            File.WriteAllText(listPath, list.ToString());
            return listPath;
        }

        //Forward slashes avoid backslash escaping inside the concat quoting, and ffmpeg accepts
        //them on Windows. A literal quote in a path is escaped the way the demuxer expects.
        private static string ToConcatPath(string path)
        {
            return path.Replace("\\", "/").Replace("'", @"'\''");
        }

        private void SaveVideo(int width, int height, int frameRate)
        {
            try
            {
                List<string> frames = GetCapturedFrames();
                if (frames.Count == 0)
                {
                    _log.Warning("no captured frames were found, skipping encode");
                    return;
                }

                //Preserves the previous half-size output. libx264 with yuv420p needs even
                //dimensions, so each axis is rounded down to an even number.
                int scaledWidth = (width / 2) - ((width / 2) % 2);
                int scaledHeight = (height / 2) - ((height / 2) % 2);
                if (scaledWidth < 2 || scaledHeight < 2)
                {
                    _log.Error($"capture bounds {width}x{height} are too small to encode");
                    return;
                }

                int rate = frameRate > 0 ? frameRate : 20;
                string listPath = WriteFrameList(frames, 1.0 / rate);

                //The old VideoWriter failed silently when the recordings folder was absent.
                string outputDirectory = Path.GetDirectoryName(ScreenRecorderStaticDetails.outputPath);
                if (!string.IsNullOrEmpty(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                string arguments =
                    $"-hide_banner -loglevel error -f concat -safe 0 -i \"{listPath}\" " +
                    $"-vf scale={scaledWidth}:{scaledHeight} -r {rate} " +
                    $"-c:v {ScreenRecorderStaticDetails.videoCodec} -preset {ScreenRecorderStaticDetails.encodePreset} " +
                    $"-crf {ScreenRecorderStaticDetails.encodeCrf} -pix_fmt yuv420p -movflags +faststart " +
                    $"-y \"{ScreenRecorderStaticDetails.outputPath}\"";

                _log.Information($"encoding {frames.Count} frames to {ScreenRecorderStaticDetails.outputPath}");
                if (RunFfmpeg(arguments))
                {
                    _log.Information("encode finished");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Finds ffmpeg. The bundled copy beside the executable is preferred so an installed
        /// launcher works with nothing on the machine, which also covers air-gapped sites.
        /// PATH is only a developer-convenience fallback. Returns null when nothing is found, so
        /// the caller can report a missing prerequisite instead of failing obscurely.
        /// </summary>
        private string ResolveFfmpegPath()
        {
            if (!string.IsNullOrWhiteSpace(ScreenRecorderStaticDetails.ffmpegPath))
            {
                if (File.Exists(ScreenRecorderStaticDetails.ffmpegPath))
                {
                    return ScreenRecorderStaticDetails.ffmpegPath;
                }
                _log.Error($"configured ffmpegPath '{ScreenRecorderStaticDetails.ffmpegPath}' does not exist, falling back to the bundled copy");
            }

            string baseDirectory = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDirectory, ScreenRecorderStaticDetails.ffmpegExecutable),
                Path.Combine(baseDirectory, ScreenRecorderStaticDetails.bundledFfmpegFolder, ScreenRecorderStaticDetails.ffmpegExecutable)
            };

            foreach (string candidate in candidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            //Developer fallback only. An installed launcher must not depend on this.
            if (ResolveFromPath(ScreenRecorderStaticDetails.ffmpegExecutable, out string onPath))
            {
                _log.Information($"using ffmpeg from PATH at '{onPath}'. An installed build should ship its own copy beside the executable");
                return onPath;
            }

            return null;
        }

        private static bool ResolveFromPath(string executable, out string resolved)
        {
            resolved = null;
            string pathVariable = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVariable))
            {
                return false;
            }
            foreach (string directory in pathVariable.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(directory))
                {
                    continue;
                }
                try
                {
                    string candidate = Path.Combine(directory.Trim(), executable);
                    if (File.Exists(candidate))
                    {
                        resolved = candidate;
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    //A malformed PATH entry must not take the recorder down.
                }
            }
            return false;
        }

        /// <summary>
        /// Runs ffmpeg and waits for it. Invokes the executable directly rather than through
        /// "cmd.exe /c" so there is no shell quoting layer. stderr is captured because ffmpeg
        /// writes its diagnostics there even on success.
        /// </summary>
        private bool RunFfmpeg(string arguments)
        {
            string executablePath = ResolveFfmpegPath();
            if (executablePath == null)
            {
                _log.Error(
                    $"ffmpeg was not found, so the recording cannot be encoded and the captured frames are lost. " +
                    $"Expected it beside the executable at '{Path.Combine(AppContext.BaseDirectory, ScreenRecorderStaticDetails.ffmpegExecutable)}', " +
                    $"in a '{ScreenRecorderStaticDetails.bundledFfmpegFolder}' subfolder, on PATH, or set ScreenRecorderStaticDetails.ffmpegPath. " +
                    $"An installed launcher is expected to ship its own copy.");
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    FileName = executablePath,
                    Arguments = arguments
                };

                using (Process ffmpeg = Process.Start(startInfo))
                {
                    string errorOutput = ffmpeg.StandardError.ReadToEnd();
                    ffmpeg.WaitForExit();

                    if (ffmpeg.ExitCode != 0)
                    {
                        _log.Error($"ffmpeg exited with {ffmpeg.ExitCode}: {errorOutput}");
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(errorOutput))
                    {
                        _log.Information($"ffmpeg: {errorOutput.Trim()}");
                    }
                    return true;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _log.Error($"could not start ffmpeg at '{executablePath}'. {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return false;
            }
        }

        private void SaveAudio()
        {
            try
            {
                string audioPath = "save recsound " + ScreenRecorderStaticDetails.outputPath + "//" + ScreenRecorderStaticDetails.audioName;
                ScreenRecorderStaticDetails.NativeMethods.record(audioPath, "", 0, 0);
                ScreenRecorderStaticDetails.NativeMethods.record("close recsound", "", 0, 0);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        private void CombineVideoAndAudio(string video, string audio)
        {
            try
            {
                string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {ScreenRecorderStaticDetails.finalName} ";
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    WorkingDirectory = ScreenRecorderStaticDetails.outputPath,
                    Arguments = command
                };

                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        public bool Stop()
        {
            bool stopped = false;
            try
            {
                Console.WriteLine("ScreenRecorder.Stop has been hit");

                ScreenRecorderStaticDetails.watch.Stop();
                int width = ScreenRecorderStaticDetails.bounds.Width;
                int height = ScreenRecorderStaticDetails.bounds.Height;
                int frameRate = ScreenRecorderStaticDetails.frameRate;

                //SaveAudio();
                Process process = ProgressBarService.StartProgressBar("Video", StatusType.Processing);
                SaveVideo(width, height, frameRate);

                //CombineVideoAndAudio(videoName, audioName);

                //this is for getting rid of the temp path
                DeletePath(ScreenRecorderStaticDetails.tempPath);
                ProgressBarService.StopProgressBar(process);

                //DeleteFileExcept(outputPath, outputPath + @"\" + videoName);}catch(Exception ex)
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return stopped;
        }
    }
}
