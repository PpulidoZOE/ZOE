using System;
using System.Diagnostics;
using System.IO;

namespace Transfer_DB.Process
{
    public static class Logfile //Clase para los logs del sistema. 
    {
        static string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        static string LogFolder = "TransferDB_Logs", ErrLogFile = "\\TransferDB_Error.err", ProcLogFile = "\\TransferDB_Process.log";

        public static void errorLogFile(Exception e)
        {
            string sFile = AppPath + LogFolder + ErrLogFile;
            try
            {
                if (!Directory.Exists(AppPath + LogFolder) && !File.Exists(sFile))
                {
                    Directory.CreateDirectory(AppPath + LogFolder);
                    var myfile = File.Create(sFile);
                    myfile.Close();
                }

                using (StreamWriter w = File.AppendText(sFile))
                {
                    w.WriteLine(getErrorInfo(e));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void processLogFile(string mssglog)
        {
            string sFile = AppPath + LogFolder + ProcLogFile;

            try
            {
                if (!Directory.Exists(AppPath + LogFolder) && !File.Exists(sFile))
                {
                    Directory.CreateDirectory(AppPath + LogFolder);
                    var myfile = File.Create(AppPath + LogFolder + ProcLogFile);
                    myfile.Close();
                }

                using (StreamWriter w = File.AppendText(sFile))
                { 
                    w.WriteLine(DateTime.Now.ToString() + " - " + mssglog);          
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static string getErrorInfo(Exception e)
        {
            string sError = "", sFullEx = "", sFullError = "";

            sError = e.Message.ToString();
            sFullEx = e.ToString();

            sFullError = String.Format(@"{0} - Unhandled Error
            Error: {1}  
            Full Error: {2}", DateTime.Now.ToString(), sError, sFullEx);

            return sFullError;
        }

        private static string buildProcessLog(string msslog)
        {
            string sFullError = "";

            sFullError = DateTime.Now.ToString() + " - " + msslog;

            return sFullError;
        }
    }
}
