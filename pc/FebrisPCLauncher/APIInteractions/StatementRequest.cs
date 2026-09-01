// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.SharedServices;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCLauncherV3.APIInteractions
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
                    await _tokenHandler.Authenticate();
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
                    await _tokenHandler.Authenticate();
                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        //public async Task<StatementInitalizationResponseViewModel> StatmentInitalizer()//HardwareUserViewModel selectedUser, Module module)
        public async Task<StatementInitalizationResponseViewModel> StatmentInitalizer(HardwareUserViewModel selectedUser, Module module)
        {
            StatementInitalizationResponseViewModel output = new StatementInitalizationResponseViewModel();
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                StatementInitalizationRequestViewModel statementRequest = new StatementInitalizationRequestViewModel()
                {
                    ModuleId=module.UUID,
                    UserId = selectedUser.UserId,
                    ActorId = selectedUser.ActorId,
                    IsTestUser = selectedUser.IsTestUser
                };
                
                dataPackage = JsonConvert.SerializeObject(statementRequest);
                method = "statementinitialization";

                result = await MakePostRequest(method, dataPackage);
                output = JsonConvert.DeserializeObject<StatementInitalizationResponseViewModel>(result);
                //LocalHardwareStaticDetails._hardwareInitializationResponse = output;
                //await ParseResponse();

                return output;

                //StatementInitializerGetViewModel statementVm = new StatementInitializerGetViewModel()
                //{
                //    Professional = selectedUser,
                //    Module = module
                //};
                // string statementInitializer = JsonConvert.SerializeObject(statementRequest);


                //Communication.FebrisRestClient febrisRestClientInitalization = new FebrisLocalLibrary.Communication.FebrisRestClient(_log)
                //{
                //    endPoint = FebrisLocalLibrary.SharedDetails.SharedDetails.StatementInitializer,
                //    authType = FebrisLocalLibrary.Communication.Authenticationtype.BearerToken,
                //    httpMethod = FebrisLocalLibrary.Communication.httpVerb.POST,
                //    authTech = FebrisLocalLibrary.Communication.AuthenticaitonTechnique.Token,
                //    postJSON = statementInitializer
                //};
                //populate Test list
                //string response = string.Empty;
                //response = febrisRestClientInitalization.MakeRequest().Result;
                //if (response == "Could Not Find Anything Here")
                //{
                //    response = febrisRestClientInitalization.MakeRequest().Result;
                //}
                //_jSONHandler.InitalizeStatement(response);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
                throw;
            }
        }
    }
}
