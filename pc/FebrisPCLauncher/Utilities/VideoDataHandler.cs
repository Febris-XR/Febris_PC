// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.Models.XApiModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Febris.PCLauncherV3.Utilites
{
    public class VideoDataHandler
    {
        private readonly IConfiguration _config;
        private ILogger _log;

        public VideoDataHandler(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public (bool videoNeeded, string videoName) VideoNeeded(JToken attachmentList)
        {
            try
            {
                bool videoNeeded = false;
                string videoName = string.Empty;
                foreach (var item in attachmentList)
                {
                    string contentType = (string)item["ContentType"].ToString().Replace("\r\n", string.Empty);
                    if (contentType == "video/mp4")
                    {
                        videoNeeded = true;
                        videoName = GetVideoName((string)item["Display"].ToString());
                    }
                }
                return (videoNeeded, videoName);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                return (false, null);
            }
        }

        public (bool videoNeeded, string videoName) VideoNeeded(List<Attachment> attachmentList)
        {
            try
            {
                bool videoNeeded = false;
                string videoName = string.Empty;
                foreach (var item in attachmentList)
                {
                    if(item.ContentType.ToLower()== "video/mp4")
                    {
                        videoNeeded = true;
                        videoName = GetVideoName(item.Display);
                    }
                    //string contentType = (string)item["ContentType"].ToString().Replace("\r\n", string.Empty);
                    //if (contentType == "video/mp4")
                    //{
                    //    videoNeeded = true;
                    //    videoName = GetVideoName((string)item["Display"].ToString());
                    //}
                }
                return (videoNeeded, videoName);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                return (false, null);
            }
        }

        /// <summary>
        /// Resolves the video name from an xAPI Language Map. Attachment.Display became a
        /// Dictionary&lt;string,string&gt; in the Option-B typing uplift, so the name is the map value.
        /// The string overload below parsed that same value out of serialized JSON and is kept for
        /// the legacy JToken path.
        /// </summary>
        private string GetVideoName(Dictionary<string, string> display)
        {
            try
            {
                string videoName = display?.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                if (string.IsNullOrWhiteSpace(videoName))
                {
                    throw new InvalidOperationException("attachment display carried no language-map value");
                }
                return videoName.Replace("}", string.Empty).Replace(" ", string.Empty).Replace(@"\r\n", string.Empty).Replace("\"", string.Empty).Trim();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                Guid fallbackName = Guid.NewGuid();
                _log.LogError("video has been named: " + fallbackName.ToString());
                return fallbackName.ToString();
            }
        }

        private string GetVideoName(string display)
        {
            try
            {
                string videoName = string.Empty;
                videoName = display.Split(":")[1];
                videoName = videoName.Replace("}", string.Empty).Replace(" ", string.Empty).Replace(@"\r\n", string.Empty).Replace("\"", string.Empty).Trim();
                return videoName;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                Guid fallbackName = Guid.NewGuid();
                _log.LogError("video has been named: " + fallbackName.ToString());
                return fallbackName.ToString();
            }
        }
    }
}
