// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.TicketModels;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Utilites;
using Febris.SharedServices;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCLauncherV3.APIInteractions
{
    class TokenRequest
    {
        //private readonly JSONHandler _jSONHandler;
        private ILogger _log;
        private IConfiguration _config;
        public string _endpoint;

        public TokenRequest(ILogger log)
        {
            _log = log;
            // _jSONHandler = new JSONHandler(_log);
            //_endpoint = LocalHardwareStaticDetails.PassedBackConfig.GetSection("ApiUrlPath").GetValue<string>("AuthenticationApi");

            _endpoint = LocalHardwareStaticDetails.ApiUrl;
        }

        public TokenRequest(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            //_jSONHandler = new JSONHandler(_log);
            //_endpoint = LocalHardwareStaticDetails.PassedBackConfig.GetSection("ApiUrlPath").GetValue<string>("AuthenticationApi");

            _endpoint = LocalHardwareStaticDetails.ApiUrl;
        }


        #region Requests
        private async Task<string> MakeAuthenticationPostRequest(string method, string dataPackage)
        {
            try
            {

                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Token/" + method,
                    httpMethod = httpVerb.POST,
                    authTech = AuthenticaitonTechnique.None,
                    authType = Authenticationtype.Basic,
                    postJSON = dataPackage ?? string.Empty,
                    //contentType = "",

                };
                string response = string.Empty;
                HttpStatusCode status;
                (response, status) = await request.MakeStringRequest();
                if (status != HttpStatusCode.OK)
                {

                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        private async Task<string> MakeGetRequest(string method, string dataPackage)
        {
            try
            {
                string endpoint = _endpoint;
                IAPIRequestFactory request = new APIRequestFactory()
                {
                    endPoint = endpoint + "Token/" + method,
                    httpMethod = httpVerb.GET,
                    authTech = AuthenticaitonTechnique.Token,
                    authType = Authenticationtype.BearerToken,
                    postJSON = dataPackage ?? string.Empty,
                    //contentType = "",
                    token = LocalHardwareStaticDetails._hardwareAuthenticationResponse?.RefreshToken ?? string.Empty

                };
                string response = string.Empty;
                HttpStatusCode status;
                (response, status) = await request.MakeStringRequest();
                if (status != HttpStatusCode.OK)
                {
                    await Authenticate();
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
                    endPoint = endpoint + "Token/" + method,
                    httpMethod = httpVerb.POST,
                    authTech = AuthenticaitonTechnique.Token,
                    authType = Authenticationtype.BearerToken,
                    postJSON = dataPackage ?? string.Empty,
                    token = LocalHardwareStaticDetails._hardwareAuthenticationResponse?.RefreshToken ?? string.Empty
                };
                string response = string.Empty;
                HttpStatusCode status;
                (response, status) = await request.MakeStringRequest();
                if (status != HttpStatusCode.OK)
                {

                }
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //private async Task<string> MakePutRequest(string method, string dataPackage)
        //{
        //    try
        //    {
        //        string endpoint = _endpoint;
        //        IAPIRequestFactory request = new APIRequestFactory()
        //        {
        //            endPoint = endpoint + "Token/" + method,
        //            httpMethod = httpVerb.PUT,
        //            authTech = AuthenticaitonTechnique.Token,
        //            authType = Authenticationtype.BearerToken,
        //            postJSON = dataPackage ?? string.Empty
        //        };
        //        string response = string.Empty;
        //        HttpStatusCode status;
        //        (response, status) = await request.MakeStringRequest();
        //        if (status != HttpStatusCode.OK)
        //        {

        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //}
        #endregion


        public async Task<HardwareAuthenticationResponse> Authenticate()
        {
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                //if (string.IsNullOrEmpty(MainViewModel.ConfigVM.HardwareLicense))
                //{
                //    UniqueIdentifier uniqueIdentifier = new UniqueIdentifier();
                //    MainViewModel.ConfigVM.HardwareLicense = uniqueIdentifier.GetHardwareLicense();
                //}
                // NODE-9. This used to send a licence derived from WMI (processor id plus
                // motherboard serial). Audit T9 changed the node to MINT the device credential at
                // registration and store only its hash, so a value this client computes for itself
                // matches no row and authentication always fails. The credential the node showed
                // once at registration is pasted into this client and kept encrypted at rest.
                PCDataProtection dataProtection = new PCDataProtection();
                string deviceCredential = dataProtection.GetDeviceCredential();
                if (string.IsNullOrWhiteSpace(deviceCredential))
                {
                    // Fail LOUD and stop. Falling back to the old derived licence would produce a
                    // 401 indistinguishable from a wrong credential, which is precisely how this
                    // defect stayed invisible: the request looked well formed and the node had
                    // nothing to match it against.
                    FebrisLog.Error(new InvalidOperationException(
                        "This device has no Febris credential. Register it on your node's Hardware "
                        + "page, copy the credential it shows once, and paste it into the "
                        + "configuration screen. Authentication cannot proceed without it."));
                    return null;
                }

                HardwareAuthenticationRequest request = new HardwareAuthenticationRequest()
                {
                    LicenseKey = deviceCredential
                };
                dataPackage = JsonConvert.SerializeObject(request);
                method = "authenticate";
                result = await MakeAuthenticationPostRequest(method, dataPackage);
                HardwareAuthenticationResponse output = JsonConvert.DeserializeObject<HardwareAuthenticationResponse>(result);
                //LocalHardwareStaticDetails._hardwareAuthenticationResponse = output;
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task<HardwareAuthenticationResponse> Get(HardwareAuthenticationRequest input)
        {
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                dataPackage = JsonConvert.SerializeObject(input);
                method = "authentication";
                result = await MakeAuthenticationPostRequest(method, dataPackage);
                HardwareAuthenticationResponse output = JsonConvert.DeserializeObject<HardwareAuthenticationResponse>(result);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task<HardwareAuthenticationResponse> Refresh(string input)
        {
            string dataPackage = string.Empty;
            string method = string.Empty;
            string result = string.Empty;
            try
            {
                result = await MakePostRequest(method, dataPackage);
                HardwareAuthenticationResponse output = JsonConvert.DeserializeObject<HardwareAuthenticationResponse>(result);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<bool> RenewToken()
        {
            bool output = default;
            try
            {
                HardwareAuthenticationResponse newTokens = default;
                //try refresh
                if (LocalHardwareStaticDetails._hardwareAuthenticationResponse != default)
                {
                    newTokens = await Refresh(LocalHardwareStaticDetails._hardwareAuthenticationResponse.RefreshToken);
                }

                if (newTokens != null && newTokens != default && !string.IsNullOrEmpty(newTokens.JwtToken) && string.IsNullOrEmpty(newTokens.RefreshToken))
                {
                    StaticDetails.LicenseAuthenticateResponse.JwtToken = newTokens.JwtToken;
                }
                else if (newTokens != null && !string.IsNullOrEmpty(newTokens.JwtToken) && !string.IsNullOrEmpty(newTokens.RefreshToken))
                {
                    LocalHardwareStaticDetails._hardwareAuthenticationResponse = newTokens;
                }
                else
                {
                    newTokens = await Authenticate();
                    LocalHardwareStaticDetails._hardwareAuthenticationResponse = newTokens;
                }
                if (newTokens != default)
                {
                    output = true;
                }
                //try get
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            return output;
        }



        //public async Task<string> GetToken()
        //{
        //    string token = string.Empty;
        //    PCDataProtection dataProtection = new PCDataProtection();
        //    bool credentialsGathered = dataProtection.CredentialsExist().Result;
        //    if (credentialsGathered)
        //    {
        //        dataPackage = JsonConvert.SerializeObject(input);
        //        method = "authentication";
        //        result = await MakeAuthenticationPostRequest(method, dataPackage);
        //        //FebrisRestClient febrisRestClientInitalization = new FebrisRestClient(_log)
        //        //{
        //        //    endPoint = FebrisLocalLibrary.SharedDetails.SharedDetails.getToken,
        //        //    authType = Authenticationtype.Basic,
        //        //    httpMethod = httpVerb.GET,
        //        //};

        //        //populate Test list
        //        string response = string.Empty;
        //        var task = febrisRestClientInitalization.MakeRequest();
        //        task.Wait();
        //        response = task.Result;
        //        if (response == string.Empty)
        //        {
        //            //Login loginWindow = new Login();
        //            //loginWindow.ShowDialog();
        //        }
        //        //token = Utilites.JSONHandler.SetToken(response);
        //        token = _jSONHandler.DeserializeToken(response);
        //        bool tokenSet = StoreToken(token);
        //    }
        //    return token;
        //}

    }
}
