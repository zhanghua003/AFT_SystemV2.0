using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT_System.Face;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.CustomControl.ModeView.Modle
{
    #region 结构体 QrInfo
    public struct QrInfo
    {
        public string TicketNo { get; set; }
        public string IdNo { get; set; }
        public string Area { get; set; }
        public string Row { get; set; }
        public string Seat { get; set; }
    }    
    #endregion

    public class OnlyQr:BaseModle
    {
        /*
         * 0,返回正常
         * 1.二维码解码失败
         */
        
        public string EventMsg { get; set; }
        static QrInfo _qrInfo;
        public OnlyQr()
        {
            
        }

        public static QrInfo? ReadData(string data, string key)
        {
            try
            {
                data.ToSaveLog("二维码扫描结果：");
                var barCodeStr = EncryptHelper.Decrypt(data, key);
                barCodeStr.ToSaveLog(string.Format("二维码解码结果(秘钥{0}):",key));
                // string str1 = "a|" + 球票号码 + "|" + 身份证号 + "|" + 场馆区域 + "|" + 场馆行号 + "|" + 场馆座位号 + "|z";
                var ticket = barCodeStr.Split(new[] { "|" }, StringSplitOptions.None);
                if (ticket.Count() == 7)
                {
                    if (ticket[0] == "a" && ticket[6] == "z")
                    {
                        //读取到的二维码信息
                        _qrInfo.TicketNo = ticket[1];
                        _qrInfo.IdNo = ticket[2];
                        _qrInfo.Area = ticket[3];
                        _qrInfo.Row = ticket[4];
                        _qrInfo.Seat = ticket[5];
                        return _qrInfo;                       
                    }
                }
                "识别错误，解码失败.秘钥不匹配.".ToSaveLog("QR二维码：");
            }
            catch (Exception ex)
            {
               ex.ToSaveLog("二维码解码失败:");
            }
            return null;
        }
    }
}
