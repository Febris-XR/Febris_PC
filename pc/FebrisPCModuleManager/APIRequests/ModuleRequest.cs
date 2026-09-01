// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.EnumLibrary.LauncherEnums;
using Febris.SharedServices;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCModuleManagerV3.APIRequests
{
    public class ModuleRequest
    {
        public string _endpoint;
        private ILogger _log;
        private IConfiguration _config;
        private readonly APIRequestFactory _febrisRestClient;
        private readonly JSONHandler _jSONHandler;
        private readonly PCFileManager _fileManager;
        private readonly TokenRequest _tokenHandler;


        public ModuleRequest(ILogger log)
        {
            _log = log;
            //_jSONHandler = new SharedServices.Launcher.JSONHandler(_log, _config);
            _tokenHandler = new TokenRequest(_log, _config);
            _endpoint = LocalHardwareStaticDetails.ApiUrl;
            //_febrisRestClient = new FebrisLocalLibrary.Communication.FebrisRestClient(_log);
            _jSONHandler = new JSONHandler(_log);
            _fileManager = new PCFileManager(_log, _config);
            //_febrisRestClient = new APIRequestFactory();

        }




        #region Requests

        private async Task<string> MakeGetRequest(string method, string dataPackage)
        {
            try
            {
                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Module/" + method,
                    httpMethod = httpVerb.GET,
                    authTech = AuthenticaitonTechnique.Token,
                    authType = Authenticationtype.BearerToken,
                    postJSON = dataPackage ?? string.Empty,
                    //contentType = "",
                    token = LocalHardwareStaticDetails._hardwareAuthenticationResponse?.JwtToken ?? string.Empty

                };
                string response = string.Empty;
                HttpStatusCode status;
                (response, status) = await request.MakeStringRequest();
                if (status != HttpStatusCode.OK)
                {
                    //await _tokenHandler.Authenticate();
                    bool complete = await _tokenHandler.RenewToken();
                    if (complete)
                    {
                        request.token = LocalHardwareStaticDetails._hardwareAuthenticationResponse.JwtToken;
                        //request.token = StaticDetails.LicenseAuthenticateResponse.JwtToken;
                        (response, status) = await request.MakeStringRequest();
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private async Task<string> MakePostRequest(string method, string dataPackage)
        {
            try
            {
                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Module/" + method,
                    httpMethod = httpVerb.POST,
                    authTech = AuthenticaitonTechnique.Token,
                    authType = Authenticationtype.BearerToken,
                    postJSON = dataPackage ?? string.Empty,
                    token = LocalHardwareStaticDetails._hardwareAuthenticationResponse?.JwtToken ?? string.Empty
                };
                string response = string.Empty;
                HttpStatusCode status;
                (response, status) = await request.MakeStringRequest();
                if (status != HttpStatusCode.OK)
                {
                    //await _tokenHandler.Authenticate();
                    bool complete = await _tokenHandler.RenewToken();
                    if (complete)
                    {
                        request.token = LocalHardwareStaticDetails._hardwareAuthenticationResponse.JwtToken;
                        (response, status) = await request.MakeStringRequest();
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<byte[]> MakeDownloadRequest(string method, string dataPackage)
        {
            try
            {
                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Module/" + method,
                    httpMethod = httpVerb.GET,
                    authTech = AuthenticaitonTechnique.Token,
                    authType = Authenticationtype.BearerToken,
                    postJSON = dataPackage ?? string.Empty,
                    token = LocalHardwareStaticDetails._hardwareAuthenticationResponse?.JwtToken ?? string.Empty
                };
                byte[] response = { };
                HttpStatusCode status;
                (response, status) = await request.MakeByteArrayRequest();
                if (status != HttpStatusCode.OK)
                {
                    //await _tokenHandler.Authenticate();
                    bool complete = await _tokenHandler.RenewToken();
                    if (complete)
                    {
                        request.token = LocalHardwareStaticDetails._hardwareAuthenticationResponse.JwtToken;
                        (response, status) = await request.MakeByteArrayRequest();
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
                //return ex.Message;
            }
        }
        #endregion

        //*********************************************************************************************************
        //Todo: check each file and make sure each module is up to date with API Get
        //
        //*********************************************************************************************************
        public async Task<List<Guid>> ModuleListRequest()
        {
            //bool hasModules = false;
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                List<Guid> output = new List<Guid>();
                method = "getmoduleidlist";
                result = await MakeGetRequest(method, dataPackage);
                output = JsonConvert.DeserializeObject<List<Guid>>(result);
                //LocalHardwareStaticDetails._hardwareInitializationResponse = output;
                //return output;


                //FebrisLocalLibrary.Communication.FebrisRestClient request = new FebrisLocalLibrary.Communication.FebrisRestClient(_log)
                //{
                //    endPoint = FebrisLocalLibrary.SharedDetails.SharedDetails.ModuleCheckingUrl,
                //    authType = FebrisLocalLibrary.Communication.Authenticationtype.BearerToken,
                //    httpMethod = FebrisLocalLibrary.Communication.httpVerb.GET,
                //    authTech = FebrisLocalLibrary.Communication.AuthenticaitonTechnique.Token
                //};

                //string moduleResponse = string.Empty;
                //moduleResponse = request.MakeRequest().Result;

                ////listOfModules = JSONHandler.DeserialiseModulesJSON(moduleResponse);                                
                //listOfModules = _jSONHandler.DeserialiseModulesJSON(moduleResponse);
                //var task = Task.Run(() => CycleThroughModules(listOfModules));
                //task.Wait();
                //bool downloadRslt = task.Result;


                return output;

            }
            catch (Exception e)
            {
                _log.LogInformation(e.Message);
                throw;
            }            
        }

        //*********************************************************************************************************
        //Todo: check each file and make sure each module is up to date with API Get
        //
        //*********************************************************************************************************
        public async Task<bool> DownloadModule(Guid input)
        {
            //bool hasModules = false;
            bool complete =false;
            string dataPackage = string.Empty;
            string method = string.Empty;
            byte[] result = default;
            try
            {
                List<Guid> output = new List<Guid>();
                method = "download/"+input.ToString();
                //method = "download";
                result = await MakeDownloadRequest(method, dataPackage);

                if (result == default)
                {
                    return complete;
                }

                //stream result
                using (MemoryStream inputStream = new MemoryStream(result))// result.Content.ReadAsStreamAsync())// new MemorySteam(result)await result.ReadAsStreamAsync())
                {
                    string fileToWriteTo = Path.Combine(PCFileSystem.ZippedModulePath, input.ToString()+".zip");
                    //string fileToWriteTo = Path.Combine(PCFileSystem.ZippedModulePath, input.ToString());
                    //write to a file
                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        #region test 3
                        //Process process = SharedServices.Launcher.ProgressBarService.StartProgressBar(input.ToString(), StatusType.Downloading);
                        await inputStream.CopyToAsync(streamToWriteTo);
                        //ProgressBarService.StopProgressBar(process);
                        #endregion
                        complete = true;
                    }
                    //Send to unpacking
                }
                return complete;
            }
            catch (Exception e)
            {
                _log.LogInformation(e.Message);
                throw;
            }
        }


    }
}
