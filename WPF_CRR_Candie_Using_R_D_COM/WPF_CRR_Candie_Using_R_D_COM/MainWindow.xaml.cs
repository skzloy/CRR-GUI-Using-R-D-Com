using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StatConnectorCommonLib;
using STATCONNECTORSRVLib;
using System.IO;
using AGD;

namespace WPF_CRR_Candie_Using_R_D_COM
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StatConnector rConn;
        string path_to_r_project;

        public MainWindow()
        {
            InitializeComponent();
            string exec_folder = System.IO.Directory.GetCurrentDirectory();
            path_to_r_project = @"C:\Program Files\R\R-2.11.1";
            Environment.SetEnvironmentVariable("PATH", path_to_r_project + @"\bin");
            //FOO2();
        }

        private void FOO()
        {
            try
            {
                StatConnector rConn = new StatConnector();

                rConn.Init("R");

                rConn.SetSymbol("n1", 20);
                rConn.Evaluate(@"x1 <- rnorm(n1)");
                var o = rConn.GetSymbol("x1");

                foreach (double d in o)
                    AddStringToLog(d.ToString());

                rConn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FOO2()
        {

            try
            {
                rConn = new StatConnector();
                string current_directory = Directory.GetCurrentDirectory();


                rConn.Init("R");
                AddStringToLog("R Initialized");

                rConn.EvaluateNoReturn(@"library(survival)");

                AddStringToLog("Library survival loaded");

                rConn.EvaluateNoReturn(@"library(cmprsk)");

                AddStringToLog("Library cmprsk loaded");

                rConn.SetSymbol("path_to_dbf_loader", current_directory.Replace(@"\", @"/") + @"/dbf.r");
                rConn.SetSymbol("path_to_bd_file", current_directory.Replace(@"\", @"/") + @"/BD.DBF");

                rConn.EvaluateNoReturn(@"source(path_to_dbf_loader)");

                rConn.EvaluateNoReturn(@"mb <- read.dbf(path_to_bd_file)$dbf");


                rConn.EvaluateNoReturn(@"names(mb) <- tolower(names(mb))");
                rConn.EvaluateNoReturn(@"names(mb)");
                rConn.EvaluateNoReturn(@"attach(mb)");
                rConn.EvaluateNoReturn(@"refdate <- ISOdate(year=2010, month=04, day=1)");

                rConn.EvaluateNoReturn(@"efst <- NA");

                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 0, difftime(refdate, diadate, units='days'), efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 1, difftime(evtdat, diadate, units='days'), efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 4, difftime(evtdat, diadate, units='days'), efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 2, difftime(evtdat, diadate, units='days'), efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 3, difftime(evtdat,  diadate, units='days'), efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 5, 0, efst)");
                rConn.EvaluateNoReturn(@"efst <- ifelse(evt == 6, 0, efst)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 0, 'No event', NA)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 1, 'No event', events_map)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 2, 'death in remission', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 3, 'second tumor', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==1, 'Other relapce', events_map)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==2, 'Isol. CNS Relapse', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==3, 'Other relapce', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==4, 'Other relapce', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==5, 'Other relapce', events_map)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==6, 'Other relapce', events_map)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 4 & rezloc==7, 'Other relapce', events_map)");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 5, 'not responder', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- ifelse (evt == 6, 'not responder', events_map) ");
                rConn.EvaluateNoReturn(@"events_map <- factor(events_map)");
                rConn.EvaluateNoReturn(@"data_grouping <- factor (ifelse (studie==2002  & riskgrup == 1   , 'SRG',  ifelse (studie==2002  & riskgrup == 2  , 'ImRG', NA))) ");
                rConn.EvaluateNoReturn(@"detach (mb)");
                rConn.EvaluateNoReturn(@"CI_TABLE   <- cuminc (ftime = efst / 365.25, fstatus = events_map, cencode = 'No event', group = data_grouping)");
                rConn.EvaluateNoReturn(@" plot.cuminc (CI_TABLE,  ylim = c (0, 0.20), xaxs='i', yaxs='i', ylab='Cumulative Incidence', lwd=2,xlim = c (0, 8.5) )");
                rConn.EvaluateNoReturn(@" text (5.8, 0.07, paste ('p=', signif (data.frame (CI_TABLE$Tests)$pv [1], 2)), adj=1)");
                rConn.EvaluateNoReturn(@" title ('ALL-MB 2002: all patients') ");
                rConn.EvaluateNoReturn(@" box  ()");





            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void AddStringToLog(string p_string)
        {
            rtb_log.AppendText(p_string + "\n");
        }

        #region Streams
        public string MemoryStreamToString(MemoryStream ms)
        {
            long pos = ms.Position;
            ms.Position = 0;

            StreamReader sr = new StreamReader(ms);
            string res = sr.ReadToEnd();

            ms.Position = pos;
            return res;

        }

        public MemoryStream StringToMemoryStream(string p_str)
        {

            MemoryStream res = new MemoryStream();
            StreamWriter sr = new StreamWriter(res);
            sr.Write(p_str);
            sr.Flush();

            res.Position = 0;
            return res;
        }
        #endregion

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (StartChecking())
                FOO2();

        }

        private bool StartChecking()
        {
            bool result = true;
            string current_directory = Directory.GetCurrentDirectory();
            string program_name = @"R for Windows 2.11.1";
            if (!CheckStart.IsProgramInstalled(program_name, true))
            {
                result = InstallSilently(program_name, current_directory + @"\R-2.11.1-win32.exe");
                if (result == false) return result;
            }


            program_name = @"R/Scilab (D)COM Server";
            if (!CheckStart.IsProgramInstalled(program_name, true))
            {
                result = InstallSilently(program_name, current_directory + @"\R_Scilab_DCOM3.0-1B5.exe");
                
                if (result == false) return result;
            }

            AGD.Candies.FS.CopyDirectoryRecursive(current_directory + @"\cmprsk", path_to_r_project + @"\library\cmprsk", true);
            AGD.Candies.FS.CopyDirectoryRecursive(current_directory + @"\rscproxy", path_to_r_project + @"\library\rscproxy", true);
            
            return result;
        }

        private bool InstallSilently(string program_name, string exec_path)
        {
            bool result = true;

            AddStringToLog(program_name + " not installed. Silent Installation launched");

            bool install_result = Installer.RunInstallMSI(exec_path);
            if (install_result)
                AddStringToLog("Installation succesfully completed");

            else
            {
                AddStringToLog("Installation failed. Contact developer");
                result = false;
            }

            return result;
        }

        private void btn_finish_Click(object sender, RoutedEventArgs e)
        {
            rConn.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                rConn.Close();
            }
            catch { }
        }
    }
}
