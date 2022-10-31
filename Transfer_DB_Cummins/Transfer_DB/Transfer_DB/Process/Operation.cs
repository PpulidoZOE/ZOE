using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using Transfer_DB.SERVER;

namespace Transfer_DB.Process
{
    class Operation : InitProcess
    {
        protected string iniDate = "", finDate = "", iniDateAnio = "", finDateAnio = "", iniD = "" , finD = "", sqlQuery = "";


        public Operation(SQLConnect conn, SQLConnect conn2, BackgroundWorker m_oWorker, string[] dates) : base(conn, conn2, m_oWorker, dates)
        {
            //this.iniDate = mainWindow.iniDateD.ToString("yyyy/MM/dd");
            //this.iniD = mainWindow.iniDateD.ToString("MM");
            //this.iniDateAnio = mainWindow.iniDateD.ToString("yyyy");

            //this.finDate = mainWindow.finDateD.ToString("yyyy/MM/dd");
            //this.finD = mainWindow.finDateD.ToString("MM");
            //this.finDateAnio = mainWindow.finDateD.ToString("yyyy");

            this.iniDate = dates[0];
            this.iniD = dates[1];
            this.iniDateAnio = dates[2];

            this.finDate = dates[3];
            this.finD = dates[4];
            this.finDateAnio = dates[5];
        }

