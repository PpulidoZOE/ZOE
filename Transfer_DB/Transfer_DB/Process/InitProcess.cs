using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        }

        public void StartProcess(string[] dates)
        {
            try
            {

                Logfile.processLogFile(String.Format("Processing Dates ({0} to {1})...", dates[0], dates[3]));
                m_oWorker.ReportProgress(0, String.Format("Processing Dates ({0} to {1})...", dates[0], dates[3]));

                Logfile.processLogFile("Checking Tables Integrity...");
                m_oWorker.ReportProgress(0, "Checking Tables Integrity...");

                if (checkTableIntegrity())
                {
                    /* PRUEBAS CAMBIO JABIL
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
                            conn2.Tr.Commit();
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
                    PRUEBAS CAMBIO JABIL */
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
            checkMissingTables();
            genAlterScript();
            genModifyScript();
            //checkFieldsOfTables(); //Solo revisa las diferencias en tablas del proceso
            return true;
        }

        //Revisa las tablas inexistentes entre las dos bases de datos, todas las tablas de la BD
        private bool checkMissingTables()
        {
            string tName;
            DataTable Dtables;
            sqlQuery = String.Format(@" SELECT DISTINCT(A.TABLE_NAME) as TABLE_NAME
                                        FROM {0} A
                                        WHERE NOT EXISTS (SELECT DISTINCT(B.TABLE_NAME)
				                                          FROM {1} B
				                                          WHERE A.TABLE_NAME = B.TABLE_NAME)",conn.DbInfSchema, conn2.DbInfSchema);

            Dtables = conn.execSQLReturn(sqlQuery);

            foreach (DataRow row in Dtables.Rows)
            {
                tName = row.Field<string>("TABLE_NAME").ToString();
                Logfile.integrityScripts(tName, 3);
                Logfile.findAndMoveFile(tName);
            }

            //foreach (DataRow row in Dtables.Rows)
            //{
            //    tName = row.Field<string>("TABLE_NAME").ToString();
            //    Logfile.findAndMoveFile(tName);
            //}

            return true;
        }

        //Revisa diferencias de Integridad en las tablas del proceso
        private bool checkFieldsOfTables()
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

        private bool genAlterScript()
        {
            string tName, colName, dataType, isNullable;
            string alterScript = "", argScript = "", isNull = "", alterDropScript = "";
            string charMaxLen, numPrec, numScale; 
            DataTable Dtables;
            sqlQuery = String.Format(@" SELECT 
	                                        A.TABLE_NAME,
	                                        A.COLUMN_NAME,
	                                        A.DATA_TYPE,
	                                        coalesce(A.CHARACTER_MAXIMUM_LENGTH, '0') as CML,
	                                        coalesce(A.NUMERIC_PRECISION,'0') as NP,
	                                        coalesce(A.NUMERIC_SCALE,'0') as NC,
	                                        A.IS_NULLABLE
                                    FROM {0} A
                                    LEFT JOIN {1} B ON A.TABLE_NAME = B.TABLE_NAME AND A.COLUMN_NAME = B.COLUMN_NAME
                                    WHERE 
	                                    B.COLUMN_NAME IS NULL
                                    ORDER BY A.TABLE_NAME", conn.DbInfSchema, conn2.DbInfSchema);

            Dtables = conn.execSQLReturn(sqlQuery);
            foreach (DataRow row in Dtables.Rows)
            {
                tName = row.Field<string>("TABLE_NAME").ToString();
                colName = row.Field<string>("COLUMN_NAME").ToString();
                dataType = row.Field<string>("DATA_TYPE").ToString();
                isNullable = row.Field<string>("IS_NULLABLE").ToString();
                charMaxLen = row.Field<int>("CML").ToString();
                numPrec = row.Field<byte>("NP").ToString();
                numScale = row.Field<int>("NC").ToString();
                alterScript = "";  argScript = "";
                alterDropScript = "";

                if (isNullable == "YES")
                {
                    isNull = "NULL";
                }
                else
                {
                    isNull = "NOT NULL";
                }

                /*
                ALTER TABLE table_name
                ADD column_name datatype;
                */

                if (dataType == "bit" || dataType == "image" || dataType == "float" || dataType == "int" || dataType == "date" || dataType == "datetime")
                    argScript = String.Format("{0} {1}", dataType, isNull);

                if (dataType == "char" || dataType == "nvarchar" || dataType == "varchar")
                    argScript = String.Format("{0}({1}) {2}", dataType, charMaxLen, isNull);

                if (dataType == "varbinary")
                    argScript = String.Format("{0}(max) {1}", dataType, isNull);

                if (dataType == "decimal" || dataType == "numeric")
                    argScript = String.Format("{0}({2},{3}) {1}", dataType, isNull, numPrec, numScale);

                alterScript = String.Format("ALTER TABLE {0} ADD \"{1}\" {2}", tName, colName, argScript);
                Logfile.integrityScripts(alterScript, 1);

            }

            return true;
        }

        private bool genModifyScript()
        {
            string tName, colName, dataType, isNullable;
            string alterScript = "", argScript = "", isNull = "";
            string charMaxLen, numPrec, numScale;
            DataTable Dtables;
            sqlQuery = String.Format(@" SELECT 
	                                        A.TABLE_NAME,
	                                        A.COLUMN_NAME,
	                                        A.ORDINAL_POSITION, 
	                                        A.DATA_TYPE,
	                                        coalesce(A.CHARACTER_MAXIMUM_LENGTH, '0') as CML,
	                                        coalesce(A.NUMERIC_PRECISION,'0') as NP,
	                                        coalesce(A.NUMERIC_SCALE,'0') as NC,
	                                        A.IS_NULLABLE
                                        FROM {0} A
                                        LEFT JOIN {1} B ON A.TABLE_NAME = B.TABLE_NAME AND A.COLUMN_NAME = B.COLUMN_NAME
                                        WHERE
	                                        (A.DATA_TYPE <> B.DATA_TYPE 
	                                         OR A.CHARACTER_MAXIMUM_LENGTH <> B.CHARACTER_MAXIMUM_LENGTH
	                                         OR A.NUMERIC_PRECISION <> B.NUMERIC_PRECISION
	                                         OR A.NUMERIC_SCALE <> B.NUMERIC_SCALE
	                                         OR A.DATETIME_PRECISION <> B.DATETIME_PRECISION
	                                         OR A.IS_NULLABLE <> B.IS_NULLABLE)
                                        ORDER BY A.TABLE_NAME", conn.DbInfSchema, conn2.DbInfSchema);

            Dtables = conn.execSQLReturn(sqlQuery);
            foreach (DataRow row in Dtables.Rows)
            {
                tName = row.Field<string>("TABLE_NAME").ToString();
                colName = row.Field<string>("COLUMN_NAME").ToString();
                dataType = row.Field<string>("DATA_TYPE").ToString();
                isNullable = row.Field<string>("IS_NULLABLE").ToString();
                charMaxLen = row.Field<int>("CML").ToString();
                numPrec = row.Field<byte>("NP").ToString();
                numScale = row.Field<int>("NC").ToString();
                alterScript = ""; argScript = "";

                if (isNullable == "YES")
                {
                    isNull = "NULL";
                }
                else
                {
                    isNull = "NOT NULL";
                }

                /*
                ALTER TABLE table_name
                ALTER COLUMN column_name datatype;
                */

                if (dataType == "bit" || dataType == "image" || dataType == "float" || dataType == "int" || dataType == "date" || dataType == "datetime")
                    argScript = String.Format("{0} {1}", dataType, isNull);

                if (dataType == "char" || dataType == "nvarchar" || dataType == "varchar")
                    argScript = String.Format("{0}({1}) {2}", dataType, charMaxLen, isNull);

                if (dataType == "varbinary")
                    argScript = String.Format("{0}(max) {1}", dataType, isNull);

                if (dataType == "decimal" || dataType == "numeric")
                    argScript = String.Format("{0}({2},{3}) {1}", dataType, isNull, numPrec, numScale);

                alterScript = String.Format("ALTER TABLE {0} ALTER COLUMN \"{1}\" {2}", tName, colName, argScript);
                Logfile.integrityScripts(alterScript, 2);

            }

            return true;
        }


    }
}
