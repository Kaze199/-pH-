using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;
using System.Data.Sql;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;



namespace smtomes
{
    public class SQL
    {
        public SQL()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        //数据连接最基本需要的两个对象  
        private OleDbConnection conn = null;
        private OleDbCommand cmd = null;
        private OleDbDataAdapter da = null;  
        //公用 打开数据库的方法  
        //private static string strCon = @"Provider=Microsoft.Jet.OleDb.4.0;Data Source=data.mdb;";

        public void openDatabase()
        {
            //string strCon = ConfigurationManager.AppSettings["sqlCon"].ToString();//获取配置文件中的数据库字符串信息
            //string strCon = "Provider=Microsoft.Jet.OLEDB.4.0;Jet OLEDB:DataBase Password=;Data Source=" + System.Web.HttpContext.Current.Server.MapPath(@"Data\data.mdb");
            string strCon = @"Provider=Microsoft.Jet.OleDb.4.0;Data Source=data.mdb;";
            conn = new OleDbConnection(strCon); //创建数据库连接对象
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        //封装的数据库语句执行的方法  
        //SQL数据更新方法
        public void execute(String sql)
        {
            openDatabase();
            cmd = new OleDbCommand(sql, conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }


        //protected DataTable GetDataTable()
        //SQL数据查询方法
        public DataTable GetDataTable(string sql)
        {
            openDatabase();
            da = new OleDbDataAdapter(sql, conn);//创建数据适配器
            DataTable table = new DataTable();
            da.Fill(table);
            return table;

        }
    }
}