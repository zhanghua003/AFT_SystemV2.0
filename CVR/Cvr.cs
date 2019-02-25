using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AFT_System.CustomControl.Mode;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;
using Timer = System.Timers.Timer;

namespace AFT_System.CVR
{
    /// <summary>第二代身份证识别</summary>
     public sealed class Cvr
    {
        #region 公共成员变量
        public CvrInfo Info;
        int _iRetPort = 0;
        /// <summary>  获取到新的身份证数据抛出事件 </summary>
        public event EventHandler<CvrInfo> GetNewDataEvent;
        private void OnGetNewDataEvent(CvrInfo e)
        {
            EventHandler<CvrInfo> handler = GetNewDataEvent;
            if (handler != null) handler(this, e);
        }

        //public int Status
        //{
        //    get
        //    {
        //        try
        //        {
        //            var status = 0x90; //Cvrsdk.Well_();
        //                switch (status)
        //                {
        //                    case 0x60: "自检失败，不能接收命令".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x02: "PC 接收超时，在规定的时间内未接收到规定长度的数据".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x03: "数据传输错误".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x01: "端口打开失败/端口尚未打开/端口号不合法".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x66: "没经过授权，无法使用".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x24: "无法识别的错误".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x23: "越权操作".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x21: "接收业务终端的命令错误，包括命令中的各种数值或逻辑搭配错误".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x11: "接收业务终端数据的长度错".ToSaveLog("CVRSDK.Status:"); break;
        //                    case 0x10: "接收业务终端数据的校验和错".ToSaveLog("CVRSDK.Status:"); break;
        //                    case -2: "动态库加载失败".ToSaveLog("CVRSDK.Status:"); break;
        //                    case -1: "设备未初始化".ToSaveLog("CVRSDK.Status:"); break;
        //                }

        //            return status;
        //        }
        //        catch (Exception ex)
        //        {
        //            IrAdvanced.WriteError(ex.Message);
        //        }
        //        return -1;
        //    }
        //}

        #endregion

        public Cvr()
        {
            if (!Init())
            {
               "第二代身份证识别硬件失败".ToSaveLog();
            }
            else
            {
                //Thread t = new Thread(ReadLoop) { IsBackground = true };
                //t.Start();
                Timer timer=new Timer(1000) {AutoReset = true};
                timer.Elapsed += delegate { ReadData(); };
                timer.Start();
            }
        }




        /// <summary> 初始化CVR第二代身份证识别硬件 </summary>
        public bool Init()
        {
            try
            {
                 var port = IrAdvanced.ReadInt("CvrCom");
                port.ToString().ToSaveLog("尝试打开二代身份证硬件连接端口：");
                _iRetPort = Cvrsdk.Well_InitComm(port);
                if (_iRetPort != 1) "CVR串口打开失败".ToSaveLog();
                return _iRetPort == 1;    //返回是否初始化成功
            }
            catch (Exception ex)
            {
                ex.Message.ToSaveLog("初始化CVR第二代身份证识别硬件失败：");
            }
            return false;
        }
   
         /// <summary> 读取CVR第二代身份证信息 </summary>
        public void ReadData()
         {
             try
             {
               
                      if (_iRetPort == 1)
                     {
                         int authenticate = Cvrsdk.Well_Authenticate();
                         if (authenticate == 1)
                         {
                             int readContent = Cvrsdk.Well_ReadContent();
                             if (readContent == 1)
                             {
                                 FillData();
                                 Info.Number.ToSaveLog("读取CVR第二代身份证信息成功：");
                             }
                             else
                             {
                                 readContent.ToString().ToSaveLog("Well_ReadContent读第二代证失败：");
                             }
                         }
                         else
                         {
                             if(authenticate!=2) authenticate.ToString().ToSaveLog("Well_Authenticate卡认证失败：");
                         }
                     }
                     else
                     {
                         "硬件CVR第二代身份证：初始化失败！".ToSaveLog();
                     }
                
             }
             catch (Exception ex)
             {
                ex.ToSaveLog("ReadData:读取CVR第二代身份证信息：初始化失败！");
             }
         }

        private void FillData()
        {
            try
            {

                byte[] name = new byte[50];int length = 50;
                //Cvrsdk.Get_PeopleName(ref name[0], ref length);
                Cvrsdk.Well_GetName(ref name[0],ref length);

                byte[] number = new byte[50];length = 50;
               // Cvrsdk.GetPeopleIDCode(ref number[0], ref length);
                Cvrsdk.Well_GetIdCard(ref number[0], ref length);

                byte[] address = new byte[70];length = 70;
               // Cvrsdk.GetPeopleAddress(ref address[0], ref length);
                Cvrsdk.Well_GetAddress(ref address[0], ref length);

                Info = new CvrInfo
                {
                    Name = ByteToString(name),
                    Address = ByteToString(address),
                    Number = ByteToString(number),
                    PeopleImg = AppDomain.CurrentDomain.BaseDirectory + "pic.bmp",
                };
                OnGetNewDataEvent(Info);   //抛出事件读取成功
            }
            catch (Exception ex)
            {
               ex.ToSaveLog("FillData:读取CVR第二代身份证信息：！");
            }
        }

        private string ByteToString(byte[] bytes)
        {
           // return Encoding.GetEncoding("GB2312").GetString(bytes).Replace("\0", "").Trim();
            return Encoding.Default.GetString(bytes).Replace("\0", "").Trim();
        }

        public bool Close()
        {
            try
            {
                Cvrsdk.Well_CloseComm().ToString().ToSaveLog("closeCommSVR:");
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("close:CVR身份硬件关闭：");
            }
            return false;
        }

        public void Dispose()
        {
            Close();
        }

        
    }
}
