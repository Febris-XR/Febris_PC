// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCModuleManagerV3.APIRequests;
using Febris.PCModuleManagerV3.Utilties;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Febris.PCModuleManagerV3.Services
{
    class CheckTimer
    {
        private readonly Timer _timer;
        private readonly Microsoft.Extensions.Logging.ILogger _log;
        private readonly Operations.SimulationReflector _simulationReflector;
        private readonly PCDataProtection _dataProtection;
        //private readonly StatementFolderChecker _statementFolderChecker;
        private readonly ModuleFileChecker _moduleFileChecker;
        private readonly PCFileManager _fileManager;


        public CheckTimer()
        {
            _timer = new Timer(LocalStaticDetails.TimeSpan) { AutoReset = true };
            _timer.Elapsed += TimerElapsed;
            //_timer.Enabled = true;            
            _log = new SerilogLoggerProvider(Log.Logger).CreateLogger(nameof(Program));
            _simulationReflector = new Operations.SimulationReflector(_log);
            _dataProtection = new PCDataProtection(_log);
            //_statementFolderChecker = new StatementFolderChecker(_log);
            _moduleFileChecker = new ModuleFileChecker(_log);
            _fileManager = new PCFileManager(_log, null);

        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Operations.SimulationReflector reflector = new Operations.SimulationReflector();
                bool simulationIsRunning = _simulationReflector.IsSimulationRunning();
                // Was PingRequest.IsConnnectedToInternet(), an ICMP ping to a hardcoded
                // 8.8.8.8. The work below talks to the NODE, so the node is what has to be
                // reachable. See NodeReachability for why the old check was wrong on
                // air-gapped and ICMP-blocking networks.
                bool nodeReachable = NodeReachability.IsNodeReachable();
                if (!simulationIsRunning && nodeReachable)
                {
                    Stop();


                    bool checker = _moduleFileChecker.ModuleChecker();



                    //var task = _dataProtection.CredentialsExist();
                    //task.Wait();
                    //bool credExist = task.Result;

                    //if (credExist)
                    //{
                    //    //get token
                    //    //FebrisLocalLibrary.Communication.TokenHandler.GetToken();

                    //    //var statementFolderCheck = Task.Run(() => _fileManager.GetDirectoryContentNames(PCFileSystem.StatementPath));//.CheckIfFolderHasFile(PCFileSystem.StatementPath));
                    //    //statementFolderCheck.Wait();
                    //    //_ = statementFolderCheck.Result;

                    //    //var moduleFolderCheck = Task.Run(() => _fileManager.GetDirectoryContentNames(PCFileSystem.RecordingsFilePath));//.CheckIfFolderHasFile(PCFileSystem.RecordingsFilePath));
                    //    //videoFolderCheck.Wait();
                    //    //_ = videoFolderCheck.Result;

                    //    //Task.Run(() => FileManager.DeleteOldVideoZipFiles()).Wait();
                    //}
                    Start();
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex.Message);
                try
                {
                    Start();
                }
                catch { }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
            //_timer.Enabled = false;

        }

        public bool Start()
        {
            try
            {
                _timer.Start();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return true;
        }

        public bool Stop()
        {
            try
            {
                _timer.Stop();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return false;
        }

        #region Topshelf.serilog creation
        private static Serilog.ILogger CreateLogger()
        {
            //var builder = new ConfigurationBuilder();
            //BuildConfig(builder);

            string path = PCFileSystem.ModuleManagerLogPath;


            //var logger = new LoggerConfiguration().ReadFrom
            //    .Configuration(builder.Build())
            //    .Enrich.FromLogContext()
            //    .WriteTo.File(Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            var logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
                .CreateLogger();
            return logger;
        }
        #endregion        
    }
}