        public bool starOperation()
        {
            OperationProcess Op = new OperationProcess(conn, conn2, m_oWorker, dates);
            //Obtener los datos para trabajar 
            if (getDataToWork(conn, 1) && getDataToWork(conn2, 2))
            {
                m_oWorker.ReportProgress(18, "Processing Import... ");
                if (Op.startOperationProcess("IMP"))
                {
                    m_oWorker.ReportProgress(27, "Processing Export...");
                    if (Op.startOperationProcess("EXP"))
                    {
                        m_oWorker.ReportProgress(36, "Processing CTM's...");
                        //if (Op.startOperationProcess("CTM")) //Cummins no tiene CTMS
                        if (true)
                        {
                            m_oWorker.ReportProgress(45, "Processing Avisos...");
                            if (Op.startOperationProcess("AVS"))
                            {
                                m_oWorker.ReportProgress(54, "Processing R1 - Impo...");
                                if (Op.startOperationProcess("R1I"))
                                {
                                    m_oWorker.ReportProgress(63, "Processing R1 - Expo...");
                                    if (Op.startOperationProcess("R1E"))
                                    {
                                        m_oWorker.ReportProgress(72, "Processing Transactions...");
                                        if (Op.startOperationProcess("TRS"))
                                        {
                                            m_oWorker.ReportProgress(90);
                                            return true;
                                            //m_oWorker.ReportProgress(81, "Processing Descargas...");
                                            //if (Op.startOperationProcess("DES"))
                                            //{
                                            //    m_oWorker.ReportProgress(90);
                                            //    return true;
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }                         
                }               
            }
            return false;
        }

        private bool getDataToWork(SQLConnect connect, int dbSelect)
        {
            int iResult = 0;

            sqlQuery = String.Format("TRUNCATE TABLE {0}TFMainData", connect.DboName);
            connect.exceSQLNoReturn(sqlQuery);

            sqlQuery = "SP_TFMAINDATA";

            iResult = connect.exceSQLNoReturnSP(sqlQuery, iniDate, finDate, connect.DboName, conn.DboName, iniDateAnio, finDateAnio, iniD, finD, dbSelect.ToString());

            //if (dbSelect.Equals(1))
            //{
            //    connect.Tr.Commit();
            //}

            connect.Tr.Commit();

            sqlQuery = "SELECT COUNT(*) FROM TFMAINDATA";
            iResult = connect.exceSQLCount(sqlQuery);

            if (iResult <= 0 && dbSelect.Equals(1))
            {
                Logfile.processLogFile(String.Format("      Origin Database ({2}): There is no information between the dates {0} and {1} to be transfered", iniDate, finDate, conn.DbCatalog));
                return false;
            }
            else if (iResult <= 0 && dbSelect.Equals(2))
            {
                Logfile.processLogFile(String.Format("      Target Database ({2}): There is no information between the dates {0} and {1} to be deleted in the Target DB ({2})", iniDate, finDate, conn.DbCatalog));
                
            }
            
            if(iResult > 0)
            {
                /*if (dbSelect.Equals(2)) //Deprecated 15/04/2019 - PPV
                {
                    checkForBadData();
                    //return false; //DEBUG
                }*/

                if (!validData(connect, dbSelect))
                {
                    return false;
                }
            }
            return true;
           
        }  

        private bool validData(SQLConnect connect, int dbSelect)
        {
            long[] iResImp, iRespExp, iResult;

            if(dbSelect.Equals(1))
            {
                Logfile.processLogFile(String.Format("      *****       Validating Data From Origin Database {0}       *****{1}", connect.DbCatalog,Environment.NewLine));
            }
            else
            {
                Logfile.processLogFile(String.Format("      *****       Validating Data From Target Database {0}       *****{1}", connect.DbCatalog,Environment.NewLine));
            }

            //Valida Impo
            sqlQuery = "SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'IMP') as 'PED', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '2' and op_type = 'IMP') as 'FACT', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '3' and op_type = 'IMP') as 'PACK'";
            iResImp = connect.excecSQLCount2(sqlQuery, 3);
            Logfile.processLogFile(String.Format("          IMPO - There are {0} Pedimentos, {1} Invoices and {2} Packings in the Origin DB ({3})", iResImp[0], iResImp[1], iResImp[2], connect.DbCatalog));
            if (iResImp[0].Equals(0) && iResImp[1].Equals(0) && iResImp[2].Equals(0))
            {               
                if (dbSelect.Equals(1))
                {
                    Logfile.processLogFile(String.Format("      The Import Operation is not complete, the process cannot continue"));
                    return false;
                }                  
            }

            //Expo
            sqlQuery = "SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'EXP') as 'PED', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '2' and op_type = 'EXP') as 'FACT', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '3' and op_type = 'EXP') as 'PACK'";
            iRespExp = connect.excecSQLCount2(sqlQuery, 3);
            Logfile.processLogFile(String.Format("          EXPO - There are {0} Pedimentos, {1} Invoices and {2} Packings in the DB ({3})", iRespExp[0], iRespExp[1], iRespExp[2], connect.DbCatalog));
            if (iRespExp[0].Equals(0) && iRespExp[1].Equals(0) && iRespExp[2].Equals(0))
            {               
                if (dbSelect.Equals(1))
                {
                    Logfile.processLogFile(String.Format("      The Export Operation is not complete, the process cannot continue"));
                    return false;
                }
            }

            //CTM's
            /*
            sqlQuery = " SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'CTM') as 'CTM Const', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '2' and op_type = 'CTM') as 'CTM Fact'";
            iResult = connect.excecSQLCount2(sqlQuery, 2);
            Logfile.processLogFile(String.Format("          CTM - There are {0} Constancias CTM, {1} CTM Invoices in the DB ({2})", iResult[0], iResult[1], connect.DbCatalog));
            */

            //Avisos
            sqlQuery = " SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'AVS') as 'Avisos' ";
            iResult = connect.excecSQLCount2(sqlQuery, 1);
            Logfile.processLogFile(String.Format("          Avisos - There are {0} Avisos in the DB ({1})", iResult[0], connect.DbCatalog));

            //Transacciones
            sqlQuery = " SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'TRS') as 'Transacciones' ";
            iResult = connect.excecSQLCount2(sqlQuery, 1);
            Logfile.processLogFile(String.Format("          Transactions - There are {0} Transactions in the DB ({1})" + Environment.NewLine, iResult[0], connect.DbCatalog));

            //Interfaces
            //sqlQuery = " SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainDAta WHERE type = '1' and op_type = 'INT') as 'Export', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainDAta WHERE type = '2' and op_type = 'INT') as 'Movs', (SELECT CONVERT(VARCHAR, value) FROM TFMainDAta WHERE type = '3' and op_type = 'INT') as 'Invent'";
            //iResult = connect.excecSQLCount2(sqlQuery, 3);
            //Logfile.processLogFile(String.Format("      Interfaces - Export Interface: {0} rows, Movements Interface: {1} rows, Inventory Interface: {2} rows", iResult[0], iResult[1], iResult[2]));

            return true;
        }
    }
}

//Deprecated Functions
/*private void checkForBadData()
        {
            int iResult = 0;
            //Busca facturas que no esten asignadas a pedimentos y que no tengan packings, busca packings que no tengan facturas asignadas y que no tengan partidas. Estos Packings y Facturas se buscan 
            //en base a la información que se transferirá de la base de datos Origen. Esto solo se ejecuta en la base de datos Target. 
            sqlQuery = String.Format(@"INSERT INTO {0}TFMAINDATA
                                       SELECT no_packing, '3', 'IMP', ''
                                       FROM {0}IMP01 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_packing = B.value
                                                           AND OP_TYPE = 'IMP' AND TYPE = '3')
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP05 C WHERE A.NO_PACKING = C.NO_PACKING)
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP02 D WHERE A.NO_PACKING = D.NO_PACKING)
                                       UNION ALL
                                        
                                       SELECT no_factura, '2', 'IMP', ''
                                       FROM {0}IMP04 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_factura = B.value
                                                           AND OP_TYPE = 'IMP' AND TYPE = '2')
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP05 C WHERE A.no_factura = C.NO_PACKING)
                                           AND A.no_pedmto IS NULL AND A.ind_asign <> '1' 
                                       
                                       UNION ALL
                                       SELECT no_packing, '3', 'EXP', ''
                                       FROM {0}EXP01 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_packing = B.value
                                                           AND OP_TYPE = 'EXP' AND TYPE = '3')
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP05 C WHERE A.NO_PACKING = C.NO_PACKING)
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP02 D WHERE A.NO_PACKING = D.NO_PACKING)
                                       UNION ALL

                                       SELECT no_factura, '2', 'EXP', ''
                                       FROM {0}EXP04 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_factura = B.value
                                                           AND OP_TYPE = 'EXP' AND TYPE = '2')
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP05 C WHERE A.no_factura = C.NO_PACKING)
                                           AND A.no_pedmto IS NULL AND A.ind_asign <> '1';", conn2.DboName, conn.DboName);

            iResult = conn2.exceSQLNoReturn(sqlQuery);
        }*/
