using System;
using System.ComponentModel;
using System.Data;
using Transfer_DB.SERVER;

namespace Transfer_DB.Process
{
    class CatalogProcess : InitProcess
    {
        //private SQLConnect conn, conn2;
        private string sqlQuery;
           
        //Datatables
        private DataTable DtCatalogs;

        //Constructor From the inherited class
        public CatalogProcess(SQLConnect conn, SQLConnect conn2, BackgroundWorker m_oWorker, string[] dates) : base(conn, conn2, m_oWorker, dates) { }

        //Function for the Main Process
        public bool insertCatalogs()
        {
            string tName, indNoPk, pkFields; //Table name to insert
            string sWhere = ""; //Where of the table (Primary Keys)
            string sInsert = ""; //Generated insert
            int iResult = 0;
            
            try
            {
                getCatalogs(); //Fill the ListCatalogs Datatable with the tables to insert

                if (DtCatalogs.Rows.Count > 0)
                {
                    foreach (DataRow row in DtCatalogs.Rows) //For each catalog
                    {
                        iResult = 0;
                        tName = row.Field<string>("tname").ToString(); //Table name
                        indNoPk = row.Field<string>("ind_nopk").ToString(); //Table name
                        pkFields = row.Field<string>("pk_fields").ToString(); //Table name
                        //mainWindow.changeTxt("Processing table " + tName + Environment.NewLine);

                        if (UtilityFunc.buildWhere(ref sWhere, tName, pkFields, conn, conn2) == false)
                        {
                            Logfile.processLogFile(String.Format("Catalog Process - The table {0} does not have primary keys, the process can not continue", tName));
                            //throw new Exception(String.Format("Catalog Process - The table {0} does not have primary keys, the process can not continue", tName));
                            return false;
                        }

                        if (UtilityFunc.ifHasRows(tName, sWhere, conn, conn2))
                        {
                            if(UtilityFunc.buildInsert(ref sInsert, tName, sWhere, conn, conn2))
                            {
                                //mainWindow.changeTxt("Inserting table " + tName + Environment.NewLine);
                                iResult = conn2.exceSQLNoReturn(sInsert);
                                //conn2.Tr.Commit();
                                Logfile.processLogFile(String.Format("      -   {0} records where inserted in the destination database {1} in the table {2}", iResult, conn2.DbCatalog, tName));
                            }
                        }                        
                    }                    
                    return true;
                }
                else
                {
                    Logfile.processLogFile("Catalog Process - There are no catalogs to be evaluated, the process cannot continue.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Logfile.errorLogFile(e);   
                Logfile.processLogFile("Catalog Process - There is a critical error in the process, please check the error log for more information.");
                m_oWorker.ReportProgress(0, "Catalog Process - There is a critical error in the process, please check the error log for more information.");
                Logfile.processLogFile("Rolling Back...");
                m_oWorker.ReportProgress(0, "Rolling Back...");
                UtilityFunc.rollBack(conn, conn2);
                return false;
            }
        }

        //Get the tables of Catalogs to be inserted
        public void getCatalogs()
        {
            sqlQuery = "SELECT tname, ind_nopk, pk_fields FROM TFCatalogs WHERE ind_insert = '1'";
            DtCatalogs = conn.execSQLReturn(sqlQuery);

        }     

    }
}
