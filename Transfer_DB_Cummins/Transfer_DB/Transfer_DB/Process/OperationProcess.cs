using System;
using System.ComponentModel;
using System.Data;
using Transfer_DB.SERVER;

namespace Transfer_DB.Process
{
    class OperationProcess : Operation
    {

        protected DataTable DTOpTables, DTOpTablesInsert, DTPeriodTrans;
        string sNameProcess, stypeOp;

        public OperationProcess(SQLConnect conn, SQLConnect conn2, BackgroundWorker m_oWorker, string[] dates) : base(conn, conn2, m_oWorker, dates) {}


        //MAIN PROCESS 
        public bool startOperationProcess(string typeOfOperation)
        {
            stypeOp = typeOfOperation;
            switch (stypeOp)
            {
                case "IMP":
                    sNameProcess = "Import";                 
                    break;
                case "EXP":
                    sNameProcess = "EXPORT";
                    break;
                case "CTM":
                    sNameProcess = "CTM's";
                    break;
                case "AVS":
                    sNameProcess = "Avisos";
                    break;
                case "R1I":
                    sNameProcess = "R1 - IMPO";
                    break;
                case "R1E":
                    sNameProcess = "R1 - EXPO";
                    break;
                case "TRS":
                    sNameProcess = "Transaction";
                    break;
                case "DES":
                    sNameProcess = "Descargas";
                    break;
            }

            Logfile.processLogFile("Processing " + sNameProcess + "...");
            if (getOpTables(stypeOp))
            {
                if (deleteOps() && insertOps())
                {
                    DTOpTables.Dispose();
                    DTOpTablesInsert.Dispose();
                    return true;
                }
            }

            return false;
        }

        private bool getOpTables(string typeProcess)
        {
            int iResult = 0;
            sqlQuery = String.Format("SELECT COUNT(*) FROM {0}TFOpTables WHERE IND_DEL = '1' AND TYPE = '{1}';", conn.DboName, typeProcess);
            iResult = conn.exceSQLCount(sqlQuery);
            if (iResult <= 0 && typeProcess != "DES")
            {
                Logfile.processLogFile(String.Format("      The table TFOpTables has not been configured in the Origin DB {0}", conn.DbCatalog));
                return false;
            }
            sqlQuery = String.Format("SELECT ID, DEL_ORDER, MAIN_TABLE, DEL_TABLE, DEL_FIELD, REF_FIELD FROM {0}TFOpTables WHERE IND_DEL = '1' AND TYPE = '{1}';", conn.DboName, typeProcess);
            DTOpTables = conn.execSQLReturn(sqlQuery);

            sqlQuery = String.Format("SELECT COUNT(*) FROM {0}TFOpTablesInsert WHERE IND_INSERT = '1' AND TYPE = '{1}';", conn.DboName, typeProcess);
            iResult = conn.exceSQLCount(sqlQuery);
            if (iResult <= 0)
            {
                Logfile.processLogFile(String.Format("      The table TFOpTablesInsert has not been configured in the Origin DB {0}", conn.DbCatalog));
                DTOpTables.Dispose();
                return false;
            }
            sqlQuery = String.Format("SELECT ID, DEL_ORDER, MAIN_TABLE, DEL_TABLE, DEL_FIELD, REF_FIELD FROM {0}TFOpTablesInsert WHERE IND_INSERT = '1' AND TYPE = '{1}';", conn.DboName, typeProcess);
            DTOpTablesInsert = conn.execSQLReturn(sqlQuery);
            return true;
        }

        private bool deleteOps()
        {
            string sDelTable = "", sDelField = "", sReField = "";

            Logfile.processLogFile(String.Format("      *** " + sNameProcess + " - Delete Started... ***"));

            foreach (DataRow row in DTOpTables.Rows)
            {
                sDelTable =  row.Field<string>("DEL_TABLE").ToString(); 
                sDelField = row.Field<string>("DEL_FIELD").ToString();
                sReField = row.Field<string>("REF_FIELD").ToString();

                deleteTable(sDelTable, sDelField, sReField);
                
            }
            //conn2.Tr.Commit();
            return true;
        }

