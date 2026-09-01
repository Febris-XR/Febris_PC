// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.ModelLibrary.LauncherModels;
using Febris.SharedServices;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
//using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCStatementManagerV3.APIRequests
{
    class StatementRequest
    {
        public string _endpoint;
        private readonly TokenRequest _tokenHandler;
        private readonly IConfiguration _config;
        private readonly JSONHandler _jSONHandler;
        private ILogger _log;

        public StatementRequest(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            _jSONHandler = new JSONHandler(_log, _config);
            _tokenHandler = new TokenRequest(_log, _config);

            _endpoint = LocalHardwareStaticDetails.ApiUrl;
        }


        #region Requests

        private async Task<string> MakeGetRequest(string method, string dataPackage)
        {
            try
            {
                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Statement/" + method,
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
                    endPoint = endpoint + "Statement/" + method,
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

        #endregion

        /// <summary>
        /// Primary statement upload path. Targets the Phase 3 typed-DTO
        /// ingest endpoint <c>/api/Statement/Submit</c> which:
        /// <list type="bullet">
        ///   <item>Validates the payload against the xAPI 1.0.3 shape at
        ///     the binder (clear 400s for malformed JSON).</item>
        ///   <item>Captures the verbatim POST bytes and persists them to
        ///     the audit-trail directory keyed on the statement UUID.</item>
        /// </list>
        /// <para>
        /// Previously targeted the parameterless <c>/api/Statement/</c>
        /// route (typed <c>Statement</c> model bind). The fallback path
        /// <see cref="UploadStatementBackup"/> still hits
        /// <c>/api/Statement/Backup</c> (JObject, permissive) -- the
        /// retry chain in <c>StatementFolderChecker</c> means producers
        /// emitting non-spec-shaped JSON still land via the backup route.
        /// </para>
        /// <para>
        /// See <c>docs/INTERACTION_MAP.md</c> for the full client-to-API
        /// endpoint mapping.
        /// </para>
        /// </summary>
        public async Task<bool> UploadStatement(string input)
        {
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                dataPackage = input;
                method = "Submit";
                result = await MakePostRequest(method, dataPackage);
                StatementUploadResponseViewModel output = JsonConvert.DeserializeObject<StatementUploadResponseViewModel>(result);

                return output?.Success??false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
                //throw;
            }
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    throw;
            //}
            //throw new NotImplementedException();
        }
        public async Task<bool> UploadStatementBackup(string input)
        {
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                dataPackage = input;
                method = "Backup";
                result = await MakePostRequest(method, dataPackage);
                StatementUploadResponseViewModel output = JsonConvert.DeserializeObject<StatementUploadResponseViewModel>(result);
                return output?.Success ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // FIX (PC-B12): return false instead of rethrowing so one file's failure does not abort the StatementFolderChecker loop (mirrors UploadStatement). See docs/MODERNIZATION/PC_MODERNIZATION.md.
                //throw;
                return false;
            }
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    throw;
            //}
            //throw new NotImplementedException();
        }



        //public async Task<StatementInitalizationResponseViewModel> StatmentInitalizer()//HardwareUserViewModel selectedUser, Module module)
        //public async Task<StatementInitalizationResponseViewModel> StatmentInitalizer(HardwareUserViewModel selectedUser, Module module)
        //{
        //    StatementInitalizationResponseViewModel output = new StatementInitalizationResponseViewModel();
        //    string dataPackage = string.Empty;
        //    string method = string.Empty;
        //    string result = string.Empty;
        //    try
        //    {
        //        StatementInitalizationRequestViewModel statementRequest = new StatementInitalizationRequestViewModel()
        //        {
        //            ModuleId = module.UUID,
        //            UserId = selectedUser.UserId,
        //            ActorId = selectedUser.ActorId,
        //            IsTestUser = selectedUser.IsTestUser
        //        };

        //        dataPackage = JsonConvert.SerializeObject(statementRequest);
        //        method = "statementinitialization";

        //        result = await MakePostRequest(method, dataPackage);
        //        output = JsonConvert.DeserializeObject<StatementInitalizationResponseViewModel>(result);
        //        //LocalHardwareStaticDetails._hardwareInitializationResponse = output;
        //        //await ParseResponse();

        //        return output;

        //        //StatementInitializerGetViewModel statementVm = new StatementInitializerGetViewModel()
        //        //{
        //        //    Professional = selectedUser,
        //        //    Module = module
        //        //};
        //        // string statementInitializer = JsonConvert.SerializeObject(statementRequest);


        //        //Communication.FebrisRestClient febrisRestClientInitalization = new FebrisLocalLibrary.Communication.FebrisRestClient(_log)
        //        //{
        //        //    endPoint = FebrisLocalLibrary.SharedDetails.SharedDetails.StatementInitializer,
        //        //    authType = FebrisLocalLibrary.Communication.Authenticationtype.BearerToken,
        //        //    httpMethod = FebrisLocalLibrary.Communication.httpVerb.POST,
        //        //    authTech = FebrisLocalLibrary.Communication.AuthenticaitonTechnique.Token,
        //        //    postJSON = statementInitializer
        //        //};
        //        //populate Test list
        //        //string response = string.Empty;
        //        //response = febrisRestClientInitalization.MakeRequest().Result;
        //        //if (response == "Could Not Find Anything Here")
        //        //{
        //        //    response = febrisRestClientInitalization.MakeRequest().Result;
        //        //}
        //        //_jSONHandler.InitalizeStatement(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //        throw;
        //    }
        //}
    }
}
