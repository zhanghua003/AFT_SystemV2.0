using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AFT_System.CustomControl.Mode;
using AFT_System.CustomControl.ModeView.Modle;
using AFT_System.Face;
using AFT_System.ICard;
using AFT_System.Public;
using Emgu.CV;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.ModeView
{

    /// <summary> 年卡验证，匹配年卡合法且摄像头拍照取证 </summary>
    public partial class OnlyIcView
    {
        private Thread _icThread;
        private bool icPass = false;
        private DateTime? tempTime;
        #region 构造函数
        public OnlyIcView()
        {
            InitializeComponent();
        }

        public override void Init()
        {
            base.Init();
            Title.Text = MyMatch.SessionName;
            //创建线程循环请求IC卡
            if (!IdCardFunc.Port.IsNullOrEmpty())
            {
                _icThread = new Thread(OpenIc) { IsBackground = true };
                _icThread.Start();
            }
            "年卡验证模式".ToSaveLog("【调用模式】:");
        }

        private void OpenIc()
        {
            "初始化IC卡硬件连接。".ToSaveLog("OpenICard:");
            while (true)
            {
                try
                  {
                    Thread.Sleep(300);
                     CheckICard();
                    }
                    catch (Exception ex)
                    {
                        ex.ToSaveLog();
                    }
            }
        }
        /// <summary> 读写IC卡 </summary>
        private int CheckICard()
        {

            try
            {
                "准备请求读卡".ToSaveLog("CheckIcard:");
                FaceFun.TimeStart();
                var icInfo = new IdCardFunc.TicketCardInfo();
                int i = IdCardFunc.GetTicketData_New(ref icInfo);
                if (i == 0)
                {
                    tempTime = DateTime.Now;
                    icInfo.IDCard.ToSaveLog("读取到IC卡:");
                    //判断是否超过时限
                    if (!icInfo.IDCard.IsNullOrEmpty()) Dispatcher.Invoke(() => MyIdNo.Text = icInfo.IDCard);
                        var flag = true;
                        Dispatcher.Invoke(() =>
                        { //黑名单检查
                            if (IsCheckBlack && FaceFun.IsInBlack(MyIdNo.Text)) LeftImg.Visibility = Visibility.Visible;
                            if (IsCheckIdNo && MyIdNo.Text.IsNullOrEmpty()) flag = false;
                        });
                        if (!flag) { ShowEventMsg("请录入身份证号码.", MsgType.TipErr); return 1; }
                        //检查白名单验证
                        if (!CheckWhite(icInfo.IDCard)) return 1;
                        PhotoOk = null;
                        lock (Obj)
                        {
                            if (MyCapture != null)
                            {
                                "拍照获取Photo.".ToSaveLog("");
                                using (var myMap = MyCapture.QueryFrame())
                                {
                                    if (myMap != null)
                                    {
                                        PhotoOk = FaceFun.BitmapToByte(myMap.Bitmap);
                                    }
                                    else
                                    {
                                        "未取得摄像头Mat数据".ToSaveLog("OnlyQrView.BarCode_OnKeyUp:");
                                    }
                                }                                  
                            }
                          
                        }
                        //写入本地记录并且通知
                        var info = new SessionsInfo
                        {
                            SessionId = MyMatch.SessionId,
                            CreateDate = DateTime.Now,
                            Name = MyMatch.SessionName,
                            IdNo = icInfo.IDCard,
                            IdCardPhoto = null,
                            TakePhoto = PhotoOk,
                            FaceData = null,
                            IdAddress = "",
                            TicketType = 1,
                            TicketNo = icInfo.CardNo,
                            Area = icInfo.StadiumArea,
                            Row = icInfo.Row,
                            Seat = icInfo.Position,
                            TelNo = IrAdvanced.ReadString("TelNo"),
                            TelArea = IrAdvanced.ReadString("TelArea"),
                            BuyName = icInfo.Name,
                            BuyPhoto = null,
                            BuyDate = null,
                            ValidateType = 4,
                            SyncTime = null,
                            Status = 0,
                            Remark = "",
                            UserName = AftUserName,
                        };
                        if (FaceFun.AddSessions(info) > 0)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                MyArea.Text = string.Format("区域:{0}区{1}排{2}座", info.Area, info.Row, info.Seat);
                                MyNum.Text = "票号:" + info.TicketNo;
                                MyType.Text = "类型:年票";
                                MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            });
                            ShowEventMsg("检票成功", MsgType.FaceOk);
                            Thread.Sleep(500);
                        }

                    FaceFun.TimeStop("IC读写耗时:");
                }
                else if(i==16)
                {
                    i.ToString().ToSaveLog("已刷卡:");
                    var str = (tempTime == null) ? "拒绝入场" : string.Format("{0}入场", tempTime);
                    ShowEventMsg("一天只能入场一次!\n" + str, MsgType.FaceErr);
                    Thread.Sleep(500);
                }
                else if (i == 1)    //未放卡
                {
                    tempTime = null;
                }
                else
                {
                    i.ToString().ToSaveLog("IC卡读卡失败:");
                    Thread.Sleep(300);
                }

                icPass = false;
                return i;
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("CheckICard失败:");
            }
            icPass = false;
            return 0;

        }  
        #endregion
  

        #region UI界面响应
        protected override void ShowEventMsg(string strMsg, MsgType type)
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    base.ShowEventMsg(strMsg, type);
                    switch (type)
                    {
                        case MsgType.TipErr:
                            Tip.Text = strMsg;
                            Tip.Foreground = Brushes.Red;
                            break;
                        case MsgType.Info:
                            Tip.Text = strMsg;
                            Tip.Foreground = Brushes.GreenYellow;
                            break;
                        case MsgType.FaceOk:
                            Tip.Text = strMsg;
                            Tip.Foreground = Brushes.White;
                            HideShow(2);
                            break;
                        case MsgType.FaceErr:
                            Tip.Text = strMsg;
                            Tip.Foreground = Brushes.Red;
                            break;
                    }
                });
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("OnlyIcView.ShowEventMsg");
            }

        }
        protected override void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Tip.Text = "就绪!\n 等待读取IC进行检票.";
                MyArea.Text = "区域:";
                MyNum.Text = "票号:";
                MyType.Text = "类型";
                MyTime.Text = "时间:";
                MyIdNo.Text = "";
                LeftImg.Visibility = Visibility.Hidden;
                icPass = false;
            });

        }
        #endregion
        #region 测试硬件连接
        protected override void TestHardConn()
        {
            base.TestHardConn();
            //测试IC卡模块
            OnHardConn(string.Format("条码枪连接\t{0}\n", IdCardFunc.TestDevice(IdCardFunc.Port)==0 ? "\t\t成功 √" : "失败 ×"));
            OnHardCompleted();
        }
        #endregion
        private void Minikeyboard_OnChanged(object sender, string str)
        {
            if (str == "退格")
            {
                var x = MyIdNo.Text.Length - 1;
                if (x < 0) return;
                MyIdNo.Text = MyIdNo.Text.Remove(x);
            }
            else
            {
                MyIdNo.Text += str;
            }
        }
    }
}
