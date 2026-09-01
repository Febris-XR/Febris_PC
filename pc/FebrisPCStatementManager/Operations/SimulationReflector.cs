// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using Febris.SharedServices.Launcher;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Febris.PCStatementManagerV3.Operations
{
    class SimulationReflector
    {
        private ILogger _log;
        private readonly SharedServices.Launcher.SimulationReflector _simulationReflector;

        public SimulationReflector(ILogger log)
        {
            _log = log;
            _simulationReflector = new SharedServices.Launcher.SimulationReflector(_log);
        }

        public bool IsSimulationRunning()
        {
            //FebrisLocalLibrary.Utilites.SimulationReflector simulationReflector = new FebrisLocalLibrary.Utilites.SimulationReflector();
            //bool simulationRunning = simulationReflector.SimulationIsRunning();
            bool simulationRunning = _simulationReflector.SimulationIsRunning();
            return simulationRunning;
        }
    }
}
