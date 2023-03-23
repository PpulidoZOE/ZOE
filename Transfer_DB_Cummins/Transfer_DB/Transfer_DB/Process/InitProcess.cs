using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using Transfer_DB.SERVER;

namespace Transfer_DB.Process
{
    class InitProcess //Class of the whole process
    {
        private string sqlQuery;
        private CatalogProcess cp;
        private Operation op;
        public SQLConnect conn,conn2; //Connectios for the process, conn = Origin DB, conn2 = Target DB

        //public MainWindow mainWindow;

        public BackgroundWorker m_oWorker;

        public string[] dates = new string[6];

        public string statusview;

        public InitProcess(SQLConnect conn, SQLConnect conn2, BackgroundWorker m_oWorker, string[] dates)
        {
            //Inicializamos las conexiones a usar
            this.conn = conn;
            this.conn2 = conn2;
            this.m_oWorker = m_oWorker;
            this.dates = dates;
            //this.mainWindow = mainWindow;
        }

        public void StartProcess(string[] dates)
        {
            try
            {

                //First insert all catalogs to the new db

                //worker.ReportProgress(50, String.Format("Processing Iteration"));
                Logfile.processLogFile(String.Format("Processing Dates ({0} to {1})...", dates[0], dates[3]));
                statusview = String.Format("Processing Dates ({0} to {1})...", dates[0], dates[3]);
                m_oWorker.ReportProgress(0, statusview);

                Logfile.processLogFile("Checking Tables Integrity...");
                m_oWorker.ReportProgress(0, "Checking Tables Integrity...");

                if (checkTableIntegrity2())
                {
                    Logfile.processLogFile("Processing Catalogs...");
                    statusview = "Processing Catalogs...";
                    m_oWorker.ReportProgress(0, statusview);

                    cp = new CatalogProcess(conn, conn2, m_oWorker, dates);
                    if (cp.insertCatalogs())
                    {
                    
                        Logfile.processLogFile("Processing Operations...");
                        m_oWorker.ReportProgress(9, "Processing Operations...");
                    

                        op = new Operation(conn, conn2, m_oWorker, dates);
                        if (op.starOperation())
                        {
                            conn2.Tr.Commit(); //Debug
                            //UtilityFunc.rollBack(conn, conn2);
                            Logfile.processLogFile("Completed Process...");
                            m_oWorker.ReportProgress(100);
                        }
                        else
                        {
                            m_oWorker.ReportProgress(0, "Rolling Back...");
                            UtilityFunc.rollBack(conn, conn2);
                        }

                    }
                    else
                    {
                        m_oWorker.ReportProgress(0, "Rolling Back...");
                        UtilityFunc.rollBack(conn, conn2);
                    }
                } 
            }
            catch (Exception e)
            {
                Logfile.errorLogFile(e);
                Logfile.processLogFile("There is a critical error in the process, please check the process log and error log for more information.");
                m_oWorker.ReportProgress(0, "There is a critical error in the process, please check the process log and error log for more information.");
                m_oWorker.ReportProgress(0, "Rolling Back...");
                UtilityFunc.rollBack(conn, conn2);
            }
        }

