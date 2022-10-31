using System;
using System.ComponentModel;
using System.Data;
using Transfer_DB.SERVER;

namespace Transfer_DB.Process
{
    class Operation : InitProcess
    {
        protected string iniDate = "", finDate = "", iniDateAnio = "", finDateAnio = "", iniD = "" , finD = "", sqlQuery = "";


        public Operation(SQLConnect conn, SQLConnect conn2, MainWindow mainWindow) : base(conn, conn2, mainWindow)
        {
            this.iniDate = mainWindow.iniDateD.ToString("yyyy/MM/dd");
            this.iniD = mainWindow.iniDateD.ToString("MM");
            this.iniDateAnio = mainWindow.iniDateD.ToString("yyyy");

            this.finDate = mainWindow.finDateD.ToString("yyyy/MM/dd");
            this.finD = mainWindow.finDateD.ToString("MM");
            this.finDateAnio = mainWindow.finDateD.ToString("yyyy");
        }       

        public bool starOperation()
        {
            OperationProcess Op = new OperationProcess(conn, conn2, mainWindow);
            //Obtener los datos para trabajar 
            if (getDataToWork(conn, 1) && getDataToWork(conn2, 2))
            {
                //IMP, EXP, CTM, AVS, TRS, INT
                if (Op.startOperationProcess("IMP") && Op.startOperationProcess("EXP")) //Importaciones && Op.startOperationProcess("EXP")
                {
                    if (Op.startOperationProcess("CTM"))
                        return true;
                }               
            }
            return false;
        }

