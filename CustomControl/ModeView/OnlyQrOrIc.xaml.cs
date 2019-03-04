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

    /// <summary> OnlyQrOrIc.xaml 的交互逻辑 </summary>
    public partial class OnlyQrOrIc
    {
        #region 私有成员
        private QrInfo? _qrInfo;
        private DateTime? tempTime;
        private string _barIdNo;
        public string BarIdNo
        {
            get { return _barIdNo; }
            set
            {
                _barIdNo = value;
                Dispatcher.InvokeAsync(() => MyIdNo.Text = _barIdNo);
            }
        }
        private IrSerialPort _irSeialPort;
        private Thread _icThread;
        private bool icPass = false;
        private bool useQr = false;
        #endregion

        #region 构造函数
        public OnlyQrOrIc()
        {
            InitializeComponent();
        }

        public override void Init()
        {
            base.Init();
            Title.Text = MyMatch.SessionName;
            "散票兼容IC年卡验证模式".ToSaveLog("【调用模式】:");
            //创建线程循环请求IC卡
            if (!IdCardFunc.Port.IsNullOrEmpty())
            {
                _icThread = new Thread(OpenIc) { IsBackground = true };
                _icThread.Start();
            }
            _irSeialPort = new IrSerialPort(IrAdvanced.ReadString("BarCodePort"));
            _irSeialPort.DataReceived += irSeialPort_DataReceived;
            _irSeialPort.ErrorEvent += (sender, e) => ShowEventMsg(e, MsgType.TipErr);
        }
        #endregion

        #region IC年卡验证模式
        private void OpenIc()
        {
            "初始化IC卡硬件连接。".ToSaveLog("OpenICard:");
            try
            {
                while (true)
                {
                    lock (Obj)
                    {
                        try
                        {
                            if (!icPass && !useQr)
                            {
                                CheckICard();
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }

                        }
                        catch (Exception ex)
                        {
                            ex.ToSaveLog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("OpenIc:");
            }

        }
        /// <summary> 读写IC卡 </summary>
        private int CheckICard()
        {
            var icInfo = new IdCardFunc.TicketCardInfo();
            try
            {
                "准备请求读卡".ToSaveLog("CheckIcard:");
                FaceFun.TimeStart();
                int i = IdCardFunc.GetTicketData_New(ref icInfo);
                if (i == 0)
                {
                    icInfo.IDCard.ToSaveLog("读取到IC卡:");
                    tempTime = DateTime.Now;

                        if (!icInfo.IDCard.IsNullOrEmpty()) Dispatcher.Invoke(() => MyIdNo.Text = icInfo.IDCard);
                        var flag = true;
                        Dispatcher.Invoke(() =>
                        {
                            //黑名单检查
                            if (IsCheckBlack && FaceFun.IsInBlack(MyIdNo.Text)) LeftImg.Visibility = Visibility.Visible;
                            if (IsCheckIdNo && MyIdNo.Text.IsNullOrEmpty()) flag = false;
                        });
                        if (!flag) { ShowEventMsg("请录入身份证号码.", MsgType.TipErr); return 1; }
                        //检查白名单验证
                        if (!CheckWhite(icInfo.IDCard)) return 1;
                        PhotoOk = null;
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

                        //写入本地记录并且通知
                        var info = new SessionsInfo
                        {
                            SessionId = MyMatch.SessionId,
                            CreateDate = DateTime.Now,
                            Name = MyMatch.SessionName,
                            IdNo = icInfo.IDCard,
                            IdCardPhoto = null,
                            TakePhoto = PhotoOk,
                            FaceData = FaceFun.StructToBytes(CameraRft),
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
                            ValidateType =7,
                            SyncTime = null,
                            Status = 0,
                            Remark = "",
                            UserName = AftUserName,
                        };
                        if (FaceFun.AddSessions(info) > 0)
                        {
                            info.IdNo.ToSaveLog("写入场流水记录:");
                            Dispatcher.Invoke(() =>
                            {
                                MyArea.Text = string.Format("区域:{0}区{1}排{2}座", info.Area, info.Row, info.Seat);
                                MyNum.Text = "票号:" + info.TicketNo;
                                MyType.Text = "类型:年票";
                                MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                MyIdNo.Text = info.IdNo;
                            });
                            ShowEventMsg("检票成功", MsgType.FaceOk);
                        }
                }
                else if (i == 16)
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
                FaceFun.TimeStop("IC读写耗时:");
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

        #region 二维码扫码结果
        void irSeialPort_DataReceived(object sender, string e)
        {
            try
            {

                _qrInfo = OnlyQr.ReadData(e, MyMatch.Key);
                if (_qrInfo != null)
                {
                    if (!_qrInfo.Value.IdNo.IsNullOrEmpty()) BarIdNo = _qrInfo.Value.IdNo;
                    //黑名单检查
                    if (IsCheckBlack && FaceFun.IsInBlack(BarIdNo)) LeftImg.Visibility = Visibility.Visible;
                    //校验身份证是否为空
                    if (IsCheckIdNo && BarIdNo.IsNullOrEmpty()) { ShowEventMsg("请录入身份证号码.", MsgType.TipErr); return; }
                    //白名单校验
                    var white = CheckWhite(_qrInfo.Value.IdNo);
                    //入场校验
                    var session = CheckSession(_qrInfo.Value.TicketNo, false);
                    if (white && session)
                    {
                        //取得人脸识别特征码
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
                        InSession();
                    }
                }
                else
                {
                    ShowEventMsg("检票失败,解码未通过.", MsgType.FaceErr);
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("BarCode_OnKeyUp:");
            }
        }
      /// <summary> 入场校验是否允许 </summary>
        protected override bool CheckSession(string idNo, bool isIdNo = true)
        {
            var pass = base.CheckSession(idNo, isIdNo);
            if (!pass)
            {
                ShowEventMsg(string.Format("该客户已经入场！"), MsgType.FaceErr);
                Dispatcher.InvokeAsync(() =>
                {
                    if (_qrInfo != null)
                    {
                        MyArea.Text = string.Format("区域:{0}{1}排{2}座", _qrInfo.Value.Area, _qrInfo.Value.Row, _qrInfo.Value.Seat);
                        MyNum.Text = "票号:" + _qrInfo.Value.TicketNo;
                        MyType.Text = "类型:散票";
                        MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    }

                });
            }
            return pass;
        }

        protected override void InSession()
        {
            try
            {

                if (_qrInfo != null)
                {
                    //写入本地记录并且通知
                    var info = new SessionsInfo
                    {
                        SessionId = MyMatch.SessionId,
                        CreateDate = DateTime.Now,
                        Name = MyMatch.SessionName,
                        IdNo = BarIdNo,
                        IdCardPhoto = null,
                        TakePhoto = PhotoOk,
                        FaceData = FaceFun.StructToBytes(CameraRft),
                        IdAddress = null,
                        TicketType = 0,
                        TicketNo = _qrInfo.Value.TicketNo,
                        Area = _qrInfo.Value.Area,
                        Row = _qrInfo.Value.Row,
                        Seat = _qrInfo.Value.Seat,
                        TelNo = IrAdvanced.ReadString("TelNo"),
                        TelArea = IrAdvanced.ReadString("TelArea"),
                        BuyName = "",
                        BuyPhoto = null,
                        BuyDate = null,
                        ValidateType = 7,
                        SyncTime = null,
                        Status = 0,
                        Remark = "",
                        UserName = AftUserName,
                    };

                    if (FaceFun.AddSessions(info) > 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MyArea.Text = string.Format("区域:{0}{1}排{2}座", info.Area, info.Row, info.Seat);
                            MyNum.Text = "票号:" + info.TicketNo;
                            MyType.Text = "类型:散票";
                            MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            ShowEventMsg("检票成功", MsgType.FaceOk);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("InSession");
            }


        }
        #endregion

        #region UI界面响应
        protected override void ShowEventMsg(string strMsg, MsgType type)
        {
            Dispatcher.Invoke(() =>
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
        protected override void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Tip.Text = "就绪!\n 等待扫描票面二维码进行检票.";
                MyArea.Text = "区域:";
                MyNum.Text = "票号:";
                MyType.Text = "类型";
                MyTime.Text = "时间:";
                MyIdNo.Text = "";
                LeftImg.Visibility = Visibility.Hidden;
            });

        }
        public override void Dispose()
        {
            base.Dispose();
            if (_irSeialPort != null) _irSeialPort.Dispose();
            if (_icThread != null) _icThread.Abort();
        }
        #endregion

        #region 测试硬件连接
        protected override void TestHardConn()
        {
            base.TestHardConn();
            #region 2019年3月屏蔽检查模块
            ////测试IC卡模块
            //OnHardConn(string.Format("条码枪连接\t{0}\n", IdCardFunc.TestDevice(IdCardFunc.Port) == 0 ? "\t\t成功 √" : "失败 ×"));
            ////测试条码枪模块
            //OnHardConn(string.Format("条码枪连接\t{0}\n", TestQr() ? "\t\t成功 √" : "失败 ×"));
            #endregion
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
