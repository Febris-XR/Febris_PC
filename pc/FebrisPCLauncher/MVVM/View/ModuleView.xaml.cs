// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.MVVM.ViewModel;
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
    /// Interaction logic for ModuleView.xaml
    /// </summary>
    public partial class ModuleView : UserControl
    {
        //ModuleViewModel _context { get; set; }
        public ModuleView()//MainViewModel mainViewModel)
        {
            InitializeComponent();
            //_mainViewModel = mainViewModel;
            //_context = MainViewModel;
            //_context = DataContext;
            //_context.ModuleList = LocalHardwareStaticDetails._moduleList;

        }

        public void ModuleSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //MainViewModel.ModuleVM.ModuleSearch = ModuleSearchFilter.Text.ToLower();

                //This will need to be changed. 
                //ModuleSelectBox.Items.Filter = delegate (object obj)
                //{
                //    //set up object
                //    Module Test = (Module)obj;
                //    //set list of information into a string
                //    string str = Test.Name.ToString().ToLower();
                //    //if it is null just ignore it
                //    if (string.IsNullOrEmpty(str)) return false;
                //    //make the new index?
                //    int index = str.IndexOf(MainViewModel.ModuleVM.ModuleSearch, 0);
                //    return (index > -1);
                //};
            }
            catch (Exception ex)
            {
               // _log.LogError(ex.Message);
            }
        }

        private void SelectModule_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                //MainViewModel.ModuleVM.SelectedModule = MainViewModel.ModuleVM.DisplayedModule;                
            }
            catch (Exception ex)
            {
                //_log.LogError(ex.Message);
                throw;
            }
        }
    }
}