        private bool getDataToWork(SQLConnect connect, int dbSelect)
        {
            int iResult = 0;

            sqlQuery = String.Format("TRUNCATE TABLE {0}TFMainData", connect.DboName);
            connect.exceSQLNoReturn(sqlQuery);

            //Relación completa: Pedimento -> Factura -> Packing
            sqlQuery = string.Format(@" INSERT INTO {2}TFMainData
                        SELECT no_pedmto, '1', 'IMP' 
                        FROM {2}imp06 
                        WHERE fecha_pago BETWEEN '{0}' AND '{1}'
                        UNION ALL
                        SELECT no_factura, '2', 'IMP'
                        FROM {2}imp04 a 
                        WHERE exists (SELECT * FROM {2}imp06 b WHERE fecha_pago BETWEEN '{0}' AND '{1}' AND a.no_pedmto = b.no_pedmto) /*AND a.ind_asign = '1'*/
                        UNION ALL
                        SELECT no_packing, '3', 'IMP'
                        FROM {2}imp05 c
                        WHERE exists (SELECT * FROM {2}imp04 a WHERE exists (SELECT * FROM {2}imp06 b WHERE fecha_pago BETWEEN '{0}' AND '{1}' AND a.no_pedmto = b.no_pedmto) AND c.no_factura = a.no_factura)
                        --Expo
                        UNION ALL
                        SELECT no_pedmto, '1', 'EXP' 
                        FROM {2}EXP06 
                        WHERE fecha_pago BETWEEN '{0}' AND '{1}'
                        UNION ALL
                        SELECT no_factura, '2', 'EXP'
                        FROM {2}EXP04 a 
                        WHERE exists (SELECT * FROM {2}EXP06 b WHERE fecha_pago BETWEEN '{0}' AND '{1}' AND a.no_pedmto = b.no_pedmto) /*AND a.ind_asign = '1'*/
                        UNION ALL
                        SELECT no_packing, '3', 'EXP'
                        FROM {2}EXP05 c
                        WHERE exists (SELECT * FROM {2}EXP04 a WHERE exists (SELECT * FROM {2}EXP06 b WHERE fecha_pago BETWEEN '{0}' AND '{1}' AND a.no_pedmto = b.no_pedmto) AND c.no_factura = a.no_factura)
                        --CTMS
                        UNION ALL
                        SELECT ctm, '1', 'CTM'
                        FROM {2}CTM01
                        WHERE anio between '{3}' and '{4}' and periodo between '{5}' and '{6}'
                        UNION ALL
                        SELECT no_factcon, '2', 'CTM'
                        FROM {2}EXP22 A
                        WHERE EXISTS ( SELECT * FROM {2}CTM01 B WHERE B.ctm = A.NO_CONST and B.anio between '{3}' and '{4}' and B.periodo between '{5}' and '{6}' ) 
                        --Avisos
                        UNION ALL
                        SELECT aviso, '1', 'AVS' 
                        FROM {2}SEC04 
                        WHERE fecha between '{0}' and '{1}'
                        --Transacciones
                        UNION ALL
                        SELECT DISTINCT(mat_doc), '1', 'TRS'
                        FROM {2}sec02 
                        WHERE fecha_sap between '{0}' and '{1}'
                        ORDER BY 3", iniDate, finDate, connect.DboName, iniDateAnio, finDateAnio, iniD, finD);

            iResult = connect.exceSQLNoReturn(sqlQuery);

            //Sin Relación (Solo con Fecha): Packings sin relación alguna, Facturas sin relación alguna 
            sqlQuery = string.Format(@"INSERT INTO {2}TFMainData
                        SELECT no_factura,  '2', 'IMP'
                        FROM {2}imp04 A 
                        WHERE A.fecha BETWEEN '{0}' AND '{1}' AND (A.no_pedmto IS NULL or A.no_pedmto = '')	
							  AND NOT exists (SELECT 1 FROM {2}TFMainDAta B where B.value = A.no_factura AND B.type = '2' and B.op_type = 'IMP')
						UNION ALL
                        SELECT no_packing, '3', 'IMP'
                        FROM {2}imp01 A
                        WHERE A.fecha BETWEEN '{0}' AND '{1}' 	
							  AND not exists (SELECT 1 FROM {2}TFMainDAta B where B.value = A.no_packing AND B.type = '3' and B.op_type = 'IMP')
                              AND not exists (SELECT 1 FROM {2}IMP05 C WHERE A.no_packing = C.no_packing)
						--EXPO
                        UNION ALL
                        SELECT no_factura, '2', 'EXP'
                        FROM {2}EXP04 A
                        WHERE A.fecha BETWEEN '{0}' AND '{1}' AND (A.no_pedmto IS NULL or A.no_pedmto = '')
							  AND NOT exists (SELECT 1 FROM {2}TFMainDAta B where B.value = A.no_factura AND B.type = '2' and B.op_type = 'EXP')
                        UNION ALL
                        SELECT no_packing, '3', 'EXP'
                        FROM {2}EXP01 A
                        WHERE A.fecha BETWEEN '{0}' AND '{1}' 	
							  AND NOT exists (SELECT 1 FROM {2}TFMainDAta B where B.value = A.no_packing AND B.type = '3' and B.op_type = 'EXP')
                              and NOT exists (SELECT 1 FROM {2}EXP05 C WHERE A.no_packing = C.no_packing)
						--CTMS
						UNION ALL
                        SELECT no_factcon, '2', 'CTM'
                        FROM {2}EXP22 A
                        WHERE A.fecha BETWEEN '{0}' AND '{1}' 
							  AND NOT EXISTS (SELECT 1 FROM {2}TFMainDAta B where B.value = A.NO_FACTCON AND B.type = '2' and B.op_type = 'CTM')
                        ORDER BY 3", iniDate, finDate, connect.DboName);

            iResult = connect.exceSQLNoReturn(sqlQuery);

            //Relación Parcial
            sqlQuery = String.Format(@"INSERT INTO {2}tfmaindata
                        --IMPO - PACKING
                        SELECT A.NO_PACKING, '3', 'IMP' FROM {2}IMP01 A
                        INNER JOIN {2}IMP05 B ON A.NO_PACKING = B.NO_PACKING
                        INNER JOIN {2}IMP04 C ON B.NO_FACTURA = C.NO_FACTURA AND C.no_pedmto IS NULL or C.no_pedmto = ''
                        WHERE A.FECHA BETWEEN '{0}' AND '{1}' AND C.fecha BETWEEN '{0}' AND '{1}' 
                        AND NOT EXISTS (select * from {2}tfmaindata E where A.NO_PACKING = E.VALUE and E.type = '3' and E.op_type = 'IMP' )
                        UNION ALL
                        --IMPO - FACTURA
                        SELECT A.NO_FACTURA, '2', 'IMP' FROM {2}IMP04 A
                        INNER JOIN {2}IMP05 B ON A.NO_FACTURA = B.NO_FACTURA
                        INNER JOIN {2}IMP01 C ON B.NO_PACKING = C.NO_PACKING
                        WHERE A.FECHA BETWEEN '{0}' AND '{1}' AND C.fecha BETWEEN '{0}' AND '{1}'
                        AND A.no_pedmto IS NULL or A.no_pedmto = '' 
                        AND NOT EXISTS (select * from {2}tfmaindata E where A.NO_FACTURA = E.VALUE and E.type = '2' and E.op_type = 'IMP' )
                        UNION ALL
                        --EXPO - PACKING
                        SELECT A.NO_PACKING, '3', 'EXP' FROM {2}EXP01 A
                        INNER JOIN {2}EXP05 B ON A.NO_PACKING = B.NO_PACKING
                        INNER JOIN {2}EXP04 C ON B.NO_FACTURA = C.NO_FACTURA AND C.no_pedmto IS NULL or C.no_pedmto = ''
                        WHERE A.FECHA BETWEEN '{0}' AND '{1}' AND C.fecha BETWEEN '{0}' AND '{1}' 
                        AND NOT EXISTS (select * from {2}tfmaindata E where A.NO_PACKING = E.VALUE and E.type = '3' and E.op_type = 'EXP' )
                        UNION ALL 
                        --EXPO - FACTURA
                        SELECT A.NO_FACTURA, '2', 'EXP' FROM {2}EXP04 A
                        INNER JOIN {2}EXP05 B ON A.NO_FACTURA = B.NO_FACTURA
                        INNER JOIN {2}EXP01 C ON B.NO_PACKING = C.NO_PACKING
                        WHERE A.FECHA BETWEEN '{0}' AND '{1}' AND C.fecha BETWEEN '{0}' AND '{1}'
                        AND A.no_pedmto IS NULL or A.no_pedmto = '' 
                        AND NOT EXISTS (select * from {2}tfmaindata E where A.NO_FACTURA = E.VALUE and E.type = '2' and E.op_type = 'EXP' )", iniDate, finDate, connect.DboName);

            iResult = connect.exceSQLNoReturn(sqlQuery);

            if (dbSelect.Equals(1))
            {
                connect.Tr.Commit();
            }
            
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
                if (dbSelect.Equals(2))
                {
                    checkForBadData();
                }
                    
                if (!validData(connect, dbSelect))
                {
                    return false;
                }
            }
            return true;
        }

        private void checkForBadData()
        {
            int iResult = 0;
            //Busca facturas que no esten asignadas a pedimentos y que no tengan packings, busca packings que no tengan facturas asignadas y que no tengan partidas. Estos Packings y Facturas se buscan 
            //en base a la información que se transferirá de la base de datos Origen. Esto solo se ejecuta en la base de datos Target. 
            sqlQuery = String.Format(@"INSERT INTO {0}TFMAINDATA
                                       SELECT no_packing, '3', 'IMP'
                                       FROM {0}IMP01 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_packing = B.value
                                                           AND OP_TYPE = 'IMP' AND TYPE = '3')
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP05 C WHERE A.NO_PACKING = C.NO_PACKING)
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP02 D WHERE A.NO_PACKING = D.NO_PACKING)
                                       UNION ALL
                                        
                                       SELECT no_factura, '2', 'IMP'
                                       FROM {0}IMP04 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_factura = B.value
                                                           AND OP_TYPE = 'IMP' AND TYPE = '2')
                                           AND NOT EXISTS(SELECT 1 FROM {0}IMP05 C WHERE A.no_factura = C.NO_PACKING)
                                           AND A.no_pedmto IS NULL AND A.ind_asign <> '1' 
                                       
