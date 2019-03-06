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
        public static void WhiteName(int session_id, string session_name)
        {
            LogManager.WriteLog("开始同步白名单数据");
            try
            {
                string searchSql = @"SELECT oi.CertificatePic,ProductName, CertificateName, oi.CertificateNo,CellPhone,AreaName,RowNum,SeatNum,TicketId ,o.Address
                        FROM himall_orderitems oi 
                        INNER JOIN himall_orders o on o.Id = oi.OrderId
                        LEFT JOIN himall_orderseats os ON os.OrderId = o.Id
                        WHERE oi.ProductName = '" + session_name + "' AND (o.OrderStatus = 3 OR o.OrderStatus = 5) ";
                DataTable dt = MySqlDBHelper.ExecuteDataTable(ticketCon, searchSql);

                List<string> SQLStringList = new List<string>();
                List<MySqlParameter[]> mySqlParameters = new List<MySqlParameter[]>();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    string certificateName = row["CertificateName"].ToString();
                    string certificateNo = row["CertificateNo"].ToString();
                    string certificatePic = row["CertificatePic"].ToString();

                    if (!string.IsNullOrEmpty(certificateName))
                    {
                        string[] certificateNames = certificateName.Split(',');
                        string[] certificateNos = certificateNo.Split(',');

                        for (int j = 0; j < certificateNames.Length; j++)
                        {
                            string insertSql = @"insert INTO white_names(`session_id`,`create_date`,`buy_name`,`id_no`,`address`,`ticket_type`,
                            `ticket_no`,`area`,`row`,`seat`,`status`,`rrfeaturet`,`pic_url` ) VALUE 
                            (@session_id,@create_date,@buy_name,@id_no,@address,@ticket_type,
                            @ticket_no,@area,@row,@seat,@status,@rrfeaturet,@pic_url)";

                            MySqlParameter[] sqlParameters = new MySqlParameter[13];

                            sqlParameters[0] = new MySqlParameter("session_id", session_id);
                            sqlParameters[1] = new MySqlParameter("create_date", DateTime.Now);
                            sqlParameters[2] = new MySqlParameter("buy_name", certificateNames[j]);
                            sqlParameters[3] = new MySqlParameter("id_no", certificateNos[j]);
                            sqlParameters[4] = new MySqlParameter("address", string.IsNullOrEmpty(row["Address"].ToString()) ? "" : row["Address"].ToString());
                            sqlParameters[5] = new MySqlParameter("ticket_type", 0);
                            sqlParameters[6] = new MySqlParameter("ticket_no", string.IsNullOrEmpty(row["TicketId"].ToString()) ? "" : row["TicketId"].ToString());
                            sqlParameters[7] = new MySqlParameter("area", string.IsNullOrEmpty(row["AreaName"].ToString()) ? "" : row["AreaName"].ToString());
                            sqlParameters[8] = new MySqlParameter("row", string.IsNullOrEmpty(row["RowNum"].ToString()) ? "" : row["RowNum"].ToString());
                            sqlParameters[9] = new MySqlParameter("seat", string.IsNullOrEmpty(row["SeatNum"].ToString()) ? "" : row["SeatNum"].ToString());
                            sqlParameters[10] = new MySqlParameter("status", 1);
                            sqlParameters[11] = new MySqlParameter("rrfeaturet", "");
                            sqlParameters[12] = new MySqlParameter("pic_url", string.IsNullOrEmpty(certificatePic) ? "" : certificatePic.Split(',')[j]);

                            mySqlParameters.Add(sqlParameters);

                            SQLStringList.Add(insertSql);
                        }

                    }
                }

                string deleteSql = "DELETE FROM white_names WHERE session_id = " + session_id;
                MySqlDBHelper.ExecuteNonQuery(entranceCon, CommandType.Text, deleteSql);

                bool succes = MySqlDBHelper.ExecuteTransaction(entranceCon, CommandType.Text, SQLStringList.ToArray(), mySqlParameters.ToArray());
                if (succes)
                    LogManager.WriteLog("同步白名单数据成功：" + SQLStringList.Count + "条数据");
                else
                {
                    LogManager.WriteLog("同步白名单数据失败");
                }

            }
            catch (Exception ex)
            {
                LogManager.WriteLog("同步白名单数据异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 同步比赛
        /// </summary>
        public static PoliceModel.SessionModel KeyTable()
        {
            LogManager.WriteLog("开始同步比赛数据");
            int session_id = 0;
            PoliceModel.SessionModel session = new PoliceModel.SessionModel();
            try
            {
                string searchSql = @"select * FROM sessions WHERE `status` = 0 ";

                DataTable sessiondt = MySqlDBHelper.ExecuteDataTable(centerCon, searchSql);

                if (sessiondt.Rows.Count > 0)
                {
                    DataRow sessiondataRow = sessiondt.Rows[0];
                    session_id = Convert.ToInt32(sessiondataRow["session_id"]);
                    session.Id = session_id;
                    session.Name = sessiondataRow["session_name"].ToString();

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
            return session;
        }

    }
}
