// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.APIInteractions;
using Febris.PCLauncherV3.BusinessLogic;
using Febris.PCLauncherV3.MVVM.View;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Operations;
using Febris.PCLauncherV3.Utilites;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Febris.PCLauncherV3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    
    public partial class MainWindow : Window
    {
        private readonly ILogger _log;
        private readonly IConfiguration _config;
        //private readonly ButtonHandler _buttonHandler;
        private  PCDataProtection _dataProtection;
        private  ServiceUtilities _serviceUtilities;
        private  FileSystemInitalizer _fileSystemInitalizer;
        //private readonly Login _loginWindow;
        //private readonly Config _configWindow;
        //private readonly VideoFiles _videoFileWindow;
        //private readonly StatementFiles _statementFiles;
        //private readonly InitalizerHandler _initalizerHandler;
        private  InitalizationRequest _initalizerHandler;
        private  Launcher _launcher;
        private  TokenRequest _tokenHandler;
        private  ConfigSettings _configSettings;

        #region General         
        public MainWindow(ILogger<MainWindow> log, IConfiguration config)
        {
            _log = log;
            _config = config;
            ///URL for api calls was not being set 
            //URLSettingUtility.SetURL();
            InitializeMethods();
            URLSettingUtility.SetURL();
            InitializeMethods();
            //_dataProtection = new PCDataProtection(_log, _config);
            //_serviceUtilities = new ServiceUtilities(_log, _config);
            //_fileSystemInitalizer = new FileSystemInitalizer(_log, _config);
            ////_loginWindow = new Login(_log, _config);
            ////_configWindow = new Config(_log, _config);
            ////_videoFileWindow = new VideoFiles(_log, _config);
            ////_statementFiles = new StatementFiles(_log, _config);
            //_initalizerHandler = new InitalizationRequest(_log, _config);
            //_launcher = new Launcher(_log, _config);
            //_tokenHandler = new TokenRequest(_log, _config);
            //_configSettings = new ConfigSettings(_log, _config);

            //check or create needed files
            _fileSystemInitalizer.FileInitalizer();


#if (!DEBUG)
            //initalize the services
            _serviceUtilities.ServiceInitializer();
#endif
            ///commented out so it wont start the other services
            _serviceUtilities.ServiceInitializer();

            //this is setup by visual studio
            try
            {
                //this.DataContext = new MainViewModel();

                #region Setup static view models
                //_=new MainViewModel();
                //MainViewModel.HomeVM = new HomeViewModel() { };
                //MainViewModel.UserVM = new UserViewModel() { };
                //MainViewModel.ModuleVM = new ModuleViewModel() { };
                //MainViewModel.LaunchVM = new LaunchViewModel() { };
                ////MainViewModel.ConfigVM = ConfigVMSetup();
                //MainViewModel.StatementVM = new StatementViewModel() { };
                //MainViewModel.VideoVM = new VideoViewModel() { };

                //MainViewModel.CurrentView = MainViewModel.HomeVM;
                //MainViewModel.HomeViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.HomeVM; });
                //MainViewModel.UserViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.UserVM; });
                //MainViewModel.ModuleViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.ModuleVM; });
                //MainViewModel.LaunchViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.LaunchVM; });
                //MainViewModel.ConfigViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.ConfigVM; });
                //MainViewModel.StatementViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.StatementVM; });
                //MainViewModel.VideoViewCommand = new RelayCommand(x => { MainViewModel.CurrentView = MainViewModel.VideoVM; });

                //MainViewModel.CurrentView = MainViewModel.HomeVM;

                
                _initalizerHandler.Initalize();
                #endregion



                InitializeComponent();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

        }

        //#######################################################################################################
        //Loads all parts of the window
        //#######################################################################################################
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //FebrisLocalLibrary.Credentials.DataProtection data = new FebrisLocalLibrary.Credentials.DataProtection();
                //bool credentialsExist = _dataProtection.CredentialsExist().Result;
                //if (!credentialsExist)
                //{
                //    //Login loginWindow = new Login();
                //    _loginWindow.ShowDialog();
                //}

                #region Domain Prefix
                JObject domainPrefix = _configSettings.Get().Result;

                var domainInfo = ConfigLogic.GetSettings().Result;

#if (DEBUG)
#elif (STAGING)
#else
                //try
                //{
                //    if (domainPrefix["domainprefix"] == null)
                //    {
                //        _configWindow.ShowDialog();
                        
                //    }
                //    LocalHardwareStaticDetails.prefix = domainPrefix["domainprefix"].ToString();
                //    //LocalHardwareStaticDetails.url = "https://" + LocalHardwareStaticDetails.prefix + ".febr.is";
                //}
                //catch
                //{
                //    _configWindow.ShowDialog();
                //}
#endif

                #endregion

                //if token is still string.empty pop up login window
                //Task.Run(() => _tokenHandler.GetToken()).Wait();
                //_initalizerHandler.Initalize();
                //ModuleList.ItemsSource = LocalHardwareStaticDetails._moduleList.Where(t => t.Obsolete == false);
                //MessageBoard.ItemsSource = LocalHardwareStaticDetails._febrisMessageBoard;
                //HomeView.LocalMessageBoard.ItemsSource = LocalHardwareStaticDetails._localMessageBoard;
                //UserViewModel.UserList.ItemsSource = LocalHardwareStaticDetails._userList;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

        }

        private void RefreshInformation(object sender, RoutedEventArgs e)
        {
            try
            {
                ///clean out the current static details area
                CleanOutStaticDetails();

                //_initalizerHandler.Initalize();

                Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(() =>
                {
                    _initalizerHandler.Initalize();

                    MainViewModel vm = new MainViewModel();
                    this.DataContext = vm;

                }));


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        private static void CleanOutStaticDetails()
        {
            LocalHardwareStaticDetails._hardwareInitializationResponse = new HardwareInitializationResponse();            
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void InitializeMethods()
        {
            _dataProtection = new PCDataProtection(_log, _config);
            _serviceUtilities = new ServiceUtilities(_log, _config);
            _fileSystemInitalizer = new FileSystemInitalizer(_log, _config);
            //_loginWindow = new Login(_log, _config);
            //_configWindow = new Config(_log, _config);
            //_videoFileWindow = new VideoFiles(_log, _config);
            //_statementFiles = new StatementFiles(_log, _config);
            _initalizerHandler = new InitalizationRequest(_log, _config);
            _launcher = new Launcher(_log, _config);
            _tokenHandler = new TokenRequest(_log, _config);
            _configSettings = new ConfigSettings(_log, _config);

            //ServiceRestartCommand();

        }

        private void ServiceRestartCommand()
        {
            try
            {
                _serviceUtilities.ServiceRestarter(ServiceOptions.Downloader);
                _serviceUtilities.ServiceRestarter(ServiceOptions.Uploader);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
        #endregion


        //private ConfigViewModel ConfigVMSetup()
        //{
        //    JObject settings = _configSettings.Get().Result;

        //    bool credsExist = false;
        //    string userName = string.Empty;
        //    string secret = string.Empty;

        //    (credsExist, userName, secret) = _dataProtection.GetCredentials().Result;

        //    ConfigViewModel output = new ConfigViewModel()
        //    {
        //        HardwareLicense = SetHardwareLicense(),
        //        DomainPrefix = settings["domainprefix"]?.ToString()??string.Empty,
        //        //EmailAddress = "AWonderfulEmail@email.com"
        //        CredentialsExist = credsExist
        //    };

        //    if (credsExist)
        //    {
        //        output.EmailAddress = userName;
        //        output.Password = secret;
        //    }

        //    return output;
        //}

        //public string SetHardwareLicense()
        //{
        //    UniqueIdentifier uniqueIdentifier = new UniqueIdentifier();
        //    try
        //    {
        //        string output = string.Empty;
        //        output = uniqueIdentifier.GetHardwareLicense();
        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.StackTrace);
        //        throw;
        //    }
        //}
    }
}
