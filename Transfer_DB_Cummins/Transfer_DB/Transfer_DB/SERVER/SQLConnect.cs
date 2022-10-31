using System;
using System.Data.SqlClient;
using System.Data;
using Transfer_DB.Process;

namespace Transfer_DB.SERVER
{

    public class SQLConnect //Class for the different Database Connection 
    {

        private SqlConnection conn;
        private SqlTransaction tr;

        SqlCommand command;
        public SqlConnection connection;

        private string dbName;
        private string dbSource;
        private string dbUser;
        private string dbPassword;
        private string dbCatalog;

        private string dboName, dbInfSchema;
                
        public string DbName
        {
            get
            {
                return dbName;
            }

            set
            {
                dbName = value;
            }
        }

        public string DbSource
        {
            get
            {
                return dbSource;
            }

            set
            {
                dbSource = value;
            }
        }

        public string DbUser
        {
            get
            {
                return dbUser;
            }

            set
            {
                dbUser = value;
            }
        }

        public string DbPassword
        {
            get
            {
                return dbPassword;
            }

            set
            {
                dbPassword = value;
            }
        }

        public string DbCatalog
        {
            get
            {
                return dbCatalog;
            }

            set
            {
                dbCatalog = value;
            }
        }

        public string DbInfSchema
        {
            get
            {
                return dbInfSchema;
            }

            set
            {
                dbInfSchema = value;
            }
        }

        public string DboName
        {
            get
            {
                return dboName;
            }

            set
            {
                dboName = value;
            }
        }

        public SqlTransaction Tr
        {
            get
            {
                return tr;
            }

            set
            {
                tr = value;
            }
        }

        //Connection Builder
        public bool SQlConnection() 
        {
            try
            {
                // Build connection string
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = dbSource;   
                builder.UserID = dbUser;              
                builder.Password = dbPassword;     
                builder.InitialCatalog = dbCatalog;

                //Connect to SQL
                Console.Write("Connecting to SQL Server ... ");

                connection = new SqlConnection(builder.ConnectionString);
                connection.Open();
                conn = connection;

                command = connection.CreateCommand();
                tr = connection.BeginTransaction();
                command.Transaction = tr;

                command.CommandTimeout = 0; //Test Propurses (Delete afer improving the responsivenes of the app with background worker)

                if (conn.State == ConnectionState.Open)
                {
                    this.DboName = dbCatalog + ".dbo.";
                    this.DbInfSchema = dbCatalog + ".INFORMATION_SCHEMA.COLUMNS";
                }
                
                return true;
            }
            catch (SqlException e)
            {
                Logfile.errorLogFile(e);
                Logfile.processLogFile("There is a critical error in the process, please check the error log for more information.");
                return false;
            }

        }

        //Function that execute a sql query and return the result in a dataset
        public DataTable execSQLReturn(string sqlQuery) 
        {
            DataTable dtsql = new DataTable();
            try
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQuery;
                //using (SqlCommand command = new SqlCommand(sqlQuery, conn))
                //{

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dtsql.Load(reader);
                        return dtsql;
                    }      
                //}             
            }
            catch (SqlException e)
            {
                throw e;
            }          
        }

        public int exceSQLNoReturnSP(string sqlQuery, string iniDate, string finDate, string DboName, string DboName2, string iniAnio, string finAnio, string iniD, string finD, string Typebd)
        {
            try
            {
                command.Parameters.Clear();
                command.CommandText = sqlQuery;
                //using (SqlCommand command = new SqlCommand(sqlQuery, conn))
                //{
                    command.CommandType = CommandType.StoredProcedure;
                    //command.Transaction = tr;
                    if (sqlQuery == "SP_TFMAINDATA")
                    {
                        command.Parameters.AddWithValue("@iniDate", iniDate);
                        command.Parameters.AddWithValue("@finDate", finDate);
                        command.Parameters.AddWithValue("@DboName", DboName);
                        command.Parameters.AddWithValue("@DboName2", DboName2);
                        command.Parameters.AddWithValue("@iniAnio", iniAnio);
                        command.Parameters.AddWithValue("@finAnio", finAnio);
                        command.Parameters.AddWithValue("@iniD", iniD);
                        command.Parameters.AddWithValue("@finD", finD);
                        command.Parameters.AddWithValue("@Typebd", Typebd);
                    }

                    if (sqlQuery == "SP_GENERASALDOS")
                    {
                        command.Parameters.AddWithValue("@iniDate", iniDate);
                        command.Parameters.AddWithValue("@finDate", finDate);
                        command.Parameters.AddWithValue("@DboName", DboName);
                        command.Parameters.AddWithValue("@DboName2", DboName2);
                    }

                    
                    return command.ExecuteNonQuery();
                //}
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        public int exceSQLNoReturn(string sqlQuery)
        {
            Console.WriteLine(conn.State);
            try
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQuery;
                return command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        public int exceSQLCount(string sqlQuery)
        {
            int iResult = 0;

            DataTable dtsql = new DataTable();
            try
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQuery;
                //using (SqlCommand command = new SqlCommand(sqlQuery, conn))
                //{
                    //command.Transaction = tr;

                using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dtsql.Load(reader);
                        iResult = dtsql.Rows[0].Field<int>(0);
                        return iResult;
                //}
                }
            }
            catch (SqlException e)
            {
                throw e;
            }
        }

        public long[] excecSQLCount2(string sqlQuery, int parms)
        {
            long [] counts = new long[parms];
            int iResult = 0;
            DataTable dtsql = new DataTable();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlQuery;
            //using (SqlCommand command = new SqlCommand(sqlQuery, conn))
            //{
                //command.Transaction = tr;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    dtsql.Load(reader);

                    for (int i = 0; i < parms; i++)
                    {
                        iResult = Convert.ToInt32(dtsql.Rows[0].Field<string>(i));
                        counts[i] = iResult;
                    }
                  
                    return counts;
                }
            //}
        }
    }
}
