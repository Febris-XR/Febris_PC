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
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView : UserControl
    {
        public readonly PCFileManager _fileManager;
        public VideoView()
        {
            InitializeComponent();
            //_fileManager = new PCFileManager(null,null);
            //PopulateList();
        }

        //public void PopulateList()
        //{
        //    try
        //    {
        //        GetSentVideoList();
        //        GetUnsentVideoList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}

        //private void GetUnsentVideoList()
        //{
        //    try
        //    {
        //        //MainViewModel.VideoVM.UnsentVideoFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.RecordingsFilePath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}

        //private void GetSentVideoList()
        //{
        //    try
        //    {
        //        //MainViewModel.VideoVM.SentVideoFileList = _fileManager.GetDirectoryContentNames(PCFileSystem.zipFolderPath);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine();
        //        throw;
        //    }
        //}
    }
}
