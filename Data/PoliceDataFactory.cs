using AFT_System.Data.PoliceModel;
using cdutcm.Common.DBHelper;
using cdutcm.Common.Log;
using IrLibrary_Jun.PublicClass;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Data
{
    /// <summary>
    /// 公安数据处理
    /// </summary>
    public static class PoliceDataFactory
    {
        const string verification = "ADC5C007GF194F43AACB66CVE27C3BCB4A55";
        const string URL = "http://59.41.223.235:18080";
        static readonly string ticketCon = ConfigurationManager.AppSettings["ticketConnectionString"].ToString();
        static readonly string serverGameName = ConfigurationManager.AppSettings["ServerGameName"].ToString();

        static string entranceCon = string.Format("Data Source={0};Database={1};User ID=root;Password=root;SslMode=None;", IrAdvanced.ReadString("ServerIp"), IrAdvanced.ReadString("DbName"));

        public static void Ticket(string datetime, string gameName)
        {
            LogManager.WriteLog("购买数据开始处理");
            try
            {
                string sql = @"SELECT ProductName, CertificateName, oi.CertificateNo,CellPhone,AreaName,RowNum,SeatNum,TicketId
                  FROM himall_orderseats os
                INNER JOIN himall_orders o ON os.OrderId = o.Id
                INNER JOIN himall_orderitems oi ON o.Id = oi.OrderId
                WHERE ProductName = '" + gameName + "' and OrderDate > '" + datetime + @"'";

                //string sql = @"select DISTINCT oi.Id,oi.CertificateName,oi.CertificateNo,oi.CertificatePic,oi.ProductName ,
                //        os.TicketId,os.GameId,os.AreaName,os.RowNum,os.SeatNum,o.CellPhone
                //        from himall_orders o 
                //        INNER JOIN himall_orderitems oi on o.id = oi.OrderId
                //        INNER JOIN himall_orderseats os on oi.CertificateNo = os.CertificateNo
                //        WHERE OrderDate > '" + DateTime.Now.ToString("yyyy-MM-dd") + @"' AND `OrderStatus` = 2 LIMIT 10";

                DataTable dataTable = MySqlDBHelper.ExecuteDataTable(ticketCon, sql);
                if (dataTable.Rows.Count > 0)
                {
                    LogManager.WriteLog("本次上次购买数据数量为" + dataTable.Rows.Count + "条");
                    Ticket ticket = new Ticket() { verification = verification, dataVoList = new List<TicketInfo>() };

                    foreach (DataRow item in dataTable.Rows)
                    {
                        ticket.dataVoList.Add(new TicketInfo()
                        {
                            activityId = serverGameName,
                            purchaserName = item["CertificateName"].ToString(),
                            purchaserIdCardNo = item["CertificateNo"].ToString(),
                            purchaserTel = item["CellPhone"].ToString(),
                            purchaserSeatNo = item["AreaName"].ToString() + item["RowNum"].ToString() + "排" + item["SeatNum"].ToString() + "号",
                            ticketNo = item["TicketId"].ToString(),
                        });
                    }

                    string url = URL + "/api/ticket";

                    string ticketdata = Newtonsoft.Json.JsonConvert.SerializeObject(ticket);

                    string msg = HttpPost(url, ticketdata);
                    LogManager.WriteLog("购买数据上传完成");
                    LogManager.WriteLog("购买数据上传返回结果：" + msg);
                }
                else
                {
                    LogManager.WriteLog("无购买数据上传");
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog("购买数据上传异常" + ex.Message);
            }
        }

        public static void Entrance()
        {
            LogManager.WriteLog("检票数据开始上传");
            try
            {
                string sql = @"SELECT DISTINCT s.session_id,s.tel_no,s.tel_area,s.ticket_no,s.id_no, s.buy_name,s.create_date from in_sessions s ORDER BY id desc LIMIT 10";

                DataTable dataTable = MySqlDBHelper.ExecuteDataTable(entranceCon, sql);

                Entrance entrance = new Entrance() { verification = verification, dataVoList = new List<EntranceInfo>() };

                foreach (DataRow item in dataTable.Rows)
                {
                    entrance.dataVoList.Add(new EntranceInfo()
                    {
                        activityId = serverGameName,
                        entranceCode = item["tel_no"].ToString(),
                        entranceName = item["tel_area"].ToString(),
                        ticketNumber = 1,
                        certificationNumber = 0,
                        ticketNo = item["ticket_no"].ToString(),
                        ticketHolderIdCardNo = item["id_no"].ToString(),
                        ticketHolderName = item["buy_name"].ToString(),
                        entranceFacePic = "",
                        abnormalNumber = 0,
                        entranceTime = Convert.ToDateTime(item["create_date"].ToString())
                    });
                }

                string url = URL + "/api/entrance";

                string ticketdata = Newtonsoft.Json.JsonConvert.SerializeObject(entrance);

                string msg = HttpPost(url, ticketdata);

                LogManager.WriteLog("检票数据上传完成");
                LogManager.WriteLog("检票数据上传返回结果：" + msg);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog("检票数据上传异常" + ex.Message);
            }
        }

        public static void InspectTicket()
        {
            LogManager.WriteLog("告警数据开始上传");
            try
            {
                string sql = @"SELECT id,session_id,session_id,buy_name,id_no,id_card_photo,year_ticket_photo,address,status,remark from black_names  ORDER BY id desc";

                DataTable dataTable = MySqlDBHelper.ExecuteDataTable(entranceCon, sql);

                InspectTicket inspectTicket = new InspectTicket() { verification = verification, dataVoList = new List<InspectTicketInfo>() };

                foreach (DataRow item in dataTable.Rows)
                {
                    inspectTicket.dataVoList.Add(new InspectTicketInfo()
                    {
                        taskId = serverGameName,
                        warningTime = DateTime.Now,
                        warningPosition = IrAdvanced.ReadString("TelArea"),
                        ticketWarningType = 1,
                        ticketNo = item["id"].ToString(),
                        ticketHolderIdCardNo = item["id_no"].ToString(),
                        ticketHolderName = item["buy_name"].ToString(),
                        entranceFacePic = item["remark"].ToString()
                    });
                }

                string url = URL + "/api/inspectTicket";

                string data = Newtonsoft.Json.JsonConvert.SerializeObject(inspectTicket);

                string msg = HttpPost(url, data);
                LogManager.WriteLog("检票数据上传完成");
                LogManager.WriteLog("检票数据上传返回结果：" + msg);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog("检票数据上传异常" + ex.Message);
            }
        }

        public static string HttpPost(string url, string paramData, Dictionary<string, string> headerDic = null)
        {
            string result = string.Empty;
            try
            {
                HttpWebRequest wbRequest = (HttpWebRequest)WebRequest.Create(url);
                wbRequest.Method = "POST";
                wbRequest.ContentType = "application/json";
                wbRequest.ContentLength = Encoding.UTF8.GetByteCount(paramData);
                if (headerDic != null && headerDic.Count > 0)
                {
                    foreach (var item in headerDic)
                    {
                        wbRequest.Headers.Add(item.Key, item.Value);
                    }
                }
                using (Stream requestStream = wbRequest.GetRequestStream())
                {
                    using (StreamWriter swrite = new StreamWriter(requestStream))
                    {
                        swrite.Write(paramData);
                    }
                }
                HttpWebResponse wbResponse = (HttpWebResponse)wbRequest.GetResponse();
                using (Stream responseStream = wbResponse.GetResponseStream())
                {
                    using (StreamReader sread = new StreamReader(responseStream))
                    {
                        result = sread.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            { }

            return result;
        }
    }

    public class Rusult
    {
        public string message { get; set; }
        public string status { get; set; }
        public string result { get; set; }
    }
}
