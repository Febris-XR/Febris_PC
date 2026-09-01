// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class StatementViewModel : ObservableObject
    {
        private List<string> _unsentStatementFileList;

        public List<string> UnsentStatementFileList
        {
            get { return _unsentStatementFileList; }
            set { _unsentStatementFileList = value; OnPropertyChange(); }
        }


        private List<string> _sentStatementFileList;

        public List<string> SentStatementFileList
        {
            get { return _sentStatementFileList; }
            set { _sentStatementFileList = value; OnPropertyChange(); }
        }
    }
}
