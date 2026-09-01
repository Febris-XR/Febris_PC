// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using Febris.PCLauncherV3.Utilites;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Febris.SharedServices.Launcher;
using Febris.ModelLibrary.Models.DataModels;
using Febris.SharedServices;
using Febris.ModelLibrary.ViewModels;
using Febris.ModelLibrary.LauncherModels;
using Febris.PCLauncherV3.APIInteractions;
using Febris.ModelLibrary.Models.XApiModels;
using System.Linq;

namespace Febris.PCLauncherV3.Operations
{
    class Launcher
    {
        private readonly ILogger _log;
        private readonly IConfiguration _config;
        private readonly VideoDataHandler _videoDataHandler;
        private readonly SimulationReflector _simulationReflector;
        //private readonly StatementInitalizer _statementInitalizer;
        private readonly StatementRequest _statementRequest;

        public Launcher(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
            _videoDataHandler = new VideoDataHandler(_log, _config);
            _simulationReflector = new SimulationReflector(_log, _config);
            _statementRequest = new StatementRequest(_log, _config);
            //_statementInitalizer = new StatementInitalizer(_log, _config);
        }
        //***********************************************************************************************************************************
        //Start the selected training or testing
        //Todo: launch video recorder? or embed that in unity.
        //***********************************************************************************************************************************
        public async Task StartSimulation(HardwareUserViewModel uservm, Module module)
        {
            try
            {
                StatementInitalizationResponseViewModel response = default;
                response = _statementRequest.StatmentInitalizer(uservm, module).Result;
                if(response == default)
                {
                    return;
                }

                //if the response fails to work properly trigger a popup

                #region - not used
                ///I think this is in a different place in the stack for V2
                //string statementInitializer = JsonConvert.SerializeObject(response);
                //string StatementJson = JSONTestInput(response.Statement);
                #endregion

                //travel to module path
                string launchPathString = Path.Combine(PCFileSystem.ModuleLinkPath, module.UUID.ToString() + ".lnk");

                // ROADMAP 22 / WI-12: the client-side "remove video attachment if opt out" block
                // that sat here is GONE. It stripped the node's video attachment whenever the
                // learner's local checkbox was clear, which let the person being recorded veto the
                // educator's decision from an uncontrolled device. The record decision is now the
                // node's, derived from the educator's per-cohort policy, and the attachment the
                // node sends IS the instruction -- there is nothing here to second-guess it with.

                #region set up simulation process
                //set up simulation process
                string statementJson = JsonConvert.SerializeObject(response);
                statementJson = JSONTestInput(statementJson);
                Process process = new Process();
                //process.StartInfo = new ProcessStartInfo(launchPathString, arguments: StaticDetails.preface + InputJson);
                process.StartInfo = new ProcessStartInfo(launchPathString, arguments: LauncherSharedDetails.StatementPreface + statementJson);
                process.StartInfo.UseShellExecute = true;
                process.Start();
                #endregion

                //dwell to give the system time to catch up
                Thread.Sleep(500);

                //start up unity reflector // need to run parallel
                #region start reflector
                Task.Run(() => _simulationReflector.SimulationRunningInitalization(process.ProcessName, process.Id));
                #endregion

                #region Check and start video requirements
                // WI-12 + ROADMAP 22. This gate used to demand three things, and two of them were
                // wrong:
                //   1. the learner's local recordSession flag -- removed, the decision is the
                //      node's now and a client does not get to veto it;
                //   2. an attachment whose Display value is the literal "video" -- UNSATISFIABLE.
                //      Display carries the recording NAME (the video ownership chain depends on
                //      that, see docs/BUGS.md T6), so it never equalled "video" and this branch
                //      could not be entered even when the node did send a video attachment.
                // What is left is the question that was always the right one, and VideoNeeded
                // already answers it correctly: is there an attachment with ContentType
                // video/mp4. Null-safe because a non-recorded launch carries no attachments at all.
                if (response.Statement.Attachments != null
                    && response.Statement.Attachments.Count > 0)
                {
                    bool needsVideo = false;
                    string videoName = string.Empty;
                    (needsVideo, videoName) = _videoDataHandler.VideoNeeded(response.Statement.Attachments);//(statementJObject["Attachments"]);
                    if (needsVideo)
                    {
                        Process videoProcess = new Process();
                        string recorderPath = PCFileSystem.screenRecorderPath;//this needs to be changed so something that will work.                         
                        videoProcess.StartInfo = new ProcessStartInfo(recorderPath);
                        videoProcess.StartInfo.Arguments = LauncherSharedDetails.VideoDataPreface + videoName;
                        videoProcess.StartInfo.UseShellExecute = true;//false;// true;//this apprently needs to be false to hide the window
                        videoProcess.StartInfo.CreateNoWindow = true;

                        //videoProcess.
                        videoProcess.Start();
                    }
                }
                #endregion

                //going to need to change this to not close out but do toher stuff
                //Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

        // NOTE (PC-B3): CLI statement transport strips all backslashes and spaces irreversibly (sb.Replace below), corrupting JSON values with paths or escaped chars. Deferred per do-not-change-functionality: fixing it requires a temp-file, stdin, or Base64 JSON protocol change across launcher, recorder, and simulator. See docs/MODERNIZATION/PC_MODERNIZATION.md.
        private static string JSONTestInput(string statement)
        {

            var sb = new StringBuilder(statement);
            //sb.Replace("\"", "'");
            sb.Replace(@"\", string.Empty);
            sb.Replace("\"{", "{");
            sb.Replace("}\"", "}");
            sb.Replace(" ", "_-_");
            //sb.Replace(" ", string.Empty);
            //sb.Replace("\\", string.Empty);
            //sb.Replace("\"", "\\\"");
            //sb.Replace(@"\\\", @"\");
            //sb.Replace(@"\", @"\\\");
            //sb.Replace("\"", "'");
            //sb.Replace("\"UUID\": \"00000000-0000-0000-0000-000000000000\",", string.Empty);
            //sb.Replace("\"Stored\": \"0001-01-01T00:00:00\",", string.Empty);
            //sb.Replace("\"Id\": 0,", string.Empty);
            //sb.Replace("\"result\": null,", string.Empty);
            //sb.Replace("\"context\": null,", string.Empty);
            //sb.Replace("\"authority\": null,", string.Empty);
            //sb.Replace("\"version\": null,", string.Empty);
            //sb.Replace("\"attachments\": null,", string.Empty);
            //sb.Replace("\"mbox_sha1sum\": null,", string.Empty);
            //sb.Replace("\"openId\": null,", string.Empty);
            //sb.Replace("\"account\": null,", string.Empty);
            //sb.Replace("\"member\": null,", string.Empty);
            var testDataInput = sb.ToString();
            return testDataInput;
        }

        //***********************************************************************************************************************************
        //format JSON Data to something useable
        //  - Todo : remove id, UUID, stored, and everything with null value
        //***********************************************************************************************************************************
        private static string JSONTestInput()
        {
            var sb = new StringBuilder(LocalHardwareStaticDetails.serializedStatement);
            //sb.Replace("\"", "'");
            sb.Replace(@"\", string.Empty);
            sb.Replace("\"{", "{");
            sb.Replace("}\"", "}");
            sb.Replace(" ", "_-_");
            //sb.Replace(" ", string.Empty);
            //sb.Replace("\\", string.Empty);
            //sb.Replace("\"", "\\\"");
            //sb.Replace(@"\\\", @"\");
            //sb.Replace(@"\", @"\\\");
            //sb.Replace("\"", "'");
            //sb.Replace("\"UUID\": \"00000000-0000-0000-0000-000000000000\",", string.Empty);
            //sb.Replace("\"Stored\": \"0001-01-01T00:00:00\",", string.Empty);
            //sb.Replace("\"Id\": 0,", string.Empty);
            //sb.Replace("\"result\": null,", string.Empty);
            //sb.Replace("\"context\": null,", string.Empty);
            //sb.Replace("\"authority\": null,", string.Empty);
            //sb.Replace("\"version\": null,", string.Empty);
            //sb.Replace("\"attachments\": null,", string.Empty);
            //sb.Replace("\"mbox_sha1sum\": null,", string.Empty);
            //sb.Replace("\"openId\": null,", string.Empty);
            //sb.Replace("\"account\": null,", string.Empty);
            //sb.Replace("\"member\": null,", string.Empty);
            var testDataInput = sb.ToString();
            return testDataInput;
        }
        //***********************************************************************************************************************************
        //go to website
        //***********************************************************************************************************************************
        public void LaunchPortal()
        {
            try
            {
                ProcessStartInfo url = new ProcessStartInfo
                {
                    FileName = LocalHardwareStaticDetails.ApiUrl,
                    UseShellExecute = true
                };
                Process.Start(url);
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

    }
}
