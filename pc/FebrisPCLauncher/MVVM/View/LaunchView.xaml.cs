// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Operations;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Febris.PCLauncherV3.MVVM.View
{
    /// <summary>
    /// Interaction logic for LaunchView.xaml
    /// </summary>
    public partial class LaunchView : UserControl
    {
        //var _context = MainViewModel.LaunchVM.UserSelected
        public LaunchView()
        {
            InitializeComponent();
            //CheckIfNeededRequirementsAreMet();
        }

        //private void CheckIfNeededRequirementsAreMet()
        //{

        //    if (LocalHardwareStaticDetails.selectedUser != null)
        //    {
        //        //MainViewModel.LaunchVM.UserSelected = true;
        //    }
        //    if (LocalHardwareStaticDetails.selectedModule != null)
        //    {
        //        //MainViewModel.LaunchVM.ModuleSelected = true;
        //    }
        //}

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Launcher _launcher = new Launcher(null, null);
                _launcher.StartSimulation(LocalHardwareStaticDetails.selectedUser, LocalHardwareStaticDetails.selectedModule);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
