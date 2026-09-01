// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.SharedServices.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCScreenRecorderV3.Utilities
{
    class ArgumentRequestHandler
    {
        private Serilog.ILogger _log;

        public ArgumentRequestHandler(Serilog.ILogger log)
        {
            _log = log;
        }


        public bool ArgumentHandler(string[] args)
        {
            bool setup = false;
            _log.Information("Received the following arguments:\n");
            //Console.WriteLine("Received the following arguments:\n");            
            string jsonString = string.Empty;
            //int unityId = 0;
            string deserializedVideoDataJson = string.Empty;
            foreach (string arg in args)
            {
                //Console.WriteLine(arg + "\n");
                _log.Information(arg + "\n");
                //if (arg.StartsWith("-videoData="))
                if (arg.StartsWith(LauncherSharedDetails.VideoDataPreface))
                {
                    jsonString = arg.Substring(LauncherSharedDetails.VideoDataPrefaceLength);
                    //Console.WriteLine("Json SubString: "+jsonString+"\n");
                    if (jsonString != null && jsonString.Length > 2)
                    {
                        try
                        {
                            deserializedVideoDataJson = JsonConvert.DeserializeObject<string>(jsonString);
                            ScreenRecorderStaticDetails.videoName = deserializedVideoDataJson;
                            ScreenRecorderStaticDetails.finalName = deserializedVideoDataJson;
                        }
                        catch
                        {
                            ScreenRecorderStaticDetails.videoName = jsonString + ".mp4";
                            ScreenRecorderStaticDetails.finalName = jsonString + ".mp4";
                        }
                        ScreenRecorderStaticDetails.outputPath = System.IO.Path.Combine(PCFileSystem.RecordingsFilePath, ScreenRecorderStaticDetails.videoName);
                        ScreenRecorderStaticDetails.tempPath = System.IO.Path.Combine(PCFileSystem.TempRecordingsFilePath, ScreenRecorderStaticDetails.videoName);
                    }
                }
            }
            return setup;
        }


    }
}
