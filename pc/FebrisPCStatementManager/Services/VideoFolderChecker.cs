// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary.LauncherEnums;
using Febris.ModelLibrary.LauncherModels;
using Febris.PCStatementManagerV3.APIRequests;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Febris.PCStatementManagerV3.Services
{
    class VideoFolderChecker
    {
        private ILogger _log;
        private IConfiguration _config;
        private readonly JSONHandler _jSONHandler;
        private readonly PCFileManager _fileManager;
        //private readonly VideoUploadRequest _videoUploadRequest;
        private readonly VideoFileProcessing _videoProcessing;

        public VideoFolderChecker(ILogger log)
        {
            _log = log;
            _videoProcessing = new VideoFileProcessing(_log);
            _fileManager = new PCFileManager(_log,_config);
        }

        public object ProcessFiles()
        {
            bool output = false;
            try
            {
                var unsentRecordingList = _fileManager.GetDirectoryContentNames(PCFileSystem.RecordingsFilePath);

                if (unsentRecordingList.Count() > 0)
                {
                    foreach (var i in unsentRecordingList)
                    {
                        if (CheckIfTypeIsMP4(i))
                        {
                            bool success = _videoProcessing.ProcessVideoFile(i);                            
                        }

                        ////gather file content
                        //string tempFile = _fileManager.GetFileContent(PCFileSystem.StatementPath, i);
                        ////send it
                        //bool sendComplete = _videoUploadRequest.UploadStatement(tempFile).Result;
                        //if (!sendComplete)
                        //{
                        //    sendComplete = _videoUploadRequest.UploadStatementBackup(tempFile).Result;
                        //}


                        //if (sendComplete)
                        //{
                        //    _fileManager.MoveStatementFileToSent(i);
                        //}
                    }

                }

            }
            catch (Exception e)
            {
                _log.LogInformation(e.Message);
            }
            return output;
        }

        //private bool MoveAndZipFile(string i)
        //{
        //    throw new NotImplementedException();
        //}

        //private bool ProcessVideoFile(string fileName)
        //{
        //    string filePath = PCFileSystem.RecordingsFilePath;
        //    Process process = ProgressBarService.StartProgressBar(fileName, StatusType.Uploading);
        //    bool rslt = false;
        //    try
        //    {

        //        //int i = 0;
        //        VideoFile videoFile = new VideoFile()
        //        {
        //            FileName = fileName,
        //            //TempFolder = Path.Combine(StaticDetails.SplitFilePath, fileName),
        //            TempFolder = Path.Combine(PCFileSystem.SplitFilePath, fileName),
        //            MaxFileSizeMB = 5,
        //            FileParts = new List<string>()
        //        };

        //        bool splitFile = SplitFile(videoFile, filePath);
        //        if (splitFile == true)
        //        {
        //            Console.WriteLine(fileName + " File split successful");
        //        }

        //        foreach (string File in videoFile.FileParts)
        //        {
        //            bool uploaded = false;
        //            uploaded = _videoUploadRequest.UploadFile(File);
        //            //bool uploaded = UploadFile(File);
        //            if (uploaded == true)
        //            {
        //                Console.WriteLine(File + " has been uploaded");
        //                rslt = true;
        //            }
        //            else
        //            {
        //                Console.WriteLine("upload did not work");
        //                rslt = false;
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //    }
        //    ProgressBarService.StopProgressBar(process);

        //    return rslt;
        //}
        //public bool SplitFile(VideoFile videoFile, string FilePath)
        //{
        //    try
        //    {
        //        bool rslt = false;
        //        string BaseFileName = Path.GetFileName(FilePath);
        //        // set the size of file chunk we are going to split into  
        //        int BufferChunkSize = videoFile.MaxFileSizeMB * (1024 * 1024);
        //        // set a buffer size and an array to store the buffer data as we read it  
        //        const int READBUFFER_SIZE = 1024;
        //        byte[] FSBuffer = new byte[READBUFFER_SIZE];
        //        // open the file to read it into chunks  
        //        using (FileStream FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))//sometimes access is denied to local folders
        //        {
        //            // calculate the number of files that will be created  
        //            int TotalFileParts = 0;
        //            if (FS.Length < BufferChunkSize)
        //            {
        //                TotalFileParts = 1;
        //            }
        //            else
        //            {
        //                float PreciseFileParts = ((float)FS.Length / (float)BufferChunkSize);
        //                TotalFileParts = (int)Math.Ceiling(PreciseFileParts);
        //            }

        //            int FilePartCount = 0;
        //            // scan through the file, and each time we get enough data to fill a chunk, write out that file  
        //            while (FS.Position < FS.Length)
        //            {
        //                string FilePartName = String.Format("{0}.part_{1}.{2}",
        //                BaseFileName,
        //                (FilePartCount + 1).ToString(),
        //                TotalFileParts.ToString());

        //                //FilePartName = Path.Combine(StaticDetails.SplitFilePath, FilePartName);
        //                FilePartName = Path.Combine(PCFileSystem.SplitFilePath, FilePartName);
        //                videoFile.FileParts.Add(FilePartName);
        //                using (FileStream FilePart = new FileStream(FilePartName, FileMode.Create))
        //                {
        //                    int bytesRemaining = BufferChunkSize;
        //                    int bytesRead = 0;
        //                    while (bytesRemaining > 0 && (bytesRead = FS.Read(FSBuffer, 0,
        //                     Math.Min(bytesRemaining, READBUFFER_SIZE))) > 0)
        //                    {
        //                        FilePart.Write(FSBuffer, 0, bytesRead);
        //                        bytesRemaining -= bytesRead;
        //                    }
        //                }
        //                // file written, loop for next chunk  
        //                FilePartCount++;
        //            }
        //            rslt = true;
        //        }
        //        return rslt;
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //        return false;
        //    }

        //}

        public bool CheckIfTypeIsMP4(string file)
        {
            bool isVideo = false;
            string extension = Path.GetExtension(file);
            if (extension.ToLower() == ".mp4")
            {
                isVideo = true;
            }
            else
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    try
                    {
                        Directory.Delete(file, true);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex.Message);
                    }
                }
            }
            return isVideo;
        }
    }
}