        private bool deleteTable(string sDelTable, string sDelField, string sReField)
        {
            string sWhere = "";
            int iResult = 0;

            if (buildWhere(ref sWhere, sDelTable, sDelField, sReField, 1, conn2))
            {
                if (tableHasRows(sDelTable, sWhere, conn2))
                {
                    if (stypeOp == "TRS" && sDelTable == "SEC02") //Proceso especial para Transacciones SEC02
                    {
                        if (getTransact(conn2))//Verificamos si hay periodos disponibles
                        {              
                            processTransact(1, null); //Realizamos el borrado
                            return true;
                        } 
                    }
                    else
                    {
                        sqlQuery = String.Format("DELETE FROM {0}{1} WHERE {2}", conn2.DboName, sDelTable, sWhere);
                        iResult = conn2.exceSQLNoReturn(sqlQuery);
                        Logfile.processLogFile(String.Format("      -   {0} Rows Deleted from table {1} Database Target {2}", iResult, sDelTable, conn2.DbCatalog));
                        return true;
                    }
                    
                }      
            }
            return false;
        }

        private bool insertOps()
        {
            string sDelTable = "", sDelField = "", sReField = "";
            string sWhere = "";
            string sInsert = "";
            int iResult = 0;

            Logfile.processLogFile(String.Format("      *** "+sNameProcess + " - Insert Started... ***"));

            foreach (DataRow row in DTOpTablesInsert.Rows)
            {
                sDelTable = row.Field<string>("DEL_TABLE").ToString();
                sDelField = row.Field<string>("DEL_FIELD").ToString();
                sReField = row.Field<string>("REF_FIELD").ToString();

                if (buildWhere(ref sWhere, sDelTable, sDelField, sReField, 2, conn))
                {
                    if (tableHasRows(sDelTable, sWhere, conn))
                    {
                        if (buildInsert(ref sInsert, sDelTable, sWhere, conn, conn2))
                        {
                            if (stypeOp == "TRS" && sDelTable == "SEC02") //Proceso especial para transacciones SEC02
                            {
                                if (getTransact(conn))
                                {
                                    processTransact(2, sInsert);
                                }
                            }
                            else
                            {
                                iResult = conn2.exceSQLNoReturn(sInsert);
                                Logfile.processLogFile(String.Format("      -   {0} Rows Inserted from table {1} to Database Target {2}", iResult, sDelTable, conn2.DbCatalog));
                                //(This Function generates the Saldos of pedimentos 2014 for saldo inicial) - Special Function just for Descargas 
                                //if (iResult > 0 && stypeOp == "DES" && sDelTable == "IMP06") //Funcion solo utilizada para VALEO
                                //{
                                //    genSaldos();
                                //}
                                if (iResult <= 0)
                                {
                                    Logfile.processLogFile(String.Format("No records were inserted in table {0} of the Destination database ({1}). The process can not continue.", sDelTable, conn2.DbCatalog));
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }                           
                    }                         
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool genSaldos() //Generación de Saldos para pedimentos de Saldo Inicial (2014 momentaneamente)
        {
            int iResult = 0;
            sqlQuery = "SP_GENERASALDOS";
            iResult = conn2.exceSQLNoReturnSP(sqlQuery,"2014/01/01", "2014/12/31", conn.DboName, conn2.DboName, null, null, null, null, null);
            Logfile.processLogFile(String.Format("      -   {0} Saldos created (IMP10) in Database Target {1}", iResult, conn2.DbCatalog));
            if (iResult <= 0)
            {
                Logfile.processLogFile(String.Format("No Saldos created in Database Target {1}, the process cannot continue.", conn2.DbCatalog));
                return false;
            }
            return true;
        }

        private bool getTransact(SQLConnect connect)
        {
            sqlQuery = String.Format("SELECT DISTINCT(CONVERT(VARCHAR,MONTH({2}SEC02.FECHA_SAP))) AS PERIODO FROM {2}SEC02 WHERE {2}SEC02.FECHA_SAP BETWEEN '{0}' AND '{1}'", dates[0], dates[3],connect.DboName);
            //sqlQuery = String.Format("SELECT DISTINCT(CONVERT(VARCHAR,MONTH({2}SEC02.FECHA_SAP))) AS PERIODO FROM {2}SEC02 WHERE {2}SEC02.FECHA_SAP BETWEEN '2015/01/01' AND '2015/07/31' ", dates[0], dates[3], connect.DboName); //DEBUG
            DTPeriodTrans = connect.execSQLReturn(sqlQuery);

            if (DTPeriodTrans.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void processTransact(int ind_action, string sInsert)
        {
            string sPeriodo = "";
            int iResult = 0;
            foreach (DataRow row in DTPeriodTrans.Rows)
            {
                sPeriodo = row.Field<string>("PERIODO").ToString();

                if (ind_action == 1) //Eliminar
                {
                    Logfile.processLogFile(String.Format("          -   Period: {0}", sPeriodo));
                    sqlQuery = String.Format("DELETE FROM {0}SEC02 WHERE {0}SEC02.FECHA_SAP BETWEEN '{1}' AND '{2}' AND MONTH({0}SEC02.FECHA_SAP) = '{3}'", conn2.DboName, dates[0], dates[3], sPeriodo);
                    //sqlQuery = String.Format("DELET FROM {0}SEC02 WHERE {0}SEC02.FECHA_SAP BETWEEN '2015/01/01' AND '2015/07/31' AND MONTH({0}SEC02.FECHA_SAP) = '{3}'", conn2.DboName, dates[0], dates[3], sPeriodo); //DEBUG
                    iResult = conn2.exceSQLNoReturn(sqlQuery);
                    Logfile.processLogFile(String.Format("              -   {0} Rows Deleted from Period: {1} Database Target {2}", iResult, sPeriodo, conn2.DbCatalog));
                }
                else
                {
                    Logfile.processLogFile(String.Format("          -   Period: {0}", sPeriodo));
                    sqlQuery = sInsert + String.Format("{0}SEC02.FECHA_SAP BETWEEN '{1}' AND '{2}' AND MONTH({0}SEC02.FECHA_SAP) = '{3}'", conn.DboName, dates[0], dates[3], sPeriodo);
                    //sqlQuery = sInsert + String.Format("{0}SEC02.FECHA_SAP BETWEEN '2015/01/01' AND '2015/07/31' AND MONTH({0}SEC02.FECHA_SAP) = '{1}'", conn.DboName, sPeriodo); //DEBUG
                    iResult = conn2.exceSQLNoReturn(sqlQuery);
                    Logfile.processLogFile(String.Format("              -   {0} Rows Inserted from Period: {1} Database Target {2}", iResult, sPeriodo, conn2.DbCatalog));
                }

            }

            DTPeriodTrans.Dispose();
        }
        /* --------------------------------------------------------- */
        private bool buildWhere(ref string sWhere, string sDelTable, string sDelField, string sReField, int indType, SQLConnect connect)
        {
            sWhere = "";

            
            if (indType == 2)
            {
                sWhere = String.Format("EXISTS (SELECT * FROM {0}TFMainData B WHERE {0}{1}.{2} = B.VALUE AND B.TYPE = '{3}' AND B.OP_TYPE = '{4}' AND B.OBS <> 'DEL_NOT_INSERT');", connect.DboName, sDelTable, sDelField, sReField, stypeOp);
            }
            else
            {
                sWhere = String.Format("EXISTS (SELECT * FROM {0}TFMainData B WHERE {0}{1}.{2} = B.VALUE AND B.TYPE = '{3}' AND B.OP_TYPE = '{4}');", connect.DboName, sDelTable, sDelField, sReField, stypeOp);
            }

            if (String.IsNullOrEmpty(sWhere))
            {
                Logfile.processLogFile(String.Format("      It was not possible to build the sentence 'Where' from this table {0}.", sDelTable));
                return false;
            }
            return true;
        }

        private bool tableHasRows(string sTable, string sWhere, SQLConnect connect)
        {
            int iResult = 0;

            sqlQuery = String.Format("SELECT COUNT(*) FROM {0}{1} WHERE {2}", connect.DboName, sTable, sWhere);
            iResult = connect.exceSQLCount(sqlQuery);
            if (iResult > 0)
            {
                Logfile.processLogFile(String.Format("      The table {0} has {1} records", sTable, iResult));
                return true;
            }
            else
            {
                Logfile.processLogFile(String.Format("      The table {0} has no records", sTable));
                return false;
            }
        }

        private bool buildInsert(ref string sInsert, string sDelTable, string sWhere, SQLConnect connect, SQLConnect connect2)
        {
            string sqlQuery, sCField, sFields = "";
            int iResult = 0;
            sInsert = "";
            DataTable DtInsert;

            /*No aplica en esta corrida*/
            //sqlQuery = String.Format(@"SELECT 
            //                    COUNT(*)
            //                FROM {0} A
            //                WHERE A.TABLE_NAME = '{2}'
            //                      AND NOT EXISTS (SELECT * 
            //                                  FROM {1} B
            //                                  WHERE B.TABLE_NAME = '{2}' 
										  //      AND A.COLUMN_NAME = B.COLUMN_NAME 
										  //      AND A.DATA_TYPE = B.DATA_TYPE 
										  //      /*AND A.ORDINAL_POSITION = B.ORDINAL_POSITION*/
										  //      AND A.DATA_TYPE = B.DATA_TYPE
										  //      AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))", connect.DbInfSchema, connect2.DbInfSchema, sDelTable);
            //iResult = conn.exceSQLCount(sqlQuery);

            //if (iResult > 0)
            //{
            //    Logfile.processLogFile(String.Format("There are differences in the structure of table {0}. Make sure that the structure of the table in in both databases is the same. The process can not continue.", sDelTable));
            //    return false;
            //}

            //sqlQuery = String.Format(@"SELECT 
            //                    A.COLUMN_NAME
            //                FROM {0} A
            //                WHERE A.TABLE_NAME = '{2}'
            //                      AND EXISTS (SELECT * 
            //                                  FROM {1} B
            //                                  WHERE B.TABLE_NAME = '{2}' 
										  //      AND A.COLUMN_NAME = B.COLUMN_NAME 
										  //      AND A.DATA_TYPE = B.DATA_TYPE 
										  //      /*AND A.ORDINAL_POSITION = B.ORDINAL_POSITION*/
										  //      AND A.DATA_TYPE = B.DATA_TYPE
										  //      AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))", connect.DbInfSchema, connect2.DbInfSchema, sDelTable);


            sqlQuery = String.Format(@"SELECT 
                                A.COLUMN_NAME
                            FROM {0} A
                            WHERE A.TABLE_NAME = '{2}'
                                  AND EXISTS (SELECT * 
                                              FROM {1} B
                                              WHERE B.TABLE_NAME = '{2}' 
										        AND A.COLUMN_NAME = B.COLUMN_NAME 
										        AND A.DATA_TYPE = B.DATA_TYPE)", connect.DbInfSchema, connect2.DbInfSchema, sDelTable);

            DtInsert = connect.execSQLReturn(sqlQuery);

            if (DtInsert.Rows.Count > 0)
            {

                for (int i = 0; i < DtInsert.Rows.Count; i++)
                {
                    sCField = DtInsert.Rows[i].Field<string>("COLUMN_NAME").ToString();

                    if (i < DtInsert.Rows.Count - 1)
                    {
                        sFields = sFields + sCField + ", ";
                    }
                    else
                    {
                        sFields = sFields + sCField;
                    }

                }
                if (stypeOp == "TRS" && sDelTable == "SEC02")
                {
                    sInsert = String.Format("INSERT INTO {0} ({1}) SELECT {1} FROM {2} WHERE ", conn2.DboName + sDelTable, sFields, connect.DboName + sDelTable);
                }
                else
                {
                    sInsert = String.Format("INSERT INTO {0} ({1}) SELECT {1} FROM {2} WHERE {3}", conn2.DboName + sDelTable, sFields, connect.DboName + sDelTable, sWhere);
                }
            }
            else
            {
                Logfile.processLogFile(String.Format("There was a problem generating the 'Insert' statement in the table {0}. Insert statement: {1}", sDelTable, sInsert));
                DtInsert.Dispose();
                return false;
            }

            DtInsert.Dispose();
            return true;
        }


    }
}

