using System;
using System.Diagnostics;
using System.IO;

namespace Transfer_DB.Process
{
    public static class Logfile //Clase para los logs del sistema. 
    {
        static string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        static string LogFolder = "TransferDB_Logs", ErrLogFile = "\\TransferDB_Error.err", ProcLogFile = "\\TransferDB_Process.log";
        static string IntegrityFolder = "DB_Integrity", AlterFile = "\\ALTER_SCRIPT.txt", AlterDropFile = "\\ALTER_DROP_SCRIPT.txt", ModifyFile = "\\MODIFY_SCRIPT.txt", CreateTable = "\\MISSING_TABLES.txt";
        static string allTableFolder = "\\SECIITV5_SCRIPTS", missingTablesScripts = "\\Missing_Table_Scripts";
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

        public static void integrityScripts(string alt_script, int type)
        {
            string sFile = "";

            if (type == 1)
                sFile = AppPath + IntegrityFolder + AlterFile;
            if (type == 4)
                sFile = AppPath + IntegrityFolder + AlterDropFile;
            if (type == 2)
                sFile = AppPath + IntegrityFolder + ModifyFile;       
            if (type == 3)
                sFile = AppPath + IntegrityFolder + CreateTable;

            try
            {
                if (!Directory.Exists(AppPath + IntegrityFolder) && !File.Exists(sFile))
                {
                    Directory.CreateDirectory(AppPath + IntegrityFolder);
                    var myfile = File.Create(sFile);
                    myfile.Close();
                }

                using (StreamWriter w = File.AppendText(sFile))
                {
                    w.WriteLine(alt_script + Environment.NewLine);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public static void findAndMoveFile(string table)
        {
           
            string pathFile = "\\dbo." + table + ".Table.sql";
            string tableFolder = AppPath + IntegrityFolder + allTableFolder + pathFile;
            string sFile = AppPath + IntegrityFolder + missingTablesScripts;

            if (!File.Exists(tableFolder))
            {
                //The file does not exists
                processLogFile(String.Format("The file {0} does not exists in the path {1}", pathFile, tableFolder));
            }
            else
            {
                if (!Directory.Exists(sFile))
                {
                    Directory.CreateDirectory(sFile);
                }

                File.Copy(tableFolder, sFile + pathFile);

                if (File.Exists(sFile + pathFile))
                {
                    processLogFile(String.Format("The file {0} moves correctly", pathFile));
                }
            }

        }

    }
}