                                       UNION ALL
                                       SELECT no_packing, '3', 'EXP'
                                       FROM {0}EXP01 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_packing = B.value
                                                           AND OP_TYPE = 'EXP' AND TYPE = '3')
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP05 C WHERE A.NO_PACKING = C.NO_PACKING)
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP02 D WHERE A.NO_PACKING = D.NO_PACKING)
                                       UNION ALL

                                       SELECT no_factura, '2', 'EXP'
                                       FROM {0}EXP04 A 
                                       WHERE 
                                           EXISTS (SELECT 1 FROM {1}TFMAINDATA B 
                                                       WHERE 
                                                           A.no_factura = B.value
                                                           AND OP_TYPE = 'EXP' AND TYPE = '2')
                                           AND NOT EXISTS(SELECT 1 FROM {0}EXP05 C WHERE A.no_factura = C.NO_PACKING)
                                           AND A.no_pedmto IS NULL AND A.ind_asign <> '1';", conn2.DboName, conn.DboName);

            iResult = conn2.exceSQLNoReturn(sqlQuery);
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
            sqlQuery = " SELECT (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '1' and op_type = 'CTM') as 'CTM Const', (SELECT CONVERT(VARCHAR, count(*)) FROM TFMainData WHERE type = '2' and op_type = 'CTM') as 'CTM Fact'";
            iResult = connect.excecSQLCount2(sqlQuery, 2);
            Logfile.processLogFile(String.Format("          CTM - There are {0} Constancias CTM, {1} CTM Invoices in the DB ({2})", iResult[0], iResult[1], connect.DbCatalog));

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
