// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.LauncherModels;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Utilities;
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
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : UserControl
    {        
        public UserView()
        {
            InitializeComponent();           
        }

        //private void UserSearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    try
        //    {

        //        //MainViewModel.UserVM.UserSearch = UserSearchFilter.Text.ToLower();

        //        ////add first and last name to filter
        //        UserSelectBox.Items.Filter = delegate (object obj)
        //        {
        //            HardwareUserViewModel user = (HardwareUserViewModel)obj;
        //            string str = user.LastName.ToLower();
        //            if (string.IsNullOrEmpty(str)) return false;
        //            //int index = str.IndexOf(MainViewModel.UserVM.UserSearch, 0);
        //            int index = str.IndexOf(UserSearchFilter.Text, 0);
        //            return (index > -1);
        //        };


        //        //MainViewModel.UserVM.UserSearch = UserSearchFilter.Text.ToLower();

        //        //////add first and last name to filter
        //        //UserSelectBox.Items.Filter = delegate (object obj)
        //        //{
        //        //    HardwareUserViewModel user = (HardwareUserViewModel)obj;
        //        //    string str = user.LastName.ToLower();
        //        //    if (string.IsNullOrEmpty(str)) return false;
        //        //    int index = str.IndexOf(MainViewModel.UserVM.UserSearch, 0);
        //        //    return (index > -1);
        //        //};
        //    }
        //    catch (Exception ex)
        //    {
        //        //_log.LogError(ex.Message);
        //        //throw;
        //    }
        //}


        //private void SelectUser_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        //MainViewModel.UserVM.SelectedUser = MainViewModel.UserVM.DisplayedUser;               
        //    }
        //    catch (Exception ex)
        //    {
        //        //_log.LogError(ex.Message);
        //        throw;
        //    }
        //}




    }

    //public class SelectUserCommand : RelayCommand
    //{
    //    private readonly HardwareUserViewModel _selectedUser;

    //    public SelectUserCommand(HardwareUserViewModel selectedUser)
    //    {
    //        _selectedUser = selectedUser;


    //    }

    //    public override void Execute(object parameter)
    //    {
    //        LocalHardwareStaticDetails.selectedUser = _selectedUser;
    //    }

    //}
}
