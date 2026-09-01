// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.BusinessLogic;
using Febris.PCLauncherV3.Utilites;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class ConfigViewModel : ObservableObject
    {
        public RelayCommand SaveDomainCommand { get; set; }

        #region Added command user selection

        private object _currentConfigCommand;
        public object CurrentConfigCommand
        {
            get { return _currentConfigCommand; }
            set
            {
                _currentConfigCommand = value;
                //ConfigModel.DeveloperAccount = (bool)value;
                //ConfigModel.DomainPrefix = (string)value;
                //ConfigModel.Domain = (string)value;
                //ConfigModel.DomainPort = (string)value;
                //ConfigModel.DomainPath = (string)value;
                //Need to be able to save this so it will not overwrite itself each time. 
                SaveDomain(ConfigModel);
                
                OnPropertyChange();
            }
        }

        public ConfigViewModel()
        {
            //CurrentConfigCommand = string.Empty;
            SaveDomainCommand = new RelayCommand(x => { CurrentConfigCommand = ConfigModel; });            
        }


        #endregion

        #region Service status
        //private bool _uploaderRunning;
        //public bool UploaderRunning
        //{
        //    get { return _uploaderRunning; }
        //    set { _uploaderRunning = value; }
        //}
        //private bool _downloaderRunning;
        //public bool DownloaderRunning
        //{
        //    get { return _downloaderRunning; }
        //    set { _downloaderRunning = value; }
        //}
        #endregion

        #region normal vm properties
        private string _hardwareLicense;
        public string HardwareLicense
        {
            get { return _hardwareLicense; }
            set
            {
                _hardwareLicense = value;
                OnPropertyChange();
            }
        }

        private bool _domainVisible;
        public bool DomainVisible
        {
            get { return _domainVisible; }
            set
            {
                _domainVisible = value;
                OnPropertyChange();
            }
        }

        private bool _credentialsExist;
        public bool CredentialsExist
        {
            get { return _credentialsExist; }
            set {
                _credentialsExist = value;
                OnPropertyChange();
            }
        }

        private bool _developerAccount;
        public bool DeveloperAccount
        {
            get { return _developerAccount; }
            set
            {
                _developerAccount = value;
                ConfigModel.DeveloperAccount = value;
                DomainVisible = !value;
                OnPropertyChange();
            }
        }

        private ConfigModel _configModel;
        public ConfigModel ConfigModel
        {
            get { return _configModel; }
            set
            {
                _configModel = value;
                OnPropertyChange();
                //_developerAccount = value.DeveloperAccount;
            }
        }

        #endregion

        #region moved
        //private string _domainPrefix;
        //public string DomainPrefix
        //{
        //    get { return _domainPrefix; }
        //    set
        //    {
        //        _domainPrefix = value;
        //        OnPropertyChange();
        //    }
        //}

        //private string _emailAddress;
        //public string EmailAddress
        //{
        //    get { return _emailAddress; }
        //    set { _emailAddress = value; OnPropertyChange(); }
        //}

        //private string _password;
        //public string Password
        //{
        //    get { return _password; }
        //    set { _password = value; OnPropertyChange(); }
        //}
        #endregion

        private string _developerAccountSelected;
        public string DeveloperAccountSelected
        {
            get { return _developerAccountSelected; }
            set {
                _developerAccountSelected = value;
                //_developerAccountSelected = SetVisablity();
                OnPropertyChange();
            }
        }

        public string SetVisablity()
        {
            if (ConfigModel.DeveloperAccount)
            {
                return "Collapsed";
            }
            else
            { 
                return "Visible"; 
            }
        }

        private void SaveDomain(ConfigModel configModel)
        {
            try
            {
                ConfigSettings _configSettings = new ConfigSettings();// _log, _config);
                //bool complete = _configSettings.SetDomainPrefix(configModel.ToLower()).Result;
                bool complete = ConfigLogic.SaveSettings(ConfigModel).Result;

                // NODE-9. The Hardware License box is where the operator pastes the credential the
                // node minted at registration. It used to be display-only, filled from a WMI-derived
                // value and never persisted, so nothing typed here survived a restart or reached the
                // node. Save it encrypted alongside the user credentials.
                PCDataProtection dataProtection = new PCDataProtection();
                dataProtection.SetDeviceCredential(HardwareLicense);

                string vis = SetVisablity();
                DeveloperAccountSelected = vis;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }
    }

    public class ConfigModel
    {
        public ConfigModel()
        {
            DeveloperAccount = false;
            Domain = string.Empty;
            DomainPrefix = string.Empty;
            DomainPort = string.Empty;
            DomainPath = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
        }

        //public string Name { get; set; }
        //public string UniqueIdentifier { get; set; }

        #region domain breakdown
        public bool DeveloperAccount { get; set; }
        public string Domain { get; set; }
        public string DomainPrefix { get; set; }
        public string DomainPort { get; set; }
        public string DomainPath { get; set; }
        #endregion

        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
