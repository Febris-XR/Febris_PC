// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.BusinessLogic;
using Febris.PCLauncherV3.MVVM.View;
using Febris.PCLauncherV3.Utilites;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger _log;
        private readonly IConfiguration _config;

        #region different View Model

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChange();
            }
        }


        public HomeViewModel HomeVM { get; set; }
        public RelayCommand HomeViewCommand { get; set; }

        public UserViewModel UserVM { get; set; }
        public RelayCommand UserViewCommand { get; set; }
        //public RelayCommand SelectUser { get; set; }

        public ModuleViewModel ModuleVM { get; set; }
        public RelayCommand ModuleViewCommand { get; set; }

        public LaunchViewModel LaunchVM { get; set; }
        public RelayCommand LaunchViewCommand { get; set; }

        public ConfigViewModel ConfigVM { get; set; }
        public RelayCommand ConfigViewCommand { get; set; }

        public StatementViewModel StatementVM { get; set; }
        public RelayCommand StatementViewCommand { get; set; }

        public VideoViewModel VideoVM { get; set; }
        public RelayCommand VideoViewCommand { get; set; }
        #endregion


        public MainViewModel()
        {


            HomeVM = new HomeViewModel()
            {
                FebrisMessageBoard = LocalHardwareStaticDetails._hardwareInitializationResponse?.MessageboardViewModels?.AdminMessageBoardList ?? new List<AdminMessageBoard>(),
                LocalMessageBoard = LocalHardwareStaticDetails._hardwareInitializationResponse?.MessageboardViewModels?.MessageBoardList ?? new List<MessageBoard>()
            };
            UserVM = new UserViewModel()
            {
                UserList = LocalHardwareStaticDetails._hardwareInitializationResponse?.UserInitaliztionViewModels?.UserViewModelList ?? new List<HardwareUserViewModel>(),
                SearchResultList = LocalHardwareStaticDetails._hardwareInitializationResponse?.UserInitaliztionViewModels?.UserViewModelList ?? new List<HardwareUserViewModel>(),
            };
            ModuleVM = new ModuleViewModel()
            {
                ModuleList = LocalHardwareStaticDetails._hardwareInitializationResponse?.ModuleList ?? new List<Module>(),
                SearchResultList = LocalHardwareStaticDetails._hardwareInitializationResponse?.ModuleList ?? new List<Module>()
            };
            LaunchVM = new LaunchViewModel() { };
            //    ModuleSelected = ModuleHasBeenSelected(),
            //    UserSelected = UserHasBeenSelected()            
            //};
            ConfigVM = ConfigVMSetup();
            //ConfigVM = new ConfigViewModel();
            StatementVM = StatementVMSetup();
            VideoVM = VideoVMSetup();
            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(x => { CurrentView = HomeVM; });
            UserViewCommand = new RelayCommand(x => { CurrentView = UserVM; });
            ModuleViewCommand = new RelayCommand(x => { CurrentView = ModuleVM; });
            LaunchViewCommand = new RelayCommand(x => { CurrentView = new LaunchViewModel(); });
            ConfigViewCommand = new RelayCommand(x => { CurrentView = ConfigVM; });
            StatementViewCommand = new RelayCommand(x => { CurrentView = StatementVM; });
            VideoViewCommand = new RelayCommand(x => { CurrentView = VideoVM; });

            //ModuleView = new ModuleView(this);
            //LocalMessageBoard = LocalHardwareStaticDetails._localMessageBoard;
            //FebrisMessageBoard = LocalHardwareStaticDetails._febrisMessageBoard;
            //UserList = LocalHardwareStaticDetails._userList;
            //ModuleList = LocalHardwareStaticDetails._moduleList;
            //SelectUser = new RelayCommand(X => { UserVM.SelectedUser = UserVM.DisplayedUser; });
        }

        private VideoViewModel VideoVMSetup()
        {
            PCFileManager _fileManager = new PCFileManager(null, null);
            VideoViewModel _vm = new VideoViewModel()
            {
                UnsentVideoFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.RecordingsFilePath),
                SentVideoFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.zipFolderPath)
            };
            return _vm;
        }

        private StatementViewModel StatementVMSetup()
        {
            PCFileManager _fileManager = new PCFileManager(null, null);
            StatementViewModel _vm = new StatementViewModel()
            {
                UnsentStatementFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.StatementPath),
                SentStatementFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.OldStatementPath)
            };
            return _vm;
        }

        private ConfigViewModel ConfigVMSetup()
        {
            ConfigSettings _configSettings = new ConfigSettings();
            PCDataProtection _dataProtection = new PCDataProtection(_log, _config);
            JObject settings = _configSettings.Get().Result;

            bool credsExist = false;
            string userName = string.Empty;
            string secret = string.Empty;
            (credsExist, userName, secret) = _dataProtection.GetCredentials().Result;

            ConfigViewModel output = new ConfigViewModel()
            {
                HardwareLicense = SetHardwareLicense(),
                CredentialsExist = credsExist,
                ConfigModel = ConfigLogic.GetSettings().Result,
                //ConfigModel = new ConfigModel()
                //    {
                //        DeveloperAccount = ,
                //        DomainPrefix = settings["domainprefix"]?.ToString() ?? string.Empty,
                //        Domain = settings,
                //        DomainPath = ,
                //        DomainPort = 
                //    }
            };

            if (output.ConfigModel != default)
            {
                string vis = output.SetVisablity();
                output.DeveloperAccountSelected = vis;
            }

            if (credsExist)
            {
                output.ConfigModel.UserName = userName;
                output.ConfigModel.Password = secret;
            }

            return output;
        }

        public string SetHardwareLicense()
        {
            try
            {
                // NODE-9. This returned a WMI-derived licence, which overwrote anything the
                // operator had entered and matched no row on the node (audit T9 mints the
                // credential and stores only its hash). Show what is actually stored, so a
                // registered device displays its real credential and an unregistered one shows
                // blank rather than a plausible value that will never authenticate.
                PCDataProtection dataProtection = new PCDataProtection();
                return dataProtection.GetDeviceCredential();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

    }


}
