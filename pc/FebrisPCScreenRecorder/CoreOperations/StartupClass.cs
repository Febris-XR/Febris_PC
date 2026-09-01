// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCScreenRecorderV3.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Febris.PCScreenRecorderV3.CoreOperations
{
    public class StartupClass
    {
        //private readonly Microsoft.Extensions.Logging.ILogger _log;
        private readonly IConfiguration _config;
        private readonly ArgumentRequestHandler _jSONHandler;
        private readonly ScreenRecorder _screenRecorder;
        private readonly SimulationReflector _simulationReflector;
        private Serilog.ILogger _log;

        public StartupClass(Serilog.ILogger log)
        {
            _log = log;
            _jSONHandler = new ArgumentRequestHandler(_log);
            _screenRecorder = new ScreenRecorder(_log);
            _simulationReflector = new SimulationReflector(_log);
        }

        //public StartupClass(ILogger<StartupClass> log, IConfiguration config)
        //{
        //    _log = log;
        //    _config = config;
        //    _jSONHandler = new JSONHandler(_log, _config);
        //    _screenRecorder = new ScreenRecorder(_log, _config);
        //    _simulationReflector = new SimulationReflector(_log, _config);

        //}

        public void Run(string[] args)
        {
            try
            {
                #region operation
                //string[] arguments = Environment.GetCommandLineArgs();
                //var task = Task.Run(() => _jSONHandler.ArgumentHandler(args));
                //task.Wait();
                //_ = task.Result;
                var task = _jSONHandler.ArgumentHandler(args);
                StartRecording();
                #endregion
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _log.Information("an error happened when trying to start this recorder");
            }
        }

        public bool StopRecording()
        {
            bool stopped = false;
            try
            {
                //Console.WriteLine("Recording stopping");

                //stopped = Task.Run(() => ScreenRecorder.Stop()).Result;
                ScreenRecorderStaticDetails.timer.Dispose();
                //StaticDetails.processCheckTimer.Dispose();
                //Task.Run(()=>ScreenRecorder.Stop()).Wait();
                //stopped = Task.Run(() => ScreenRecorder.Stop()).Result;
                stopped = _screenRecorder.Stop();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return stopped;
        }

        internal void StartRecording()
        {
            try
            {
                //set to record
                //StaticDetails.bounds = new Rectangle(0, 0, 1920, 1080);
                _screenRecorder.CreateTempFolder(ScreenRecorderStaticDetails.tempPath);

                //Task.Run(() => StaticDetails.timer = new Timer(tmrRecord_Tick, null, 0, 42));
                ScreenRecorderStaticDetails.timer = new Timer(TmrRecord_Tick, null, 0, ScreenRecorderStaticDetails.TimerPeriod);

                Thread.Sleep(5000);




                bool isRunning = _simulationReflector.IsSimulationRunning();
                //bool isRunning = true;


                while (isRunning)
                {
                    isRunning = _simulationReflector.IsSimulationRunning();
                    Thread.Sleep(2000);
                }
                if (!isRunning)
                {
                    _log.Information("recorder stopping due to reflector showing false" + "\n");
                    _log.Information("processName: " + ScreenRecorderStaticDetails.simulationProcessName);
                    _log.Information("ProcessId: " + ScreenRecorderStaticDetails.simulationProcessId);
                    bool stopped = StopRecording();
                    _log.Information("process is stopped: " + stopped.ToString());
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }

        }

        // NOTE (PC-B11): Timer fires every TimerPeriod (50ms) and each Task.Run runs RecordFrame with no reentrancy guard, so overlapping captures race on the non-thread-safe ScreenRecorderStaticDetails.inputImageSequence (List) and fileCount, and there is no max-duration or max-frame safety stop. Deferred per do-not-change-functionality: a robust fix requires serializing captures, guarding the shared list, and adding configurable safety caps (PC-M5 Tier 3). See docs/MODERNIZATION/PC_MODERNIZATION.md.
        private void TmrRecord_Tick(Object o)
        {
            try
            {
                Console.WriteLine("record");
                Task.Run(() => _screenRecorder.RecordFrame());
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message + " : A recorder tick has been dropped");
            }
        }
    }
}
