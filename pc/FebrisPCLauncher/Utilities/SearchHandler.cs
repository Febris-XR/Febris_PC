// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.LauncherModels;
using Febris.ModelLibrary.Models.DataModels;
using Febris.ModelLibrary.ViewModels;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Febris.PCLauncherV3.Utilites
{
    class SearchHandler
    {
        private readonly ILogger<SearchHandler> _log;
        private readonly IConfiguration _config;

        public SearchHandler(ILogger<SearchHandler> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        //public object ModuleSearch(object obj)
        //{
        //    try
        //    {
        //        //set up object
        //        Module eduList = (Module)obj;
        //        //set list of information into a string
        //        string str = eduList.Name.ToString().ToLower();
        //        //if it is null just ignore it
        //        if (String.IsNullOrEmpty(str)) return false;
        //        //make the new index?
        //        int index = str.IndexOf(LocalHardwareStaticDetails.ModuleSearch, 0);

        //        return (index > -1);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //        return null;
        //    }
        //}

        //public object UserSearch(object obj)
        //{
        //    try
        //    {
        //        HardwareUserViewModel healthCareProfessional = (HardwareUserViewModel)obj;
        //        string str = healthCareProfessional.FirstName.ToString().ToLower();
        //        if (string.IsNullOrEmpty(str)) return false;
        //        int index = str.IndexOf(LocalHardwareStaticDetails.UserSearch, 0);
        //        return (index > -1);
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.LogError(ex.Message);
        //        return null;
        //    }
        //}

        public void UserFromTextBox()
        {
            try
            {
                //LocalHardwareStaticDetails.selectedUser = LocalHardwareStaticDetails._userList
                //    .Where(i => i.IdentificationNumber.ToLower() == ((MainWindow)Application.Current.MainWindow)
                //    .UserSearchFilter.Text)
                //    .Single();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

    }
}
