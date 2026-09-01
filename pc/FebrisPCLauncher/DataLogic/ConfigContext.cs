// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCLauncherV3.MVVM.ViewModel;
using Febris.SharedServices.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Febris.PCLauncherV3.DataLogic
{
    public class ConfigContext
    {
        private PCFileManager _fileManager = new PCFileManager();

        internal async Task<ConfigModel> Get()
        {
            ConfigModel output = new ConfigModel();
            try
            {
                string preoutput = _fileManager.GetFileContent(PCFileSystem.ConfigLocation, string.Empty);// "Config.json");
                output = JsonConvert.DeserializeObject<ConfigModel>(preoutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return output;
        }

        internal async Task<bool> Post(ConfigModel input)
        {
            bool output = false;
            try
            {
                string data = JsonConvert.SerializeObject(input);
                //string path = Path.Combine(FileSystem.ConfigLocation, "Config.json");
                output = _fileManager.Set(data, PCFileSystem.ConfigLocation);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return output;
        }



    }
}
