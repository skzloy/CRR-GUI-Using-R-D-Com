using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace WPF_CRR_Candie_Using_R_D_COM
{
    public class CheckStart
    {
        public static bool IsProgramInstalled(string p_program_name, bool x86Platform)
        {
            bool result = false;

            string uninstallKey = string.Empty;

            if (x86Platform)
            {
                uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            else
            {
                uninstallKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            }

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {

                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if (sk.GetValue("DisplayName") != null && sk.GetValue("DisplayName").ToString().ToUpper().Equals(p_program_name.ToUpper()))
                        {
                            result = true; //exists
                        }

                    }
                }
            }


            return result;
        }
    }
}
