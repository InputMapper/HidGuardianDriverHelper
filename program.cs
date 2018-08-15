using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverHelper
{
    class Program
    {
        private static string DevconPath { get
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x64\devcon.exe")))
                {
                    return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x64\devcon.exe");
                }
                else if (File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x86\devcon.exe")))
                {
                    return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x64\devcon.exe");
                }
                else return string.Empty;
            }
        }

        private static string InfPath
        {
            get
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x64\HidGuardian.inf")))
                {
                    return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x64\HidGuardian.inf");
                }
                else if (File.Exists(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x86\HidGuardian.inf")))
                {
                    return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"x86\HidGuardian.inf");
                }
                else return string.Empty;
            }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("DriverHelper");
        static void Main(string[] args)
        {
            log.InfoFormat("Working Directory: {0}", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            string parameter = string.Concat(args);
            switch (parameter)
            {
                case "--install":
                    Install();
                    break;
                case "--uninstall":
                    Uninstall();
                    break;
                default:
                    Console.WriteLine("Install or remove the HidGuardian filter driver.");
                    Console.WriteLine("");
                    Console.WriteLine("DriverHelper [--install][--uninstall]");
                    Console.WriteLine("");
                    Console.WriteLine("Results of command are stored in DriverHelper.log");
                    break;
            }
        }

        private static void Uninstall()
        {
            log.Info("Uninstall begining");
            if (DevconPath == string.Empty)
            {
                log.Error("Driver install failed, Devcon not found.");
                Environment.Exit(2);
            }
            else
            {
                log.InfoFormat("Devcon location set as {0}", DevconPath);
            }

            Process FilterUninstallProcess = new Process();
            FilterUninstallProcess.StartInfo.UseShellExecute = false;
            FilterUninstallProcess.StartInfo.RedirectStandardOutput = true;
            FilterUninstallProcess.StartInfo.FileName = DevconPath;
            FilterUninstallProcess.StartInfo.Arguments = @"classfilter HIDClass upper !HidGuardian";

            int exitCode = 0;
            string FilterOutput = string.Empty;
            while (exitCode == 0)
            {
                FilterUninstallProcess.Start();
                FilterOutput = FilterUninstallProcess.StandardOutput.ReadToEnd();
                FilterUninstallProcess.WaitForExit();

                exitCode = FilterUninstallProcess.ExitCode;
            }

            log.InfoFormat("Filter remobal returned: "+Environment.NewLine+"{0}", FilterOutput);


            Process DriverUninstallProcess = new Process();
            DriverUninstallProcess.StartInfo.UseShellExecute = false;
            DriverUninstallProcess.StartInfo.RedirectStandardOutput = true;
            DriverUninstallProcess.StartInfo.FileName = DevconPath;
            DriverUninstallProcess.StartInfo.Arguments = @"remove Root\HidGuardian";

            DriverUninstallProcess.Start();
            string output = DriverUninstallProcess.StandardOutput.ReadToEnd();
            DriverUninstallProcess.WaitForExit();

            if (DriverUninstallProcess.ExitCode != 0)
            {
                log.ErrorFormat("Driver uninstall failed, returned code {0}: " + Environment.NewLine + "{1}", DriverUninstallProcess.ExitCode, output);
                Environment.Exit(22);
            }
            else
            {
                log.InfoFormat("Driver uninstall success: " + Environment.NewLine + "{0}", output);
            }

        }

        private static void Install()
        {
            log.Info("Install begining");
            if (DevconPath == string.Empty)
            {
                log.Error("Driver install failed, Devcon not found.");
                Environment.Exit(2);
            } else
            {
                log.InfoFormat("Devcon location set as {0}", DevconPath);
            }

            if (InfPath == string.Empty)
            {
                log.Error("Driver install failed, Inf not found.");
                Environment.Exit(2);
            }
            else
            {
                log.InfoFormat("Inf location set as {0}", InfPath);
            }

            Process DriverInstallProcess = new Process();
            DriverInstallProcess.StartInfo.UseShellExecute = false;
            DriverInstallProcess.StartInfo.RedirectStandardOutput = true;
            DriverInstallProcess.StartInfo.FileName = DevconPath;
            DriverInstallProcess.StartInfo.Arguments = string.Format(@"install ""{0}"" Root\HidGuardian", InfPath);

            DriverInstallProcess.Start();
            string output = DriverInstallProcess.StandardOutput.ReadToEnd();
            DriverInstallProcess.WaitForExit();

            if (DriverInstallProcess.ExitCode != 0)
            {
                log.ErrorFormat("Driver install failed, returned code {0}: " + Environment.NewLine + "{1}", DriverInstallProcess.ExitCode, output);
                Environment.Exit(22);
            } else
            {
                log.InfoFormat("Driver install success: " + Environment.NewLine + "{0}", output);
            }

            Process FilterInstallProcess = new Process();
            FilterInstallProcess.StartInfo.UseShellExecute = false;
            FilterInstallProcess.StartInfo.RedirectStandardOutput = true;
            FilterInstallProcess.StartInfo.FileName = DevconPath;
            FilterInstallProcess.StartInfo.Arguments = @"classfilter HIDClass upper -HidGuardian";

            FilterInstallProcess.Start();
            string FilterOutput = FilterInstallProcess.StandardOutput.ReadToEnd();
            FilterInstallProcess.WaitForExit();

            if (FilterInstallProcess.ExitCode != 0 && FilterInstallProcess.ExitCode != 1)
            {
                log.ErrorFormat("Filter install failed, returned code {0}: " + Environment.NewLine + "{1}", FilterInstallProcess.ExitCode, FilterOutput);
                Environment.Exit(22);
            }
            else
            {
                log.InfoFormat("Filter install success: " + Environment.NewLine + "{0}", FilterOutput);
            }


        }

    }
}
