// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.PCStatementManagerV3.APIRequests;
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Febris.PCStatementManagerV3.Services
{
    class StatementFolderChecker
    {
        private ILogger _log;       
        private IConfiguration _config;
        private readonly JSONHandler _jSONHandler;
        private readonly PCFileManager _fileManager;
        private readonly StatementRequest _statementRequest;

        public StatementFolderChecker(ILogger log)
        {
            _log = log;            
            _jSONHandler = new JSONHandler(_log);
            _fileManager = new PCFileManager(_log, _config);
            _statementRequest = new StatementRequest(_log, _config);

        }

        public object ProcessFiles()
        {
            bool output = false;
            try
            {
                var unsentStatementList = _fileManager.GetDirectoryContentNames(PCFileSystem.StatementPath);
                if(unsentStatementList.Count() > 0)
                {
                    foreach(var i in unsentStatementList)
                    {
                        //gather file content
                        string tempFile = _fileManager.GetFileContent(PCFileSystem.StatementPath, i);
                        //send it
                        bool sendComplete = _statementRequest.UploadStatement(tempFile).Result;
                        if (!sendComplete)
                        {
                            sendComplete = _statementRequest.UploadStatementBackup(tempFile).Result;
                        }


                        if (sendComplete)
                        {
                            _fileManager.MoveStatementFileToSent(i);
                        }
                    }

                }
                
            }
            catch (Exception e)
            {
                _log.LogInformation(e.Message);
            }
            return output;
        }
    }
}
