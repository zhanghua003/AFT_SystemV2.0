using IrLibrary_Jun.PublicClass;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cdutcm.Common.DBHelper;
using System.Data;
using MySql.Data.MySqlClient;
using cdutcm.Common.Log;

namespace AFT_System.Data
{
    /// <summary>
    /// 中心数据处理
    /// </summary>
    public static class CenterDataFactory
    {
        /// <summary>
        /// 入场数据库
        /// </summary>
        static readonly string entranceCon = string.Format("Data Source={0};Database={1};User ID=root;Password=root;SslMode=None;", IrAdvanced.ReadString("ServerIp"), IrAdvanced.ReadString("DbName"));
        /// <summary>
        /// 售票数据库
        /// </summary>
        static readonly string ticketCon = ConfigurationManager.AppSettings["ticketConnectionString"].ToString();
        /// <summary>
        /// 中心数据库
        /// </summary>
        static readonly string centerCon = ConfigurationManager.AppSettings["centerConnectionString"].ToString();

        /// <summary>
        /// 同步黑名单
        /// </summary>
        public static void BlackName(int session_id)
        {
            LogManager.WriteLog("开始同步黑名单数据");
            try
            {
                string searchSql = @"SELECT `id`,`session_id`,`create_date`,`buy_name`,`id_no`,`id_card_photo`, `year_ticket_photo`,`address`,`status`,`remark` FROM black_names where `session_id` = " + session_id;

                DataTable dt = MySqlDBHelper.ExecuteDataTable(centerCon, searchSql);

                string[] SQLStringList = new string[dt.Rows.Count];
                MySqlParameter[][] mySqlParameters = new MySqlParameter[dt.Rows.Count][];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];

                    string insertSql = @"INSERT INTO black_names( `id`,`session_id`,`create_date`,`buy_name`,`id_no`,`id_card_photo`, `year_ticket_photo`,`address`,`status`,`remark`) VALUES(@id,@session_id,@create_date,@buy_name,@id_no,@id_card_photo,@year_ticket_photo,@address,@status,@remark)";
                    mySqlParameters[i] = new MySqlParameter[10];
                    mySqlParameters[i][0] = new MySqlParameter("id", row["id"]);
                    mySqlParameters[i][1] = new MySqlParameter("session_id", row["session_id"]);
                    mySqlParameters[i][2] = new MySqlParameter("create_date", row["create_date"]);
                    mySqlParameters[i][3] = new MySqlParameter("buy_name", row["buy_name"]);
                    mySqlParameters[i][4] = new MySqlParameter("id_no", row["id_no"]);
                    mySqlParameters[i][5] = new MySqlParameter("id_card_photo", row["id_card_photo"]);
                    mySqlParameters[i][6] = new MySqlParameter("year_ticket_photo", row["year_ticket_photo"]);
                    mySqlParameters[i][7] = new MySqlParameter("address", row["address"]);
                    mySqlParameters[i][8] = new MySqlParameter("status", row["status"]);
                    mySqlParameters[i][9] = new MySqlParameter("remark", row["remark"]);

                    SQLStringList[i] = insertSql;
                }

                string deleteSql = "DELETE FROM black_names WHERE session_id = " + session_id;
                MySqlDBHelper.ExecuteNonQuery(entranceCon, CommandType.Text, deleteSql);

