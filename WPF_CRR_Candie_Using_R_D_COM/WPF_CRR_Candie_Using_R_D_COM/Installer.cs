using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WPF_CRR_Candie_Using_R_D_COM
{
    class Installer
    {
        public static bool RunInstallMSI(string pathExternalInstaller)
        {
            try
            {
                Console.WriteLine("Starting to install application");
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = pathExternalInstaller;
                process.StartInfo.Arguments = "/SILENT";
                process.Start();

                
                process.WaitForExit();
                Console.WriteLine("Application installed successfully!");
                return true; //Return True if process ended successfully
            }
            catch
            {
                Console.WriteLine("There was a problem installing the application!");
                return false;  //Return False if process ended unsuccessfully
            }
        }

        public static bool RunUninstallMSI(string guid)
        {
            try
            {
                Console.WriteLine("Starting to uninstall application");
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", string.Format("/c start /MIN /wait msiexec.exe /x {0} /quiet", guid));
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process process = Process.Start(startInfo);
                process.WaitForExit();
                Console.WriteLine("Application uninstalled successfully!");
                return true; //Return True if process ended successfully
            }
            catch
            {
                Console.WriteLine("There was a problem uninstalling the application!");
                return false; //Return False if process ended unsuccessfully
            }
        }

    }
}
