// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCStatementManagerV3.Services;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using Topshelf;

namespace Febris.PCStatementManagerV3
{
    public class Program
    {

        //top shelf running
        // path>name.exe install start
        //stoping it
        //path >name.exe uninstall

        //***************************************************************************************
        //Todo: add a precentive setup to stop it from executing when in the middle of an operation
        //Need to add log files
        //Only interact with .mp4 otherwise delete file
        //***************************************************************************************
        //private static readonly string _exePath = Assembly.GetExecutingAssembly().Location;
        public static void Main(string[] args)
        {
#if (!DEBUG)            
            Log.Logger = CreateLogger();        
#endif

            // ROADMAP 21: teach this SERVICE process the node's API URL. LocalHardwareStaticDetails
            // is a static in the shared library and statics are per-process, so the Launcher
            // calling URLSettingUtility.SetURL() in ITS process never reached this one -- ApiUrl
            // stayed empty here and every request resolved to a relative "Token/...". Reads
            // ApiUrlPath:DataApi from appsettings.json next to the executable, or the
            // ApiUrlPath__DataApi environment variable (the builder already chains
            // AddEnvironmentVariables). Fails CLOSED and LOUDLY: an unconfigured client must not
            // guess a node, but it must say so in a way an operator can act on.
            var startupConfig = new ConfigurationBuilder();
            BuildConfig(startupConfig);
            string apiUrlError;
            if (!ClientApiUrlResolver.TryApply(startupConfig.Build(), out apiUrlError))
            {
                Log.Fatal(apiUrlError);
                Environment.ExitCode = 1;
                return;
            }
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //Directory.SetCurrentDirectory(path);
            //using topshelf
            try
            {
//#if (!DEBUG)
                #region topshelf host factory
                var exitCode = HostFactory.Run(x =>
                {
                    x.Service<CheckTimer>(s =>
                    {
                        s.ConstructUsing(checkTimer => new CheckTimer());
                        s.WhenStarted(checkTimer => checkTimer.Start());
                        s.WhenStopped(checkTimer => checkTimer.Stop());
                    });
                    //get permissions
                    x.RunAsLocalSystem();

                    //x.StartAutomatically();

                    //Log.Logger = CreateLogger();
                    x.UseSerilog();

                    x.SetServiceName("FebrisBackgroundUploader");
                    x.SetDisplayName("Febris Background Uploader");
                    x.SetDescription("Uploads Videos and Statements to Febris Servers");
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
                #endregion
//#endif
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        #region Topshelf.serilog creation
        private static ILogger CreateLogger()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            string path = PCFileSystem.UploaderLogPath;

            var logger = new LoggerConfiguration().ReadFrom
                .Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
            //var logger = new LoggerConfiguration()
            //    .WriteTo.File(Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
            //    .MinimumLevel.Debug()
            //    .CreateLogger();
            return logger;
        }

        //private static void CreateLogger()
        //{            
        //    string path = FebrisLocalLibrary.FileSystem.FileSystem.UploaderLogPath;

        //    var logger = new LoggerConfiguration()
        //        .WriteTo.File(Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
        //        .MinimumLevel.Debug()
        //        .CreateLogger();            
        //}
        #endregion

        #region config
        /// <summary>
        /// Logging setup with serialog
        /// </summary>
        /// <param name="builder"></param>
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  // ROADMAP 21: was
                // optional:false with NO appsettings.json committed in this project, so the service
                // threw FileNotFoundException at startup. The URL check below is the real gate.
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
        #endregion
    }
}
