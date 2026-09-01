// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
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
    /// Interaction logic for StatementView.xaml
    /// </summary>
    public partial class StatementView : UserControl
    {
        //public readonly PCFileManager _fileManager;
        //public StatementViewModel _vm { get =>MainViewModel.StatementViewModel; set; }
        public StatementView()
        {
            InitializeComponent();
            //_fileManager = new PCFileManager(null, null);
            //_vm = (StatementViewModel)this._vm;
            //_vm = ((StatementViewModel)(this.DataContext));
            //PopulateList();
        }

        //public void PopulateList()
        //{
        //    try
        //    {
        //        GetSentList();
        //        GetUnsentList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}

        //private void GetUnsentList()
        //{
        //    try
        //    {
        //        _vm.UnsentStatementFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.StatementPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}

        //private void GetSentList()
        //{
        //    try
        //    {
        //        _vm.SentStatementFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.OldStatementPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}
    }
}
