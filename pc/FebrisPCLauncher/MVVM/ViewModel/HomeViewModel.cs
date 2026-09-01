// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.ModelLibrary.Models.DataModels;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;


namespace Febris.PCLauncherV3.MVVM.ViewModel
{

    public class HomeViewModel : ObservableObject
    {

        private List<MessageBoard> _localMessageBoard;
        public List<MessageBoard> LocalMessageBoard
        {
            get { return _localMessageBoard; }
            set
            {
                _localMessageBoard = value;
                OnPropertyChange();
            }
        }


        private List<AdminMessageBoard> _febrisMessageBoard;
        public List<AdminMessageBoard> FebrisMessageBoard
        {
            get { return _febrisMessageBoard; }
            set
            {
                _febrisMessageBoard = value;
                OnPropertyChange();
            }
        }
     

    }
}
