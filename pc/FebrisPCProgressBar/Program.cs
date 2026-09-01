// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Threading;

namespace Febris.ConsoleProgressBar
{
    public class Program
    {
        public static void Main(string[] args)
        {            
            try
            {
                string title = string.Empty;
                string status = string.Empty;
                // NOTE (PC-B2): cannot mark a local volatile (compiler error), and the loop is designed to run until the parent process kills this child via StopProgressBar, so changing the loop exit would alter intended functionality. Deferred per do-not-change-functionality. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                bool running = true;
                string[] arguments = Environment.GetCommandLineArgs();
                foreach (var arg in arguments)
                {                    
                    var varArray = arg.Split("|");
                    switch (varArray[0])
                    {
                        case "Title":
                            title = varArray[1];
                            break;
                        case "Status":
                            status = varArray[1];
                            break;
                    }
                }
                Console.WriteLine(title + " is " + status);
                using (var progress = new ProgressBar())
                {
                    // NOTE (PC-B2): this loop intentionally runs until the parent kills this process via ProgressBarService.StopProgressBar. Robustly stopping it (volatile flag, Kill plus Dispose on the parent side) spans ProgressBarService.cs and changes process-lifecycle behavior, so it is deferred here. Deferred per do-not-change-functionality. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                    while (running != false)
                    {
                        for (int i = 0; i <= 100; i++)
                        {
                            progress.Report((double)i / 100);
                            Thread.Sleep(20);
                        }
                    }
                }
                Console.WriteLine("Done.");                
            }
            catch
            {
                Console.WriteLine("An Error Occured with this progress bar graphic");
                // FIX (PC-B2): removed headless Console.ReadKey() that blocks this launched process forever on exception, leaving a zombie. See docs/MODERNIZATION/PC_MODERNIZATION.md.
                // Console.ReadKey();
            }
        }
    }
}
