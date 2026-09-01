// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.PCModuleManagerV3.APIRequests;
using Febris.PCModuleManagerV3.Utilties;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCModuleManagerV3.Services
{
    class ModuleFileChecker
    {
        private ILogger _log;
        private IConfiguration _config;
        private readonly JSONHandler _jSONHandler;
        private readonly PCFileManager _fileManager;
        private readonly ModuleRequest _moduleRequest;
        private readonly ModuleFileProcessing _moduleProcessing;


        public ModuleFileChecker(ILogger log)
        {
            _log = log;
            _jSONHandler = new JSONHandler(_log);
            _fileManager = new PCFileManager(_log, _config);
            _moduleRequest = new ModuleRequest(_log);
            _moduleProcessing = new ModuleFileProcessing(_log);

        }

        //*********************************************************************************************************
        //Todo: check each file and make sure each module is up to date with API Get
        //
        //*********************************************************************************************************
        public bool ModuleChecker()
        {
            bool hasModules = false;
            try
            {
                List<Guid> listOfSetModules = new List<Guid>();
                listOfSetModules = _moduleRequest.ModuleListRequest().Result;


                //FebrisLocalLibrary.Communication.FebrisRestClient request = new FebrisLocalLibrary.Communication.FebrisRestClient(_log)
                //{
                //    endPoint = FebrisLocalLibrary.SharedDetails.SharedDetails.ModuleCheckingUrl,
                //    authType = FebrisLocalLibrary.Communication.Authenticationtype.BearerToken,
                //    httpMethod = FebrisLocalLibrary.Communication.httpVerb.GET,
                //    authTech = FebrisLocalLibrary.Communication.AuthenticaitonTechnique.Token
                //};

                //string moduleResponse = string.Empty;
                //moduleResponse = request.MakeRequest().Result;

                //listOfModules = JSONHandler.DeserialiseModulesJSON(moduleResponse);                                
                //listOfModules = _jSONHandler.DeserialiseModulesJSON(moduleResponse);
                var task = Task.Run(() => CycleThroughModules(listOfSetModules));
                task.Wait();
                bool downloadRslt = task.Result;

            }
            catch (Exception e)
            {
                _log.LogInformation(e.Message);
            }
            return hasModules;
        }

        //*********************************************************************************************************
        //check each module matches db's modules
        //
        //*********************************************************************************************************
        private bool CycleThroughModules(List<Guid> moduleList)
        {
            bool downloaded = false;
            //check current modules installed
            //IEnumerable<string> moduleFolders = Directory.EnumerateFiles(StaticDetails.ModuleLinkPath);
            try
            {
                IEnumerable<string> moduleFolders = Directory.EnumerateFiles(PCFileSystem.ZippedModulePath);
                foreach (Guid module in moduleList)
                {
                    //string moduleNameWithoutExtention = Path.GetFileNameWithoutExtension(module);
                    //string tempFileName = moduleFolders.Where(i => i.Contains(Path.GetFileName(moduleNameWithoutExtention))).FirstOrDefault();
                    //if (tempFileName == null)
                    // NOTE (PC-B7): Module cache existence is checked by zip filename alone (no version or hash), so a network drop mid-download leaves a truncated zip that passes this check and is never retried. Deferred per do-not-change-functionality: a robust fix needs a durable upload queue with persistent state, hash verification, atomic rename, and backoff (PC-M7, Tier 4). See docs/MODERNIZATION/PC_MODERNIZATION.md.
                    bool fileExistsInSystem = moduleFolders.Where(i => i.Contains(Path.GetFileName(module.ToString()))).Any();
                    if (!fileExistsInSystem)
                    {
                        //get module name
                        //StaticDetails.tempModuleName = module;
                        //set test name without any information

                        //string simName = FileManager.CreateSimulationName(module);
                        //string simName = _fileManager.CreateSimulationName(module);

                        //This probably needs to be reworked
                        //for (int i = 0; i<10; i++)
                        //{
                        //can use the normal module downloader here ************************************************

                        bool downloadRslt = _moduleRequest.DownloadModule(module).Result;
                        if (downloadRslt)
                        {
                            string fileName = module.ToString() + ".zip";
                            string newFileNameandPath = Path.Combine(PCFileSystem.ZippedModulePath, fileName);

                            Task.Run(() => _moduleProcessing.FileUnzipper(newFileNameandPath, module, module.ToString())).Wait();
                        }
                        downloaded = downloadRslt;
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogWarning(e.Message);
            }
            return downloaded;
        }

        //*********************************************************************************************************
        //Todo: download modules that are not uptodate with API GET
        //
        //May be able to use the FebrisRestClient handler - does not copy only returns something
        //*********************************************************************************************************
        //public bool DownloadModule(string FileName)
        //{
        //    bool rslt = true;
        //    try
        //    {
        //        //FebrisLocalLibrary.Communication.FebrisRestClient febrisRestClient = new FebrisLocalLibrary.Communication.FebrisRestClient();                
        //        //rslt = febrisRestClient.ModuleDownloader(FileName).Wait();
        //        var task = Task.Run(() => _moduleRequest.DownloadModule(FileName));
        //        task.Wait();
        //        rslt = task.Result;
        //    }
        //    catch (Exception e)
        //    {
        //        _log.LogError(e.Message);
        //    }
        //    return rslt;
        //}
    }
}
