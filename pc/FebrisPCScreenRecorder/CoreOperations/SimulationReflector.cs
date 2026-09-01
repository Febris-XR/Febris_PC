// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCScreenRecorderV3.CoreOperations
{
    class SimulationReflector
    {
        //private Microsoft.Extensions.Logging.ILogger _log;
        //private IConfiguration _config;
        private Serilog.ILogger _log;

        public SimulationReflector(Serilog.ILogger log)
        {
            _log = log;
        }

        //public SimulationReflector(Microsoft.Extensions.Logging.ILogger log, IConfiguration config)
        //{
        //    _log = log;
        //    _config = config;
        //}

        //private FebrisLocalLibrary.Utilites.UnityReflector unityReflector { get; }
        public bool IsSimulationRunning()
        {
            try
            {
                SharedServices.Launcher.SimulationReflector unityReflector = new SharedServices.Launcher.SimulationReflector();// _log);
                bool simulationIsRunning = unityReflector.SimulationIsRunning();//.SimulationRunning();

                _log.Information("reflector shows value :" + simulationIsRunning.ToString() + "\n");
                //Console.WriteLine("reflector shows value :"+ simulationIsRunning.ToString() + "\n");
                return simulationIsRunning;
            }
            catch
            {
                _log.Information("reflector failed and returns false" + "\n");
                //Console.WriteLine("reflector failed and returns false" + "\n");
                return false;
            }
        }
    }
}
