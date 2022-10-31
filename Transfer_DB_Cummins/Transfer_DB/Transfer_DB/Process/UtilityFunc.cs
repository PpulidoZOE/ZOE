using System;
using System.Data;
using Transfer_DB.SERVER;
using System.Windows.Threading;
using System.Windows;

namespace Transfer_DB.Process
{
    public static class UtilityFunc
    {

        private static Action EmptyDelegate = delegate () { };

        //Function to build the where clause
        public static bool buildWhere(ref string sWhereB, string tname, string pkFields ,SQLConnect conn, SQLConnect conn2)
        {
            DataTable DtWhere;
            string sqlQuery, field;
            sWhereB = "";

            sqlQuery = @"SELECT A.TABLE_NAME, A.CONSTRAINT_NAME, B.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS A, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE B
                        WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND A.CONSTRAINT_NAME = B.CONSTRAINT_NAME
                        AND A.TABLE_NAME = " + "'" + tname + "'";

            DtWhere = conn.execSQLReturn(sqlQuery);

            if(DtWhere.Rows.Count > 0)
            {
                for (int i = 0; i < DtWhere.Rows.Count; i++)
                {
                    field = DtWhere.Rows[i].Field<string>("column_name").ToString();

                    if (i < DtWhere.Rows.Count - 1)
                    {
                        sWhereB = sWhereB + String.Format("A.{0} = B.{0}", field) + " and ";
                    }
                    else
                    {
                        sWhereB = sWhereB + String.Format("A.{0} = B.{0}", field);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(pkFields))
                {
                    DtWhere.Dispose();
                    return buildWhereNoPK(ref sWhereB, pkFields);
                }
                else
                    DtWhere.Dispose();
                    return false; //For some Reason, the Where coudnt be generated
            }

            DtWhere.Dispose();
            return true; //The Where was succesfully created
        }

        public static bool buildWhereNoPK(ref string sWhereB, string pkFields)
        {
            string[] pks = pkFields.Split(',');
            int iCount = 0, len = pks.Length;

            foreach (string pk in pks)
            {
                iCount++;

                if ( iCount == len)
                    sWhereB = sWhereB + String.Format("A.{0} = B.{0}", pk);
                else
                    sWhereB = sWhereB + String.Format("A.{0} = B.{0}", pk) + " and ";               
            }
            return true;
        }

        //Function to check if the table has rows to insert 
        public static bool ifHasRows(string tname, string where, SQLConnect conn, SQLConnect conn2)
        {
            string sqlQuery;
            int iResult = 0;

            sqlQuery = @"SELECT COUNT(*) FROM " + conn.DboName + tname + " A WHERE NOT EXISTS (SELECT 1 FROM " + conn2.DboName + tname + " B WHERE " + where + ")";

            iResult = conn.exceSQLCount(sqlQuery);

            if (iResult > 0)
            {
                Logfile.processLogFile(String.Format("      The table {0} has {1} rows to be migrated", tname, iResult.ToString()));
                return true;
            }
            else
            {
                Logfile.processLogFile(String.Format("      The table {0} has no rows to be migrated", tname));
                return false;
            }

        }

        //Function that builds the insert statement
        public static bool buildInsert(ref string sInsert, string tname, string where, SQLConnect conn, SQLConnect conn2)
        {
            string sqlQuery, sCField, sFields = "";
            int iResult = 0;
            sInsert = "";
            DataTable DtInsert;

            sqlQuery = String.Format(@"SELECT 
                                COUNT(*)
                            FROM {0} A
                            WHERE A.TABLE_NAME = '{2}'
                                  AND EXISTS (SELECT * 
                                              FROM {1} B
                                              WHERE B.TABLE_NAME = '{2}' 
										        AND A.COLUMN_NAME = B.COLUMN_NAME 
										        AND A.DATA_TYPE = B.DATA_TYPE 
										        /*AND A.ORDINAL_POSITION = B.ORDINAL_POSITION*/
										        AND A.DATA_TYPE = B.DATA_TYPE
										        AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))", conn.DbInfSchema, conn2.DbInfSchema, tname);
            iResult = conn.exceSQLCount(sqlQuery);

            if (iResult <= 0)
            {
                Logfile.processLogFile(String.Format("There are differences in the structure of table {0}. Make sure that the structure of the table in in both databases is the same.", tname));
                return false;
            }

            sqlQuery = String.Format(@"SELECT 
                                A.COLUMN_NAME
                            FROM {0} A
                            WHERE A.TABLE_NAME = '{2}'
                                  AND EXISTS (SELECT * 
                                              FROM {1} B
                                              WHERE B.TABLE_NAME = '{2}' 
										        AND A.COLUMN_NAME = B.COLUMN_NAME 
										        AND A.DATA_TYPE = B.DATA_TYPE 
										        /*AND A.ORDINAL_POSITION = B.ORDINAL_POSITION*/
										        AND A.DATA_TYPE = B.DATA_TYPE
										        AND ISNULL(A.CHARACTER_MAXIMUM_LENGTH, 0) = ISNULL(B.CHARACTER_MAXIMUM_LENGTH, 0))", conn.DbInfSchema, conn2.DbInfSchema, tname);

            DtInsert = conn.execSQLReturn(sqlQuery);

            if(DtInsert.Rows.Count > 0)
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
                sInsert = String.Format("INSERT INTO {0} ({1}) SELECT {1} FROM {2} A WHERE NOT EXISTS (SELECT * FROM {0} B WHERE {3})", conn2.DboName + tname, sFields, conn.DboName+tname, where);                
                
            }
            else
            {
                Logfile.processLogFile(String.Format("There was a problem generating the 'Insert' statement in the table {0}. Insert statement: {1}", tname, sInsert));
                DtInsert.Dispose();
                return false;
            }

            DtInsert.Dispose();
            return true;
            
        }

        //Refresh and Update a UIElement when called
        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        //This function makes the RollBack for the two databases
        public static void rollBack(SQLConnect conn, SQLConnect conn2)
        {
            try
            {
                Logfile.processLogFile("Rolling Back...");
                conn2.Tr.Rollback();
                Logfile.processLogFile("Finish Rolling Back...");
            }
            catch (Exception e)
            {
                Logfile.errorLogFile(e);
                Logfile.processLogFile("There is a critical error in the process, please check the process log and error log for more information.");
            }

        }
    }
}
