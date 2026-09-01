// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCModuleManagerV3.Services;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using Topshelf;

namespace Febris.PCModuleManagerV3
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // The logger MUST be assigned before the fail-closed guard below, not after. Serilog's
            // static Log.Logger is a SilentLogger until it is assigned, and Log.Fatal on a
            // SilentLogger produces no output at all, so a guard that runs first refuses to start
            // and says NOTHING. That is exactly what shipped: 0079a91 inserted the identical guard
            // block into this file and into FebrisPCStatementManager at two different anchors, and
            // here it landed above the #region that holds this assignment. The sibling service got
            // it right by accident of layout, not by design. Found 2026-08-25.
            //
            // Known and deliberately unchanged: under #if (!DEBUG) a DEBUG build assigns nothing,
            // so in DEBUG both services log to SilentLogger for their whole lifetime, including
            // Services/CheckTimer.cs's SerilogLoggerProvider and Topshelf's UseSerilog (which reads
            // this static and never assigns it). These ship as Release Windows services, so Release
            // is the configuration that matters and it is the one this fix corrects.
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

            #region topshelf service
            //Log.Logger = CreateLogger();
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //Directory.SetCurrentDirectory(path);

            try
            {
//#if (!DEBUG)
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

                    x.UseSerilog();


                    x.SetServiceName("FebrisModuleManager");
                    x.SetDisplayName("Febris Module Manager");
                    x.SetDescription("Downloads and unpacks educaiton modules from Febris");
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
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
            #endregion
        }



#region Topshelf.serilog creation
        private static ILogger CreateLogger()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            string path = PCFileSystem.ModuleManagerLogPath;

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
