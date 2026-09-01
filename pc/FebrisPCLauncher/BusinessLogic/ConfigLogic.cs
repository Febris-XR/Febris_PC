// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.DataLogic;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.PCLauncherV3.Utilities;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCLauncherV3.BusinessLogic
{
    public class ConfigLogic
    {
        ConfigContext _context = new ConfigContext();

        //public static async Task SaveSettings()
        //{
        //    try
        //    {
        //        ConfigModel newData = LocalHardwareStaticDetails.StaticMainVM.ConfigVM.ConfigModel;
        //        ConfigContext _context = new ConfigContext();
        //        ConfigModel data = await _context.Get() ?? new ConfigModel();

        //        data.DeveloperAccount = newData.DeveloperAccount;
        //        if (!newData.DeveloperAccount)
        //        {
        //            data.Domain = newData.Domain;
        //            data.DomainPrefix = newData.DomainPrefix;
        //            data.DomainPort = newData.DomainPort;
        //            data.DomainPath = newData.DomainPath;
        //        }

        //        if (!string.IsNullOrEmpty(newData.UserName))
        //        {
        //            data.UserName = newData.UserName;
        //        }
        //        if (!string.IsNullOrEmpty(newData.Password))
        //        {
        //            data.Password = newData.Password;
        //        }

        //        bool compelete = await _context.Post(newData);

        //        if (compelete)
        //        {
        //            URLSettingUtility.SetURL();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        internal static async Task<bool> SaveSettings(ConfigModel configModel)
        {            
            try
            {                
                ConfigContext _context = new ConfigContext();
                ConfigModel data = await _context.Get() ?? new ConfigModel();

                data.DeveloperAccount = configModel.DeveloperAccount;
                if (!configModel.DeveloperAccount)
                {
                    data.Domain = configModel.Domain;
                    data.DomainPrefix = configModel.DomainPrefix;
                    data.DomainPort = configModel.DomainPort;
                    data.DomainPath = configModel.DomainPath;
                }

                if (!string.IsNullOrEmpty(configModel.UserName))
                {
                    data.UserName = configModel.UserName;
                }
                if (!string.IsNullOrEmpty(configModel.Password))
                {
                    data.Password = configModel.Password;
                }

                // FIX (PC-B8): Post the merged object (data), not the raw input, so a blank UserName or Password preserves the stored value. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                // bool compelete = await _context.Post(configModel);
                bool compelete = await _context.Post(data);

                if (compelete)
                {
                    URLSettingUtility.SetURL();
                }
                return compelete;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public static async Task<ConfigModel> GetSettings()
        {
            try
            {
                ConfigContext _context = new ConfigContext();
                ConfigModel data = await _context.Get() ?? new ConfigModel();
                //if (data == default)
                //{
                //    data = new ConfigModel();
                //}
                return data;
                //LocalHardwareStaticDetails.StaticMainVM.ConfigVM.ConfigModel = data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new ConfigModel();
        }

        
    }
}
