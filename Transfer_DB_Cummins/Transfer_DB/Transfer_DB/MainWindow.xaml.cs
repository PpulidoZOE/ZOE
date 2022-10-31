using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Transfer_DB.SERVER;
using Transfer_DB.Process;
using System.ComponentModel;
using System.Threading;
using System.Windows.Documents;

namespace Transfer_DB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SQLConnect> AvConnect = new List<SQLConnect>();

        SQLConnect conn; //Database connection 1 
        SQLConnect conn2; //Dtabase connection 2

        InitProcess pr;

        public DateTime finDateD, iniDateD;
        public string[] dates = new string[6];

        BackgroundWorker m_oWorker;

        //Constructor 
        public MainWindow()
        {
            InitializeComponent();
            //DEBUG
            this.ini_date.SelectedDate = new DateTime(2015, 01, 01);
            this.final_date.SelectedDate = new DateTime(2015, 12, 31);

            this.tst_conn1.Visibility = Visibility.Hidden;
            this.bt_conn2.Visibility = Visibility.Hidden;

            //this.ini_date.SelectedDate = DateTime.Now;
            //this.final_date.SelectedDate = DateTime.Now;

            DbManagement();

            //DEBUG
            //cb_origdb.SelectedIndex = 0;
            //cb_tdb.SelectedIndex = 1;
        }

        //Reads the file that contains the databases
        public void DbManagement()
        {
            string line;
            string line2;

            string pattern = @"\[([^\[\]]+)\]";
            //string pattern2 = @"(\w+)\s?=\s?(\w+)"; Work In progress, this is for the use of regex instead of split 

            string[][] stringSeparators = { new string[] { "DbName" }, new string[] { "DbSource" }, new string[] { "DbUser" },
                                            new string[] { "DbPassword" }, new string[] { "DbCatalog" } };
            try
            {
                using (StreamReader sr = new StreamReader("dbconfig.ini"))
                {
                    line = sr.ReadToEnd();
                    foreach (Match m in Regex.Matches(line, pattern))
                    {
                        line2 = m.Groups[0].Value;
                        SQLConnect dbnew = new SQLConnect();

                        dbnew.DbName = line2.Split(stringSeparators[0], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        dbnew.DbSource = line2.Split(stringSeparators[1], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        dbnew.DbUser = line2.Split(stringSeparators[2], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        dbnew.DbPassword = line2.Split(stringSeparators[3], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();
                        dbnew.DbCatalog = line2.Split(stringSeparators[4], StringSplitOptions.None)[1].Split('=')[1].Split(';')[0].Trim();

                        AvConnect.Add(dbnew);
                    }
                }

                cb_origdb.DisplayMemberPath = "DbName";
                cb_origdb.SelectedValue = "DbName";
                cb_tdb.DisplayMemberPath = "DbName";
                cb_tdb.SelectedValue = "DbName";

                foreach (var i in AvConnect)
                {
                    cb_origdb.Items.Add(i);
                    cb_tdb.Items.Add(i);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        
        //Selection and Set of the Origin Database 
        private void cb_origdb_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            if (cb_origdb.SelectedItem != null)
            {
                var b = (SQLConnect)cb_origdb.SelectedItem;
                conn = b;
                this.dsource.Text = b.DbSource;
                this.dname.Text = b.DbCatalog;
                this.duser.Text = b.DbUser;
                this.dpassword.Password = b.DbPassword;
            }

        }

        //Selection and Set of the Target Database
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_tdb.SelectedItem != null)
            {
                var b = (SQLConnect)cb_tdb.SelectedItem;
                conn2 = b;
                this.dsource2.Text = b.DbSource;
                this.dname2.Text = b.DbCatalog;
                this.duser2.Text = b.DbUser;
                this.dpassword2.Password = b.DbPassword;
            }

        }

        //Test the Origin Database Connection
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (conn.SQlConnection() == true) {

                conn_succ_t.Content = "Connection Successful !!";
                conn_succ_t.Foreground = Brushes.Green;
                Console.WriteLine("Conexión Existosa 1.");
            }
            else
            {
                conn_succ_t.Content = "Error Connection !!";
                conn_succ_t.Foreground = Brushes.Red;
            }
        }

        //Tests the Target Databse Connection
        private void bt_conn2_Click(object sender, RoutedEventArgs e)
        {
            if (conn2.SQlConnection() == true)
            {
                conn_succ2_t.Content = "Connection Successful !!";
                conn_succ2_t.Foreground = Brushes.Green;
                Console.WriteLine("Conexión Existosa 2.");
            }
            else
            {
                conn_succ2_t.Content = "Error Connection !!";
                conn_succ2_t.Foreground = Brushes.Red;
            }
        }

        //Button to initiate the main process
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.textBox.Clear();

            if (validData())
            {

                bworkerinit();

                this.textBox.Clear();

                dates[0] = this.iniDateD.ToString("yyyy/MM/dd");
                dates[1] = this.iniDateD.ToString("MM");
                dates[2] = this.iniDateD.ToString("yyyy");

                dates[3] = this.finDateD.ToString("yyyy/MM/dd");
                dates[4] = this.finDateD.ToString("MM");
                dates[5] = this.finDateD.ToString("yyyy");

                m_oWorker.RunWorkerAsync();
                //conn.SQlConnection();
                //conn2.SQlConnection();

                //InitProcess pr = new InitProcess(conn, conn2, this);
                //pr.StartProcess();
            }

        }

        private bool validData()
        {
            if (String.IsNullOrEmpty(this.cb_origdb.Text))
            {
                this.textBox.AppendText(" - No se selecciono una base de datos Origen." + Environment.NewLine);
                return false;
            }

            if (String.IsNullOrEmpty(this.cb_tdb.Text))
            {
                this.textBox.AppendText("- No se selecciono una base de datos Target." + Environment.NewLine);
                return false;
            }

            if (this.cb_tdb.Text == this.cb_origdb.Text || this.cb_origdb.Text == this.cb_tdb.Text)
            {
                this.textBox.AppendText("- La base de datos Origen y Target deben de ser distintas" + Environment.NewLine);
                return false;
            }


            if (this.ini_date.SelectedDate == null)
            {
                this.textBox.AppendText("- No se ha seleccionado una fecha de inicio." + Environment.NewLine);
                return false;
            }


            if (this.final_date.SelectedDate == null)
            {
                this.textBox.AppendText("- No se ha seleccionado una fecha final." + Environment.NewLine);
                return false;
            }


            return true;
        }

        private void final_date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            iniDateD = this.ini_date.SelectedDate.Value;
            finDateD = this.final_date.SelectedDate.Value;

            if (finDateD < iniDateD)
            {
                date_error_t.Content = "The final date can not be before the start date!";
                date_error_t.Foreground = Brushes.Red;
            }
            else
            {
                date_error_t.Content = "";
            }
        }

        // ****************   WORKER   ******************
        public void bworkerinit()
        {
            this.button.IsEnabled = false;
            m_oWorker = new BackgroundWorker();
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.ProgressChanged += new ProgressChangedEventHandler
                    (m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = true;
            m_oWorker.WorkerSupportsCancellation = true;
        }

        public void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                if (conn.SQlConnection() && conn2.SQlConnection())
                {
                    pr = new InitProcess(conn, conn2, m_oWorker, dates);
                    pr.StartProcess(dates);

                    conn2.connection.Close();
                    conn.connection.Close();
                }
                else
                {
                    m_oWorker.ReportProgress(0, "There is a critical error in the process, please check the process log and error log for more information.");
                }

            }
            catch(Exception es)
            {
                Logfile.errorLogFile(es);
                Logfile.processLogFile("There is a critical error in the process, please check the process log and error log for more information.");
                m_oWorker.ReportProgress(0, "There is a critical error in the process, please check the process log and error log for more information.");
            }
        }

        public void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.button.IsEnabled = true;
            this.textBox.AppendText(String.Format("{0} - {1} {2}", DateTime.Now.ToString(), "Completed Process...", Environment.NewLine));
        }

        public void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //var myList = e.UserState as List<List<string>>;
            var status = (string)e.UserState;
            this.ProgressBar.Value = e.ProgressPercentage;
            this.textBox.AppendText(String.Format("{0} - {1} {2}", DateTime.Now.ToString(), status, Environment.NewLine));
        }


    }
}
