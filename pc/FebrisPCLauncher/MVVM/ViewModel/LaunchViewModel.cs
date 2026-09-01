// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Febris.PCLauncherV3.MVVM.ViewModel
{
    public class LaunchViewModel :ObservableObject
    {
        #region Added command user selection
        private object _currentCommand;
        public object CurrentCommand
        {
            get { return _currentCommand; }
            set
            {
                _currentCommand = value;                
                OnPropertyChange();
            }
        }

        public RelayCommand SelectUserCommand { get; set; }
        public RelayCommand SelectModuleCommand { get; set; }
        //public RelayCommand SelectRecordCommand { get; set; }

        public LaunchViewModel()
        {
            UserSelected = UserHasBeenSelected();//(LocalHardwareStaticDetails.selectedUser==default);
            ModuleSelected = ModuleHasBeenSelected();// (LocalHardwareStaticDetails.selectedModule==default);
            //CurrentCommand = new HardwareUserViewModel();
            //SelectUserCommand = new RelayCommand(x => { CurrentCommand = UserSelected; });
            //SelectModuleCommand = new RelayCommand(x => { CurrentCommand = ModuleSelected; });
            //SelectRecordCommand = new RelayCommand(x => { CurrentCommand = RecordSession; });
        }

       
        #endregion

        // ROADMAP 22: the RecordSession property that sat here is GONE with the checkbox that
        // bound to it. It was the only writer of LocalHardwareStaticDetails.recordSession, and its
        // only reader was the launcher gate this same change replaced, so the static went too.
        // The record decision is the node's, derived from the educator's per-cohort policy and
        // delivered as the statement's video attachment.
        //
        // HISTORY, corrected 2026-08-25 after the owner challenged the original write-up. This
        // checkbox is the V3 remnant of a mechanism that DID work in V2, where the server decided
        // (Module.IsTest AND the institution's VideoStorageOption) and the learner's opt-in was a
        // second, narrower gate on top of it. The V2-to-V3 rewrite broke both ends at once. If a
        // learner-facing opt-in is ever wanted again it is an ADDITION to the educator policy, not
        // a replacement for it, and it belongs here.


        private bool _userSelected;
        public bool UserSelected
        {
            get { return _userSelected; }
            set { _userSelected = value; 
                OnPropertyChange(); }
        }


        private bool _moduleSelected;
        public bool ModuleSelected
        {
            get { return _moduleSelected; }
            set { 
                _moduleSelected = value; 
                OnPropertyChange(); 
            }
        }

        private bool UserHasBeenSelected()
        {
            bool output = false;
            if (LocalHardwareStaticDetails.selectedUser.ActorId != default)
            {
                output = true;
            }
            return output;
        }
        private bool ModuleHasBeenSelected()
        {
            bool output = false;
            if (LocalHardwareStaticDetails.selectedModule.UUID != default)
            {
                output = true;
            }
            return output;
        }

        //private bool SetToRecord()
        //{
        //    bool output = false;
        //    if (LocalHardwareStaticDetails.selectedModule.UUID != default)
        //    {
        //        output = true;
        //    }
        //    return output;
        //}
    }
}
