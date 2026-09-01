// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.MVVM.View;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class UserViewModel : ObservableObject
    {
        #region Added command user selection
        private object _currentSelectedUser;
        public object CurrentSelectedUser
        {
            get { return _currentSelectedUser; }
            set
            {
                _currentSelectedUser = value;
                SelectedUser = (HardwareUserViewModel)value;
                OnPropertyChange();
            }
        }

        public RelayCommand SelectUserCommand { get; set; }
        public UserViewModel()
        {
            CurrentSelectedUser = new HardwareUserViewModel();
            SelectUserCommand = new RelayCommand(x => { CurrentSelectedUser = DisplayedUser; });

        }
        #endregion


        private List<Cohort> _cohortList;
        public List<Cohort> CohortList
        {
            get { return _cohortList; }
            set { _cohortList = value; }
        }


        private HardwareUserViewModel _displayedUser;
        public HardwareUserViewModel DisplayedUser
        {
            get { return _displayedUser; }
            set
            {
                _displayedUser = value;
                OnPropertyChange();
            }
        }

        private List<HardwareUserViewModel> _userList;
        public List<HardwareUserViewModel> UserList
        {
            get { return _userList; }
            set
            {
                _userList = value;
                OnPropertyChange();
            }
        }

        private string _userSearch;
        public string UserSearch
        {
            get { return _userSearch; }
            set
            {
                _userSearch = value;
                GenerateSearchResults();
                OnPropertyChange();
            }
        }

        private List<HardwareUserViewModel> _searchResultList;
        public List<HardwareUserViewModel> SearchResultList
        {
            get { return _searchResultList; }
            set
            {
                _searchResultList = value;
                OnPropertyChange();
            }
        }

        private HardwareUserViewModel _selectedUser;
        public HardwareUserViewModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;        
                LocalHardwareStaticDetails.selectedUser = value;
                OnPropertyChange();
            }
        }

        private void GenerateSearchResults()
        {
            if (string.IsNullOrEmpty(UserSearch))
            {
                SearchResultList = UserList;
            }
            else
            {
                SearchResultList = UserList.Where(i => i.FirstName.ToLower().Contains(UserSearch.ToLower())
                || i.IdentificationNumber.ToLower().Contains(UserSearch.ToLower())
                || i.LastName.ToLower().Contains(UserSearch.ToLower())
                ).ToList();
            }

        }
        
    }
}
