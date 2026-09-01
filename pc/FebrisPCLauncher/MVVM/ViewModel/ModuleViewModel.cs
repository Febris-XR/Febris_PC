// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class ModuleViewModel : ObservableObject
    {
        #region Added command user selection
        private object _currentSelectedModule;

        public object CurrentSelectedModule
        {
            get { return _currentSelectedModule; }
            set
            {
                _currentSelectedModule = value;
                SelectedModule = (Module)value;
                OnPropertyChange();
            }
        }
        public RelayCommand SelectModuleCommand { get; set; }
        public ModuleViewModel()
        {
            CurrentSelectedModule = new Module();
            SelectModuleCommand = new RelayCommand(x => { CurrentSelectedModule = DisplayedModule; });
        }
        #endregion
        //private Module _selectedModule;
        //public Module SelectedModule
        //{
        //    get { return _selectedModule; }
        //    set
        //    {
        //        _selectedModule = value;
        //        //OnPropertyChange();
        //    }
        //}

        private Module _displayedModule;
        public Module DisplayedModule
        {
            get { return _displayedModule; }
            set
            {
                _displayedModule = value;
                OnPropertyChange();
            }
        }
        private Module _selectedModule;
        public Module SelectedModule
        {
            get { return _selectedModule; }
            set
            {
                _selectedModule = value;
                LocalHardwareStaticDetails.selectedModule = value;
                OnPropertyChange();
            }
        }

        private List<Module> _moduleList;
        public List<Module> ModuleList
        {
            get { return _moduleList; }
            set
            {
                _moduleList = value;
              OnPropertyChange();
            }
        }
        private List<Module> _searchResultList;
        public List<Module> SearchResultList
        {
            get { return _searchResultList; }
            set
            {
                _searchResultList = value;
                OnPropertyChange();
            }
        }

        private string _moduleSearch;
        public string ModuleSearch
        {
            get { return _moduleSearch; }
            set
            {
                _moduleSearch = value;
                GenerateSearchResults();
                OnPropertyChange();
            }
        }

        private void GenerateSearchResults()
        {
            if (string.IsNullOrEmpty(ModuleSearch))
            {
                SearchResultList = ModuleList;
            }
            else
            {
                SearchResultList = ModuleList.Where(i => i.Name.ToLower().Contains(ModuleSearch.ToLower())               
                ).ToList();
            }

        }
    }
}
