using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace MySqlConn
{
    public class Database
    {
        public enum SSLMode
        {
            Preffered,
            Required,
            Disabled
        }

        public enum ConnectionState
        {
            Disconnected,
            Connected,
            Error
        }

        MySql.Data.MySqlClient.MySqlConnection conn;
        
        private int commandTimeout = 30;

        private ConnectionState connState = 0;
       
        private string connectionString = "";

        private string server = "";

        private string port = "3306";

        private string userId = "";

        private string password = "";

        private string encrypt = "";

        private string sslMode = "";

        private string databaseName = "";

        /// <summary>
        /// Set or get server name or IP address
        /// </summary>
        public string Server { get => server; set => server = value; }
        /// <summary>
        /// Set or get server name or port (Default = 3306);
        /// </summary>
        public string Port { get => port; set => port = value; }
        /// <summary>
        /// Set or get user ID
        /// </summary>
        public string UserId { get => userId; set => userId = value; }
        /// <summary>
        /// Set or get password
        /// </summary>
        public string Password { get => password; set => password = value; }
        /// <summary>
        /// Set or get database name
        /// </summary>
        public string DatabaseName { get => databaseName; set => databaseName = value; }
        /// <summary>
        /// Set or get connection string
        /// </summary>
        public string ConnectionString { get => connectionString; set => SetConnectionString(value); }
        /// <summary>
        /// Set or get enctypt parameter (OLD)
        /// </summary>
        public bool Encrypt { get 
            { 
                if (encrypt == "true") return true; 
                else return false; 
            } 
            set { if (value == true) encrypt = "true"; else encrypt = "false"; } }
        /// <summary>
        /// Set or get SSL Mode (current)
        /// </summary>
        public SSLMode SslMode { get 
            {
                if (sslMode.ToLower() == "prefered") return SSLMode.Preffered;
                else if (sslMode.ToLower() == "required") return SSLMode.Required;
                else return SSLMode.Disabled;
            }
            set => sslMode = value.ToString();
        }
        /// <summary>
        /// Max time for query completion in seconds
        /// </summary>
        public int CommandTimeout { get => commandTimeout; set => commandTimeout = value; }
        public ConnectionState ConnState { get => connState; }

        private void SetConnectionString(string value)
        {
            connectionString = value.Replace(" ","");
            string _port = "";
            string _sslMode = "";
            string _encrypt = "";

            server = GetConnValue("Server");
            userId = GetConnValue("Uid");
            password = GetConnValue("Pwd");
            databaseName = GetConnValue("Database");

            _port = GetConnValue("Port");
            if (_port.Length > 0)
                port = _port;

            _sslMode = GetConnValue("SslMode");
            if (_sslMode.Length > 0)
                sslMode = _sslMode;

            _encrypt = GetConnValue("Encrypt");
            if (_encrypt.Length > 0 && sslMode.Length == 0)     //SSL Mode is prefered over Encrypt
                encrypt = _encrypt;
        }

        private string GetConnValue(string valueName)
        {
            return new Regex(valueName + "(.*?)=(.*?);").Match(connectionString).Groups[2].Value;
        }

        public Database()
        { 
        
        }

        public Database(string ConnString)
        {
            ConnectionString = ConnString;
        }

        /// <summary>
        /// Open database connection using Connection parameters
        /// </summary>
        public void OpenConn()
        {
            string missingInfo = "";

            if (server.Length < 1) missingInfo = "Server";
            if (databaseName.Length < 1) missingInfo = "DatabaseName";
            if (password.Length < 1) missingInfo = "Password";
            if (userId.Length < 1) missingInfo = "UserId";

            string myConnectionString = "server=" + server +
                ";uid=" + userId +
                ";pwd=" + password +
                ";database=" + databaseName;

            if (port.Length > 0) myConnectionString += ";Port=" + port;

            if (sslMode.Length > 0) myConnectionString += ";SslMode=" + sslMode;
            else
                if (encrypt.Length > 0) myConnectionString += ";Encrypt=" + encrypt;

            if (missingInfo.Length > 0)
            {
                throw new Exception("Can not establish connection due to missing information: " + missingInfo);
            }

            if (connectionString.Length > 0) myConnectionString = connectionString; //prefering user connection string

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();
                connState = ConnectionState.Connected;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                connState = ConnectionState.Error;
                throw ex;
            }
        }

        /// <summary>
        /// Close database connection
        /// </summary>
        public void CloseConn()
        {
            try
            {
                conn.Close();
                connState = ConnectionState.Disconnected;
            }
            
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                connState = ConnectionState.Error;
                throw ex; 
            }
        }

        public void Reconnect()
        {
            if (ConnState == ConnectionState.Connected)
                CloseConn();

            OpenConn();
        }

        /// <summary>
        /// Returns result as System.Data.DataTable
        /// </summary>
        /// <param name="sqlCommand">SQL command as System.String</param>
        /// <param name="autoConn">Autmatically open and then close connection after execution (Default = FALSE) as System.Boolean</param>
        /// <returns></returns>
        public System.Data.DataTable GetData(string sqlCommand, bool autoConn = false)
        {
            DataTable dta = new DataTable();

            if (autoConn && connState != ConnectionState.Connected)
                OpenConn();

            if (connState == ConnectionState.Connected)
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
                    cmd.CommandText = sqlCommand;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = commandTimeout;
                    MySql.Data.MySqlClient.MySqlDataReader reader = cmd.ExecuteReader();

                    int i = 0;

                    while (i <= (reader.FieldCount - 1))
                    {
                        dta.Columns.Add(reader.GetName(i).ToString(), reader.GetFieldType(i));
                        i++;
                    }

                    while (reader.Read())
                    {
                        DataRow dr = dta.NewRow();
                        foreach (DataColumn dc in dta.Columns)
                        {
                            dr[dc.ColumnName] = reader[dc.ColumnName].ToString();
                        }
                        dta.Rows.Add(dr);
                    }
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    connState = ConnectionState.Error;
                    throw ex;
                }
                finally
                {
                    if (autoConn)
                        CloseConn();
                }
            else
                throw new Exception("Connection has not been established.");

            return dta;
        }

        /// <summary>
        /// Executes SQL query without data return
        /// </summary>
        /// <param name="sqlCommand">SQL query (System.String)</param>
        public void Execute(string sqlCommand, bool autoConn = false)
        {
            if (autoConn && connState != ConnectionState.Connected)
                OpenConn();

            if (connState == ConnectionState.Connected)

                try
                {
                    MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
                    cmd.CommandText = sqlCommand;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = commandTimeout;
                    cmd.ExecuteNonQuery();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    connState = ConnectionState.Error;
                    throw ex;
                }
                finally
                {
                    if (autoConn)
                        CloseConn();
                }
            else
                throw new Exception("Connection has not been established.");


        }

    }
}
