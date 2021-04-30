using System;
using System.Collections.Generic;
using System.Text;
using MySqlConn;
using System.Data;

namespace MySqlConn_Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Database mySql = new Database("Server = myServer; Database = myDataBase; Uid = myUsername; Pwd = myPassword;");
            mySql.OpenConn();

            try
            {
                DataTable dta = mySql.GetData("SELECT * FROM table");
                mySql.Execute("INSERT INTO tabulka (column1, column2) VALUES ('hrc', 'prc')");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

        static void CommentedExample()
        {
            //Create new instance with connection string
            Database mySql = new Database("Server = myServer; Database = myDataBase; Uid = myUsername; Pwd = myPassword;");

            //Or create new instance withou connection string and use parameteres insted
            /*
            //Database mySql = new Database();
            //Mandatory params
            mySql.Server = "myServer";
            mySql.DatabaseName = "myDataBase";
            mySql.UserId = "myUsername";
            mySql.Password = "myPassword";
            //Optional params
            mySql.Port = "3306";                        //Default = 3306
            mySql.Encrypt = true;                           
            mySql.SslMode = Database.SSLMode.Required;  //If Encrypt and SSLMode is selected, SSLMode is preferred.
            */

            mySql.OpenConn();

            try
            {
                //Get your data
                DataTable dta = mySql.GetData("SELECT * FROM table");

                //Get and change connection parameters
                if (mySql.UserId == "user1")
                {
                    mySql.UserId = "user2";
                    mySql.Password = "pwd2";
                    mySql.Reconnect();
                }

                //Execute any SQL (no data returns)
                mySql.Execute("INSERT INTO tabulka (column1, column2) VALUES ('hrc', 'prc')");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

    }
}
