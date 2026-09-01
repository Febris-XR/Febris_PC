// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.EnumLibrary;
using Febris.PCLauncherV3.Utilites;
using System;
using System.Collections.Generic;
using System.Text;
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
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using Febris.PCLauncherV3.Operations;

namespace Febris.PCLauncherV3.MVVM.View
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : UserControl
    {
        //public MainViewModel _mainViewModel { get; set; }
        //private MainViewModel _vm;
        private readonly ServiceUtilities _serviceUtilities;

        public ConfigView()
        {
            InitializeComponent();
            _serviceUtilities = new ServiceUtilities(null,null);// _log, _config);
        }

        private void ServiceRestartCommand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serviceUtilities.ServiceRestarter(ServiceOptions.Downloader);
                _serviceUtilities.ServiceRestarter(ServiceOptions.Uploader);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                // FIX (PC-B9): log and continue instead of rethrowing to avoid crashing the launcher. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                // throw;
            }
        }



        private void SaveCredentials_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PCDataProtection _dataProtection = new PCDataProtection();
                _dataProtection.SetCredentials(EmailAddress.Text, PasswordInput.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                // FIX (PC-B9): log and continue instead of rethrowing to avoid crashing the launcher. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                // throw;
            }
        }

        private void OpenPortal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Launcher _launcher = new Launcher(null, null);
                _launcher.LaunchPortal();//.SetCredentials(EmailAddress.Text, PasswordInput.Password);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                // FIX (PC-B9): log and continue instead of rethrowing to avoid crashing the launcher. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                // throw;
            }
        }



        //private void SavePrefix_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string domainPrefix = DomainPrefixInput.Text.ToLower();
        //        ConfigSettings _configSettings = new ConfigSettings();// _log, _config);
        //        bool complete = _configSettings.SetDomainPrefix(domainPrefix).Result;

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.StackTrace);
        //        throw;
        //    }
        //}
    }
}