                MySqlDBHelper.ExecuteTransaction(entranceCon, CommandType.Text, SQLStringList, mySqlParameters);
                LogManager.WriteLog("同步黑名单数据成功：" + dt.Rows.Count + "条数据");
            }
            catch (Exception ex)
            {
                LogManager.WriteLog("同步黑名单数据异常：" + ex.Message);
            }
        }


        /// <summary>
        /// 同步白名单
        /// </summary>
        public static void WhiteName(string session_name)
        {
            //white_names
        }

        /// <summary>
        /// 同步比赛
        /// </summary>
        public static int KeyTable()
        {
            LogManager.WriteLog("开始同步比赛数据");
            int session_id = 0;
            try
            {
                string searchSql = @"select * FROM sessions WHERE `status` = 0 ";

                DataTable sessiondt = MySqlDBHelper.ExecuteDataTable(centerCon, searchSql);

                if (sessiondt.Rows.Count > 0)
                {
                    DataRow sessiondataRow = sessiondt.Rows[0];
                    session_id = Convert.ToInt32(sessiondataRow["session_id"]);

                    LogManager.WriteLog("比赛场次：" + sessiondataRow["session_name"].ToString());

                    string deleteSql = "DELETE FROM key_table where session_id =" + session_id + "; DELETE FROM sessions where session_id =" + session_id + ";";
                    MySqlDBHelper.ExecuteNonQuery(entranceCon, CommandType.Text, deleteSql);


                    string sessionInsertSql = @"INSERT INTO `sessions` 
                (`session_id`,`create_date`,`session_name`,`session_date`,`date_start`,`date_end`,`check_rule`,`status`,`remark`) VALUES 
                (@session_id,@create_date,@session_name,@session_date,@date_start,@date_end,@check_rule,@status,@remark)";

                    MySqlParameter[] mySqlParameters = new MySqlParameter[9];
                    mySqlParameters[0] = new MySqlParameter("session_id", sessiondataRow["session_id"]);
                    mySqlParameters[1] = new MySqlParameter("create_date", sessiondataRow["create_date"]);
                    mySqlParameters[2] = new MySqlParameter("session_name", sessiondataRow["session_name"]);
                    mySqlParameters[3] = new MySqlParameter("session_date", sessiondataRow["session_date"]);
                    mySqlParameters[4] = new MySqlParameter("date_start", sessiondataRow["date_start"]);
                    mySqlParameters[5] = new MySqlParameter("date_end", sessiondataRow["date_end"]);
                    mySqlParameters[6] = new MySqlParameter("check_rule", sessiondataRow["check_rule"]);
                    mySqlParameters[7] = new MySqlParameter("status", sessiondataRow["status"]);
                    mySqlParameters[8] = new MySqlParameter("remark", sessiondataRow["remark"]);

                    MySqlDBHelper.ExecuteNonQuery(entranceCon, CommandType.Text, sessionInsertSql, mySqlParameters);

                    string keysearchSql = @"select * FROM key_table WHERE `session_id` = " + session_id;

                    DataTable keytabledt = MySqlDBHelper.ExecuteDataTable(centerCon, keysearchSql);

                    if (keytabledt.Rows.Count > 0)
                    {
                        DataRow row = keytabledt.Rows[0];

                        string keytableInsertSql = @"INSERT INTO 
                `key_table` (`key_id`,`create_date`,`session_id`,`key_content`,`key_type`,`remark`) 
                VALUES (@key_id,@create_date,@session_id,@key_content,@key_type,@remark)";

                        MySqlParameter[] keySqlParameters = new MySqlParameter[6];

                        keySqlParameters[0] = new MySqlParameter("key_id", row["key_id"]);
                        keySqlParameters[1] = new MySqlParameter("create_date", row["create_date"]);
                        keySqlParameters[2] = new MySqlParameter("session_id", row["session_id"]);
                        keySqlParameters[3] = new MySqlParameter("key_content", row["key_content"]);
                        keySqlParameters[4] = new MySqlParameter("key_type", row["key_type"]);
                        keySqlParameters[5] = new MySqlParameter("remark", row["remark"]);

                        MySqlDBHelper.ExecuteNonQuery(entranceCon, CommandType.Text, keytableInsertSql, keySqlParameters);
                    }
                }
                LogManager.WriteLog("同步比赛数据完成");
            }
            catch (Exception ex)
            {
                LogManager.WriteLog("同步比赛数据异常：" + ex.Message);
            }
            return session_id;
        }
    }
}
