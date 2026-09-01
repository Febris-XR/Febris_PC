// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCScreenRecorderV3.CoreOperations;
using Febris.PCScreenRecorderV3.Utilities;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;

namespace Febris.PCScreenRecorderV3
{
    class Program
    {
        #region serilog creation
        private static ILogger CreateLogger()
        {
            //ILogger logger = new LoggerConfiguration().CreateLogger();
            //try
            //{
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            string path = PCFileSystem.RecorderLogPath;

            ILogger logger = new LoggerConfiguration().ReadFrom
                .Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.File(System.IO.Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //}
            //catch { }
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
            try
            {
                builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables();
            }
            catch { }
        }
        #endregion

        //private FebrisLocalLibrary.Utilites.UnityReflector unityReflector {get;}
        public static void Main(string[] args)
        {
            try
            {
                try
                {
                    Log.Logger = CreateLogger();
                }
                catch { }
                try
                {
                    try
                    {
                        args = Environment.GetCommandLineArgs();
                        foreach (var arg in args)
                        {
                            Log.Logger.Error(arg);
                        }
                    }
                    catch { }
                    #region operation                    
                    ArgumentRequestHandler jSONHandler = new ArgumentRequestHandler(Log.Logger);
                    var task = jSONHandler.ArgumentHandler(args);
                    StartupClass startup = new StartupClass(Log.Logger);
                    startup.StartRecording();
                    #endregion
                }
                catch
                {
                    Log.Logger.Error("Could not collect arguments");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
            }
        }

        #region dependency injection but apparently does not work. 
        //public static void Main(string[] args)
        //{
        //    try
        //    {
        //        Log.Logger = CreateLogger();
        //        try
        //        {
        //            args = Environment.GetCommandLineArgs();
        //            foreach (var arg in args)
        //            {
        //                Log.Logger.Error(arg);
        //            }
        //        }
        //        catch
        //        {
        //            Log.Logger.Error("Could not collect arguments");
        //        }
        //        #region set up host
        //        var host = Host.CreateDefaultBuilder()
        //            .ConfigureServices((context, services) =>
        //            {
        //                #region in this project
        //                //core operations
        //                services.AddTransient<ScreenRecorder>();
        //                services.AddTransient<SimulationReflector>();
        //                services.AddTransient<StartupClass>();
        //                //utilites                    
        //                services.AddTransient<Utilities.JSONHandler>();
        //                #endregion

        //                #region in local library
        //                //communication
        //                services.AddTransient<FebrisLocalLibrary.Communication.FebrisRestClient>();
        //                services.AddTransient<FebrisLocalLibrary.Communication.TokenHandler>();
        //                //creds
        //                services.AddTransient<FebrisLocalLibrary.Credentials.DataProtection>();
        //                //filesystem
        //                services.AddTransient<FebrisLocalLibrary.FileSystem.FileManager>();
        //                services.AddTransient<FebrisLocalLibrary.FileSystem.FileSystem>();
        //                services.AddTransient<FebrisLocalLibrary.FileSystem.FileSystemInitalizer>();
        //                //service
        //                services.AddTransient<FebrisLocalLibrary.Service.ProgressBarService>();
        //                //utilites
        //                services.AddTransient<FebrisLocalLibrary.Utilites.ProcessUtilites>();
        //                services.AddTransient<FebrisLocalLibrary.Utilites.SimulationReflector>();
        //                services.AddTransient<FebrisLocalLibrary.Utilites.ServiceUtilities>();
        //                services.AddTransient<FebrisLocalLibrary.Utilites.JSONHandler>();
        //                services.AddTransient<FebrisLocalLibrary.Utilites.UniqueIdentifier>();
        //                #endregion
        //            })
        //            .UseSerilog()
        //            .Build();
        //        #endregion

        //        #region inject logger and config

        //        var startUpInjection = ActivatorUtilities.CreateInstance<StartupClass>(host.Services);

        //        #endregion

        //        #region start Startup         
        //        startUpInjection.Run(args);
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error(ex.Message);
        //    }
        //}
        #endregion
    }
}
