// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using System.IO;
using Febris.SharedServices;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Operations;
using Febris.PCLauncherV3.APIInteractions;
using Febris.PCLauncherV3.Utilites;
using Serilog;

namespace Febris.PCLauncherV3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private readonly InitalizationRequest _initalizerHandler;
        //public App()
        //{
        //    _initalizerHandler = new InitalizationRequest(_log, _config);
        //}
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = CreateLogger();
            #region set up host
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    #region in this project
                    //windows
                    services.AddTransient<MainWindow>();
                    
                    services.AddTransient<UserViewModel>();
                    services.AddTransient<StatementViewModel>();
                    services.AddTransient<ModuleViewModel>();
                    services.AddTransient<LaunchViewModel>();
                    services.AddTransient<ConfigViewModel>();
                    //operations
                    services.AddTransient<Operations.Launcher>();
                    //services.AddTransient<Operations.InitalizerHandler>();
                    //services.AddTransient<StatementRequest>();                    
                    ////utilites                    
                    //services.AddTransient<Utilites.ButtonHandler>();
                    // services.AddTransient<JSONHandler>();
                    //services.AddTransient<Utilites.SearchHandler>();
                    //services.AddTransient<Utilites.StatementInitalizer>();
                    services.AddTransient<Utilites.VideoDataHandler>();

                    //Api Requests
                    services.AddTransient<StatementRequest>();
                    services.AddTransient<InitalizationRequest>();
                    services.AddTransient<TokenRequest>();

                    #endregion

                    #region in local library
                    //communication
                    services.AddTransient<APIRequestFactory>();

                    //services.AddTransient<FebrisLocalLibrary.Communication.FebrisRestClient>();
                    //services.AddTransient<FebrisLocalLibrary.Communication.TokenHandler>();
                    //creds
                    services.AddTransient<PCDataProtection>();
                    //filesystem
                    services.AddTransient<PCFileManager>();
                    services.AddTransient<PCFileSystem>();
                    services.AddTransient<FileSystemInitalizer>();
                    //service
                    services.AddTransient<ProgressBarService>();
                    //utilites
                    services.AddTransient<ProcessUtilites>();
                    services.AddTransient<SimulationReflector>();
                    services.AddTransient<ServiceUtilities>();
                    services.AddTransient<JSONHandler>();
                    services.AddTransient<Utilites.UniqueIdentifier>();
                    #endregion                    
                })
                .UseSerilog()
                .Build();
            #endregion

            #region inject logger and config
            #region project injection
            //windows
            var mainWindowInjection = ActivatorUtilities.CreateInstance<MainWindow>(host.Services);

            //Turns out everything under here throws massive errors when used

            //var loginWindowInjection = ActivatorUtilities.CreateInstance<Login>(host.Services);
            //var professionalInfoWindowInjection = ActivatorUtilities.CreateInstance<ProfessionalInfo>(host.Services);
            //var statementFilesWindowInjection = ActivatorUtilities.CreateInstance<StatementFiles>(host.Services);
            //var educationInfoWindowInjection = ActivatorUtilities.CreateInstance<TestInfo>(host.Services);
            //var videoFileWindowInjection = ActivatorUtilities.CreateInstance<VideoFiles>(host.Services);
            ////operaitons
            //var launcherInjection = ActivatorUtilities.CreateInstance<Operations.Launcher>(host.Services);
            //var initalizerInjection = ActivatorUtilities.CreateInstance<Operations.InitalizerHandler>(host.Services);
            ////utilites
            //var buttonInjection = ActivatorUtilities.CreateInstance<Utilites.ButtonHandler>(host.Services);
            //var jSONInjection = ActivatorUtilities.CreateInstance<Utilites.JSONHandler>(host.Services);
            //var searchInjection = ActivatorUtilities.CreateInstance<Utilites.SearchHandler>(host.Services);
            //var statementInitalizerInjection = ActivatorUtilities.CreateInstance<Utilites.StatementInitalizer>(host.Services);
            //var videoDataInjection = ActivatorUtilities.CreateInstance<Utilites.VideoDataHandler>(host.Services);
            //#endregion

            //#region library injection
            ////communication
            //var febrisRestClientInjection = ActivatorUtilities.CreateInstance<FebrisLocalLibrary.Communication.FebrisRestClient>(host.Services);
            //var tokenHandlerInjection = ActivatorUtilities.CreateInstance<FebrisLocalLibrary.Communication.TokenHandler>(host.Services);
            ////creds
            //var dataProtectionInjection = ActivatorUtilities.CreateInstance<FebrisLocalLibrary.Credentials.DataProtection>(host.Services);
            ////fileSystem
            //var fileManagerInjection = ActivatorUtilities.CreateInstance<FileManager>(host.Services);
            //var fileSystemInjection = ActivatorUtilities.CreateInstance<FileSystem>(host.Services);
            //var fileSystemInitalizerInjection = ActivatorUtilities.CreateInstance<FileSystemInitalizer>(host.Services);
            ////services
            //var progressBarInjection = ActivatorUtilities.CreateInstance<FebrisLocalLibrary.Service.ProgressBarService>(host.Services);
            ////utilites
            //var processUtilitesInjection = ActivatorUtilities.CreateInstance<ProcessUtilites>(host.Services);
            //var simulationReflectorInjection = ActivatorUtilities.CreateInstance<SimulationReflector>(host.Services);
            //var serviceUtilitesInjection = ActivatorUtilities.CreateInstance<ServiceUtilities>(host.Services);
            //var jSONHandlerInjection = ActivatorUtilities.CreateInstance<JSONHandler>(host.Services);
            //var uniqueIdentifierInjection = ActivatorUtilities.CreateInstance<UniqueIdentifier>(host.Services);
            #endregion

            #endregion

            #region start main window            
            mainWindowInjection.Show();
            #endregion

            


            base.OnStartup(e);
        }

        #region serilog creation
        private static Serilog.ILogger CreateLogger()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            string path = PCFileSystem.LauncherLogPath;

            var logger = new LoggerConfiguration().ReadFrom
                .Configuration(builder.Build())
                .Enrich.FromLogContext()
                .WriteTo.File(System.IO.Path.Combine(path, "log.json"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
            return logger;
        }

        #endregion

        #region serilog config
        /// <summary>
        /// Logging setup with serialog
        /// </summary>
        /// <param name="builder"></param>
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables();
        }
        #endregion

       
    }
}
