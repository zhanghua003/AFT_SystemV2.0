using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.Public;
using Emgu.CV;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.ModeView
{
    /// <summary> 身份证（人脸识别）+二维码识别模式 </summary>
    public partial class FaceQrView
    {
        #region 私有成员
        private QrInfo? _qrInfo;
        private IrSerialPort _irSeialPort;
        #endregion

        #region 构造函数
        public FaceQrView()
        {
            InitializeComponent();
        }
        public override void Init()
        {
            base.Init();
            CameraPic1.Visibility = Visibility.Visible;
            Title.Text = MyMatch.SessionName;
            "身份证+人脸识别+散票验证模式".ToSaveLog("【调用模式】:");
            _irSeialPort = new IrSerialPort(IrAdvanced.ReadString("BarCodePort"));
            _irSeialPort.DataReceived += irSeialPort_DataReceived;
            _irSeialPort.ErrorEvent += (sender, e) => ShowEventMsg(e, MsgType.TipErr);
        }
        #endregion

        #region 二维码扫描
        void irSeialPort_DataReceived(object sender, string e)
        {
            try
            {

                _qrInfo = OnlyQr.ReadData(e, MyMatch.Key);
                if (_qrInfo != null)
                {
                    //白名单校验
                    var white = CheckWhite(_qrInfo.Value.IdNo);
                    //入场校验
                    var session = CheckSession(_qrInfo.Value.TicketNo, false);
                    if (white && session)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MyArea.Text = string.Format("区域:{0}{1}排{2}座", _qrInfo.Value.Area, _qrInfo.Value.Row, _qrInfo.Value.Seat);
                            MyNum.Text = "票号:" + _qrInfo.Value.TicketNo;
                            MyType.Text = "类型:散票";
                            MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        });
                       
                        //取得人脸识别特征码
                        Mat myMap = MyCapture.QueryFrame();
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
        #endregion

        #region 人脸识别处理
        protected override void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            base.Capture_ImageGrabbed(sender, e);
            try
            {
                if (FaceOut >= FaceOutCount)
                {
                    ShowEventMsg("对比失败", MsgType.FaceErr);
                    Dispatcher.InvokeAsync(() => { MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoErr); });
                }
                //摄像头视频流输出
                Dispatcher.Invoke(() => { CameraPic1.ImageView = CvrPass && !FacePass ? Face.FaceBitmapSource : MyMat.Bitmap; });

                if (CvrPass && FacePass && _qrInfo != null)
                {
                    if (IsIdSame && (Cvr.Info.Number != _qrInfo.Value.IdNo))
                    {
                        ShowEventMsg("身份证号码与票面不符合！", MsgType.Info);
                    }
                    else
                    {
                        InSession(); //写入本地记录并且通知
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceIdView.Capture_ImageGrabbed:");
            }
        }
        #endregion

        #region 二代身份证识别
        //二代身份证识别触发
        protected override void Cvr_GetNewDataEvent(object sender, CvrInfo e)
        {
            base.Cvr_GetNewDataEvent(sender, e);
            try
            {
                if (CvrPass)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        //黑名单检查
                        if (IsCheckBlack && FaceFun.IsInBlack(e.Number)) LeftImg.Visibility = Visibility.Visible;
                        var item = IrAdvanced.GetBitmapImageFromFile(e.PeopleImg);
                        if (item != null)
                        {
                            CvrMain.Source = CvrImg.Source = item;
                            ShowEventMsg("正在识别人脸特征！", MsgType.Info);
                            FaceOut = 0;
                            CvrPass = true;
                        }
                        else
                        {
                            CvrPass = false;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog();
                CvrPass = false;
            }
        }        
        #endregion

        #region 写入数据库
        /// <summary> 写入本地记录并且通知 </summary>
        protected override void InSession()
        {
            //写入本地记录并且通知
            var info = new SessionsInfo
            {
                SessionId = MyMatch.SessionId,
                CreateDate = DateTime.Now,
                Name = MyMatch.SessionName,
                IdNo = Cvr.Info.Number,
                IdCardPhoto = IrAdvanced.ReadBytesFromFile(FaceFun.BaseDir + "\\pic.bmp"),
                TakePhoto = PhotoOk,
                FaceData = FaceFun.StructToBytes(CameraRft),
                IdAddress = Cvr.Info.Address,
                TicketType = 0,
                TicketNo = _qrInfo.Value.TicketNo,
                Area = _qrInfo.Value.Area,
                Row = _qrInfo.Value.Row,
                Seat = _qrInfo.Value.Seat,
                TelNo = IrAdvanced.ReadString("TelNo"),
                TelArea = IrAdvanced.ReadString("TelArea"),
                BuyName = Cvr.Info.Name,
                BuyPhoto = null,
                BuyDate = null,
                ValidateType =3,
                SyncTime = null,
                Status = 0,
                Remark = "",
                UserName = AftUserName,
            };

            if (FaceFun.AddSessions(info) > 0)
            {
                ShowEventMsg("检票成功", MsgType.FaceOk);
            }
            else
            {
                "写入数据库失败".ToSaveLog("入场记录时：");
                ShowEventMsg("检票失败", MsgType.FaceErr);
            }
        }   
        #endregion
 
        #region UI界面响应
        /// <summary> 入场校验是否允许 </summary>
        protected override bool CheckSession(string idNo, bool isIdNo = true)
        {
            var pass = base.CheckSession(idNo, isIdNo);
            if (!pass)
            {
                ShowEventMsg(string.Format("该客户已经入场！"), MsgType.Info);
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

        protected override void ShowEventMsg(string strMsg, MsgType type)
        {
            base.ShowEventMsg(strMsg, type);
            Dispatcher.InvokeAsync(() =>
            {
                switch (type)
                {
                    case MsgType.TipErr:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.Red;
                        break;
                    case MsgType.Info:
                        Tip2.Text = strMsg;
                        Tip2.Foreground = Brushes.GreenYellow;
                        break;
                    case MsgType.TipOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoOk);
                        Tip2.Text = "请将二维码票面放置在识别区域!";
                        break;
                    case MsgType.FaceOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        HideShow(2);
                        break;
                    case MsgType.FaceErr:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.Red;
                        break;
                }

                if (type == MsgType.FaceOk) _qrInfo = null;
            });
        }
        protected override void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Tip1.Text = "";
                Tip2.Text = "请将二代身份证放置在识别区域！";
                MyArea.Text = "区域:";
                MyNum.Text = "票号:";
                MyType.Text = "类型";
                MyTime.Text = "时间:";
                LeftImg.Visibility = Visibility.Hidden;
                CvrMain.Source = CvrImg.Source = MiniImg.Source = null;
                _qrInfo = null;
            });

        }
        public override void Dispose()
        {
            base.Dispose();
            if (_irSeialPort != null) _irSeialPort.Dispose();
        }
        #endregion
        #region 测试硬件连接
        protected override void TestHardConn()
        {
            base.TestHardConn();
            //测试条码枪模块
            OnHardConn(string.Format("条码枪连接\t{0}\n", TestQr() ? "\t\t成功 √" : "失败 ×"));
            OnHardCompleted();
        }
        #endregion

    }
}
