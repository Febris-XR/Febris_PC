// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;

namespace Febris.PCModuleManagerV3.Utilties
{
    class ModuleFileProcessing
    {
        private ILogger _log;
        private IConfiguration _config;
        private readonly JSONHandler _jSONHandler;
        private readonly PCFileManager _fileManager;
        

        public ModuleFileProcessing(ILogger log)
        {
            _log = log;
            _jSONHandler = new JSONHandler(_log);
            _fileManager = new PCFileManager(_log, _config);
            

        }


        #region zipped file system
        public bool FileUnzipper(string zipFile, Guid module, string linkName)
        {
            bool unzipped = false;
            //Process process = FebrisLocalLibrary.Service.ProgressBarService.StartProgressBar(zipFile, FebrisLocalLibrary.Enums.StatusType.Processing);
            try
            {
                string zipPath = zipFile;
                // Normalizes the path.
                //string extractPath = Path.GetFullPath(StaticDetails.ModulePath);
                string extractPath = Path.GetFullPath(PCFileSystem.ModulePath);
                string destinationPath = string.Empty;

                destinationPath = Path.GetFullPath(Path.Combine(extractPath, Path.GetFileNameWithoutExtension(zipFile)));

                ZipFile.ExtractToDirectory(zipPath, destinationPath);
                unzipped = true;

                //await RemoveOldEditions(StaticDetails.tempLinkName);
                Task.Run(() => FindSimulationApplication(destinationPath, module.ToString(), linkName)).Wait();
                //await FindSimulationApplication(destinationPath).Wait();

            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
            //FebrisLocalLibrary.Service.ProgressBarService.StopProgressBar(process);
            return unzipped;
        }

        public async Task<bool> FindSimulationApplication(string rootDirectory, string moduleName, string linkName)
        {
            //Process process = FebrisLocalLibrary.Service.ProgressBarService.StartProgressBar("Simulation", FebrisLocalLibrary.Enums.StatusType.Installing);           

            bool simulationFound = false;
            //bool rslt = false;
            //find enumerable list of directories
            //IEnumerable<string> folders = Directory.EnumerateDirectories(StaticDetails.zippedModulePath);
            //IEnumerable<string> files = Directory.EnumerateFiles(StaticDetails.zippedModulePath);
            try
            {
                string[] allfiles = Directory.GetFiles(rootDirectory, "*.exe", SearchOption.AllDirectories);

                foreach (string i in allfiles)
                {
                    // FIX (PC-B6): i is a full path from GetFiles, so it never equals the bare name. Compare the filename case-insensitively so the crash handler is actually excluded. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                    //if (i != "UnityCrashHandler64.exe")
                    if (Path.GetFileName(i).ToLower() != "unitycrashhandler64.exe")
                    {
                        moduleName = i;
                        simulationFound = true;
                        break;
                    }
                }
                //await CreateShortCut(StaticDetails.tempLinkName, StaticDetails.tempModuleName);
                Task.Run(() => CreateShortCut(linkName, moduleName)).Wait();
                //return rslt;
                //FebrisLocalLibrary.Service.ProgressBarService.StopProgressBar(process);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return simulationFound;
        }

        public bool CreateShortCut(string linkName, string targetFilePath)
        {
            bool shortCutCreated = false;
            try
            {
                //targetFilePath = Path.Combine(StaticDetails.ModulePath, targetFilePath);
                targetFilePath = Path.Combine(PCFileSystem.ModulePath, targetFilePath);

                //string shortcutLocation = Path.Combine(StaticDetails.ModuleLinkPath, linkName + ".lnk");
                string shortcutLocation = Path.Combine(PCFileSystem.ModuleLinkPath, linkName + ".lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                shortcut.TargetPath = targetFilePath;
                shortcut.Save();
                shortCutCreated = true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return shortCutCreated;
        }


        #endregion
    }
}