        private bool checkTableIntegrity()
        {
            string tName;
            int iResult = 0;
            bool integrityError = false;
            DataTable Dtables;
            sqlQuery = String.Format(@" SELECT DISTINCT(DEL_TABLE) AS TABLES FROM TFOPTABLES
                                        UNION
                                        SELECT DISTINCT(DEL_TABLE) FROM TFOpTablesInsert
                                        UNION
                                        SELECT TNAME FROM TFCATALOGS");

            Dtables = conn.execSQLReturn(sqlQuery);

            foreach (DataRow row in Dtables.Rows)
            {
                tName = row.Field<string>("TABLES").ToString();

                sqlQuery = String.Format(@" SELECT COUNT(*) FROM (
	                                            SELECT 
		                                            A.TABLE_NAME
	                                            FROM {0} A
	                                            WHERE 
		                                            A.TABLE_NAME = '{2}'
		                                            AND NOT EXISTS (SELECT * 
						                                            FROM {1} B
						                                            WHERE 
							                                            B.TABLE_NAME = '{2}' 
							                                            AND A.COLUMN_NAME = B.COLUMN_NAME 
							                                            AND A.DATA_TYPE = B.DATA_TYPE 
							                                            AND A.DATA_TYPE = B.DATA_TYPE
							                                            AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))
	                                            UNION ALL
	                                            SELECT 
		                                            A.TABLE_NAME
	                                            FROM {1} A
	                                            WHERE 
		                                            A.TABLE_NAME = '{2}'
		                                            AND NOT EXISTS (SELECT * 
						                                            FROM {0} B
						                                            WHERE 
							                                            B.TABLE_NAME = '{2}' 
							                                            AND A.COLUMN_NAME = B.COLUMN_NAME 
							                                            AND A.DATA_TYPE = B.DATA_TYPE 
							                                            AND A.DATA_TYPE = B.DATA_TYPE
							                                            AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))) AS count;", conn.DbInfSchema, conn2.DbInfSchema, tName);

                iResult = conn.exceSQLCount(sqlQuery);

                if (iResult > 0)
                {
                    m_oWorker.ReportProgress(0, "       Table Integrity Error, check de log for more information.");
                    Logfile.processLogFile(String.Format("      There are differences in the structure of table {0}. Make sure that the structure of the table in both databases is the same.", tName));
                    integrityError = true;
                }
            }
            Dtables.Dispose();

            if (integrityError)
            {
                Logfile.processLogFile(String.Format("Please solve all the table structure issues in order to continue!"));
                m_oWorker.ReportProgress(0, "Please solve all the table structure issues in order to continue!");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool checkTableIntegrity2()
        {
            string tName;
            int iResult = 0;
            bool integrityError = false;
            DataTable Dtables;
            //Verifica solo las tablas con ind = 1
            
            sqlQuery = String.Format(@" SELECT DISTINCT(DEL_TABLE) AS TABLES FROM TFOPTABLES where IND_DEL = '1'
                                        UNION
                                        SELECT DISTINCT(DEL_TABLE) FROM TFOpTablesInsert where IND_INSERT = '1'
                                        UNION
                                        SELECT TNAME FROM TFCATALOGS WHERE ind_insert = '1'");

            Dtables = conn.execSQLReturn(sqlQuery);

            foreach (DataRow row in Dtables.Rows)
            {
                tName = row.Field<string>("TABLES").ToString();

                sqlQuery = String.Format(@" 
	                SELECT 
		                COUNT(*) as count
	                FROM {1} A
	                INNER JOIN {0} B ON A.TABLE_NAME = B.TABLE_NAME AND A.COLUMN_NAME = B.COLUMN_NAME
	                WHERE 
	                A.TABLE_NAME = '{2}' AND
	                (A.DATA_TYPE < B.DATA_TYPE
	                OR COALESCE(A.CHARACTER_MAXIMUM_LENGTH,0) < COALESCE(B.CHARACTER_MAXIMUM_LENGTH,0)
	                OR COALESCE(A.CHARACTER_OCTET_LENGTH,0) < COALESCE(B.CHARACTER_OCTET_LENGTH,0)
	                OR COALESCE(A.NUMERIC_PRECISION,0) < COALESCE(B.NUMERIC_PRECISION,0)
	                OR COALESCE(A.NUMERIC_PRECISION_RADIX,0) < COALESCE(B.NUMERIC_PRECISION_RADIX,0)
	                OR COALESCE(A.NUMERIC_SCALE,0) < COALESCE(B.NUMERIC_SCALE,0))", conn.DbInfSchema, conn2.DbInfSchema, tName);

                iResult = conn.exceSQLCount(sqlQuery);

                if (iResult > 0)
                {
                    m_oWorker.ReportProgress(0, "       Table Integrity Error, check de log for more information.");
                    Logfile.processLogFile(String.Format("      There are differences in the structure of table {0}. Make sure that the structure of the table in both databases is the same.", tName));
                    integrityError = true;
                }
            }
            Dtables.Dispose();

            if (integrityError)
            {
                Logfile.processLogFile(String.Format("Please solve all the table structure issues in order to continue!"));
                m_oWorker.ReportProgress(0, "Please solve all the table structure issues in order to continue!");
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
