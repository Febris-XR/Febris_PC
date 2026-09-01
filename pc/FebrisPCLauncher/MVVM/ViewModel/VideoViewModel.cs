// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class VideoViewModel : ObservableObject
    {
        private List<string> _unsentVideoFileList;

        public List<string> UnsentVideoFileList
        {
            get { return _unsentVideoFileList; }
            set { _unsentVideoFileList = value; 
                OnPropertyChange(); }
        }


        private List<string> _sentVideoFileList;

        public List<string> SentVideoFileList
        {
            get { return _sentVideoFileList; }
            set { _sentVideoFileList = value; 
                OnPropertyChange(); }
        }


    }
}
