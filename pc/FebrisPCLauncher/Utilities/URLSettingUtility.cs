// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.BusinessLogic;
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.SharedServices.Launcher;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCLauncherV3.Utilities
{
    public class URLSettingUtility
    {
        public static void SetURL()
        {
            ConfigModel _configModel = ConfigLogic.GetSettings().Result;

            if (_configModel.DeveloperAccount)
            {
                LocalHardwareStaticDetails.ApiUrl = LocalHardwareStaticDetails.DeveloperUrl;
            }
            else
            {
                string prefix = _configModel.DomainPrefix ?? string.Empty;
                string domain = _configModel.Domain ?? string.Empty;
                string port = _configModel.DomainPort ?? string.Empty;
                string path = _configModel.DomainPath ?? string.Empty;
                string newUrl = string.Empty;
                if (!prefix.Contains("https://"))
                {
                    newUrl = "https://";
                }

                if (!string.IsNullOrEmpty(prefix)&&prefix!="https://")
                {
                    newUrl += prefix + ".";
                }


                if (!string.IsNullOrEmpty(domain))
                {
                    newUrl += domain;
                }

                if (!string.IsNullOrEmpty(port))
                {
                    newUrl += ":" + port;
                }
                if (!string.IsNullOrEmpty(path))
                {
                    newUrl += "/" + path + "/";
                }

                LocalHardwareStaticDetails.ApiUrl = newUrl;
            }
        }
    }
}
