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
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.ICard;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.ModeView
{
    /// <summary> IC卡（人脸识别）+二维码识别模式 </summary>
    public partial class FaceIcView
    {
        #region 私有成员变量
        private Thread _icThread;
        private bool icPass = false;
        private DateTime? tempTime;
        #endregion


        #region 构造函数
        public FaceIcView()
        {
            InitializeComponent();
        }
        public override void Init()
        {
            base.Init();
            CameraPic1.Visibility = Visibility.Visible;
            Title.Text = MyMatch.SessionName;
            //创建线程循环请求IC卡
            if (!IdCardFunc.Port.IsNullOrEmpty())
            {
                _icThread = new Thread(OpenIc) { IsBackground = true };
                _icThread.Start();
            }
            "身份证+人脸识别+年票卡模式".ToSaveLog("【调用模式】:");
        }
        private void OpenIc()
        {
            "初始化IC卡硬件连接。".ToSaveLog("OpenICard:");
            while (true)
            {
                lock (Obj)
                {
                    try
                    {
                        if (!icPass && CvrPass && FacePass)
                        {
                            CheckICard();
                        }
                        else
                        {
                            Thread.Sleep(300);
                        }

                    }
                    catch (Exception ex)
                    {
                        ex.ToSaveLog();
                    }
                }
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
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceIdView.GetCamBitmap:");
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
                            CvrImgMain.Source = CvrImg.Source = item;
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

        #region 读写IC卡
        /// <summary> 读写IC卡 </summary>
        private int CheckICard()
        {
            FaceFun.TimeStart();
            var icInfo = new IdCardFunc.TicketCardInfo();
            int i = IdCardFunc.GetTicketData_New(ref icInfo);
            if (i == 0)
            {
                if (IsIdSame && (Cvr.Info.Number != icInfo.IDCard))
                {
                    ShowEventMsg("身份证号码与票面不符合！", MsgType.Info);
                }
                else
                {
                    tempTime = DateTime.Now;
                        //检查白名单验证
                        if (!CheckWhite(icInfo.IDCard)) return 1;
                        //写入本地记录并且通知
                        var info = new SessionsInfo
                        {
                            SessionId = MyMatch.SessionId,
                            CreateDate = DateTime.Now,
                            Name = MyMatch.SessionName,
                            IdNo = icInfo.IDCard,
                            IdCardPhoto = IrAdvanced.ReadBytesFromFile(FaceFun.BaseDir + "\\pic.bmp"),
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
                            ValidateType = 2,
                            SyncTime = null,
                            Status = 0,
                            Remark = "",
                            UserName = AftUserName,
                        };
                        if (FaceFun.AddSessions(info) > 0)
                        {
                            Dispatcher.InvokeAsync(() =>
                            {
                                ShowEventMsg("检票成功", MsgType.FaceOk);
                                MyArea.Text = string.Format("区域:{0}{1}排{2}座", info.Area, info.Row, info.Seat);
                                MyNum.Text = "票号:" + info.TicketNo;
                                MyType.Text = "类型:年票";
                                MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            });
                        }
                }
            }
            else if (i == 16)
            {
                i.ToString().ToSaveLog("已刷卡:");
                var str = (tempTime == null) ? "拒绝入场" : string.Format("{0}入场", tempTime);
                ShowEventMsg("一天只能入场一次!\n" + str, MsgType.InfoErr);
                ShowEventMsg("检票失败", MsgType.FaceErr);
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
            return i;
        }        
        #endregion

        #region 合法性验证
        /// <summary> 入场校验是否允许，true通过，false未通过 </summary>
        /// <param name="idNo">身份证</param>
        /// <param name="isIdNo">是是身份证号,否是票号</param>
        /// <returns>是否通过</returns>
        protected override bool CheckSession(string idNo, bool isIdNo = true)
        {
            return true;
        } 
        
        #endregion

        #region UI界面响应


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
                    case MsgType.InfoErr:
                        Tip2.Text = strMsg;
                        Tip2.Foreground = Brushes.Red;
                        break;
                    case MsgType.TipOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        Tip2.Text = "请将身份证放置刷卡感应区域!";
                        MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoOk);
                        break;
                    case MsgType.FaceOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        icPass = false;
                        HideShow(2);
                        break;
                    case MsgType.FaceErr:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.Red;
                        break;
                }

            });
        }
        protected override void Time_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Tip1.Text = "";
                Tip2.Text = "请将IC卡放置在识别区域！";
                MyArea.Text = "区域:";
                MyNum.Text = "票号:";
                MyType.Text = "类型";
                MyTime.Text = "时间:";
                CvrImgMain.Source = CvrImg.Source = MiniImg.Source = null;
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
            OnHardConn(string.Format("条码枪连接\t{0}\n", IdCardFunc.TestDevice(IdCardFunc.Port) == 0 ? "\t\t成功 √" : "失败 ×"));
            OnHardCompleted();
        }
        #endregion
    }
}
