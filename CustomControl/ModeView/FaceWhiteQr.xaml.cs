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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AFT_System.CVR;
using AFT_System.ICard;
using AFT_System.Public;
using FaceVerifyLib;
using Emgu.CV;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;
using System.Drawing.Imaging;

namespace AFT_System.CustomControl.ModeView
{
    /// <summary> FaceWhiteIc.xaml 的交互逻辑 </summary>
    public partial class FaceWhiteQr
    {
        #region 私有成员变量
        private Thread _icThread;
        private bool icPass = false;
        private rr_feature_t Rft2;    //图片特征码
        private WhiteNameInfo? white;
        /// <summary> 人脸比对结论：摄像头人脸识别置信度 </summary>
        protected float FaceIr { get; set; }
        /// <summary> 人脸识别取信值阀值 </summary>
        protected readonly float _confidence;

        private bool FacePass = false;
        private bool Compareing = false;
        protected readonly FaceVerifyFactory faceFactory;
        #endregion

        #region 构造函数
        public FaceWhiteQr()
        {
            InitializeComponent();
            _confidence = IrAdvanced.ReadInt("Confidence", 85) / 100.0f;   //可信度
            try
            {
                faceFactory = new FaceVerifyFactory();
            }
            catch(Exception ex)
            {

            }
        }

        public override void Init()
        {
            try
            {
                base.Init();
                CameraPic2.Visibility = Visibility.Visible;
                //Title.Text = MyMatch.SessionName;
                //创建线程循环请求IC卡
                //if (!IdCardFunc.Port.IsNullOrEmpty())
                //{
                //    _icThread = new Thread(OpenIc) { IsBackground = true };
                //    _icThread.Start();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            "人脸识别+散票模式".ToSaveLog("【调用模式】:");
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
                        if (!icPass)
                        {
                            IrAdvanced.DoEvents(CheckICard());
                        }
                        else
                        {
                            Thread.Sleep(1000);
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
        DateTime sucessTime;
        protected override void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                MyMat = null;
                MyMat = new Mat();
                if (MyCapture == null) return;
                MyCapture.Retrieve(MyMat, 0);
                //摄像头视频流输出
                Dispatcher.Invoke(() =>
                {
                    CameraPic2.ImageView2 = null;
                    CameraPic2.ImageView2 = MyMat.Bitmap;
                });
                System.Threading.Thread.Sleep(40);//每秒25帧

                if (FacePass)
                {
                    if (sucessTime.AddSeconds(1.5) < DateTime.Now)
                    {
                        FacePass = false;
                    }
                    else
                        return;
                }
                if (AFT_System.Face.FaceFun.WhiteList != null && !Compareing)
                {
                    Compareing = true;
                    Thread thread = new Thread(FaceCompare);
                    thread.Start();
                }

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("Capture_ImageGrabbed:");
            }
        }

        private void FaceCompare()
        {
            Dispatcher.InvokeAsync(() =>
            {
                ShowEventMsg("识别中", MsgType.TipErr);
                this.MyName.Text = "购票者:";
                this.MyTime.Text = "检票时间:";
                CvrImg.Source = null;
            });
            try
            {
                "开始识别".ToSaveLog("FaceVerify");
                FaceVerifyData faceVerifyData = faceFactory.FaceVerify(MyMat);

                "识别完成".ToSaveLog("FaceVerify");
                if (faceVerifyData.Success)
                {
                    DateTime checkTime = DateTime.Now;
                    foreach (WhiteNameInfo item in AFT_System.Face.FaceFun.WhiteList)
                    {
                        FaceIr = faceFactory.FaceResult(faceVerifyData.rr_Feature, item.rrfeature);
                        if (FaceIr > _confidence)
                        {
                            if (AFT_System.Face.FaceFun.IsInSessions(item.IdNo, MyMatch.SessionId,out checkTime))
                            {
                                Dispatcher.InvokeAsync(() =>
                                {
                                    CvrImg.Source = null;
                                    CvrImg.Source = new BitmapImage(new Uri(item.IdCardPhotoPath, UriKind.Absolute));
                                    this.MyName.Text = "购票者:" + item.BuyName;
                                    this.MyTime.Text = "检票时间:" + checkTime.ToString("yyyy-MM-dd hh:mm");
                                    ShowEventMsg("该人员已检票", MsgType.TipErr);
                                });
                            }
                            else {
                                Dispatcher.InvokeAsync(() =>
                                {
                                    CvrImg.Source = null;
                                    CvrImg.Source = new BitmapImage(new Uri(item.IdCardPhotoPath, UriKind.Absolute));

                                    //CameraPic2.ImageView2 = null;
                                    //CameraPic2.ImageView2 = Face.FaceBitmapSource;


                                    //保存识别到的摄像头图片
                                    item.IdNo.ToSaveLog("保存检票成功照片:");
                                    PhotoOk = FaceFun.BitmapToByte(MyMat.Bitmap);
                                    ShowEventMsg("比对成功", MsgType.FaceOk);
                                    FaceIr.ToString().ToSaveLog(item.IdNo + " 比对成功，相似度：");
                                    if (Cvr != null)
                                    {
                                        Cvr.Info = new CvrInfo()
                                        {
                                            Name = item.BuyName,
                                            Number = item.IdNo,
                                            PeopleImg = item.IdCardPhotoPath
                                        };
                                        this.MyName.Text = "购票者:" + item.BuyName;
                                        this.MyTime.Text = "检票时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm");
                                        InSession();
                                    }
                                });
                            }
                            FacePass = true; //当取信值大于80%认为是同一个人比对成功     
                            sucessTime = DateTime.Now;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                ex.ToSaveLog("OnComplete：");
            }
            Compareing = false;
        }
        #endregion

        #region 读写IC卡
        /// <summary> 读写IC卡 </summary>
        private int CheckICard()
        {
            AFT_System.Face.FaceFun.TimeStart();
            var icInfo = new IdCardFunc.TicketCardInfo();
            int i = IdCardFunc.GetTicketData_New(ref icInfo);
            if (i == 0)
            {
                if (icInfo.IDCard.IsNullOrEmpty()) { "读卡成功,但身份证号码为空.".ToSaveLog(); return 1; }
                //黑名单检查
                if (IsCheckBlack && AFT_System.Face.FaceFun.IsInBlack(icInfo.IDCard)) LeftImg.Visibility = Visibility.Visible;
                //判断是否超过时限
                var now = DateTime.Now;
                var time = IrAdvanced.StringToDateTime(icInfo.Reserved2);
                if ((time.Year <= now.Year && time.Month <= now.Month && time.Day < now.Day))
                {
                    //读取数据库白名单记录
                    white = AFT_System.Face.FaceFun.CheckWhiteName(icInfo.IDCard);
                    if (white != null)
                    {
                        Bitmap photobitmap = IrAdvanced.GetBitmapFormByte(white.Value.IdCardPhoto);
                        Dispatcher.Invoke(() =>
                        {
                            BitmapImage photoImg = IrAdvanced.GetBitmapImageFormByte(white.Value.IdCardPhoto);
                            CvrImgMain.Source = CvrImg.Source = photoImg;
                        });
                        FaceVerifyFactory faceFactory = new FaceVerifyFactory();
                        FaceVerifyData faceVerifyData = faceFactory.FaceVerify(photobitmap);
                        Rft2 = faceVerifyData.rr_Feature;
                        if (Face.TzIr.ToInt32() == 0)
                        {
                            icPass = true;
                            ShowEventMsg("正在进行人脸识别", MsgType.Info);
                        }
                        else
                        {
                            ShowEventMsg("无法识别该照片人脸特征码", MsgType.InfoErr);
                        }
                    }
                    else
                    {
                        ShowEventMsg("白名单不存在此年票记录.", MsgType.InfoErr);
                    }
                }
                else
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        ShowEventMsg(string.Format("一天只能入场一次!\n{0}已使用", icInfo.Reserved2), MsgType.InfoErr);
                        MyArea.Text = string.Format("区域:{0}区{1}排{2}座", icInfo.StadiumArea, icInfo.Row, icInfo.Position);
                        MyNum.Text = "票号:" + icInfo.CardNo;
                        MyType.Text = "类型:年票";
                        MyTime.Text = "时间:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ShowEventMsg("检票失败", MsgType.FaceErr);
                    });
                }

            }
            AFT_System.Face.FaceFun.TimeStop("IC读写耗时:");
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
                        break;
                    case MsgType.InfoErr:
                        break;
                    case MsgType.TipOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
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
                MyArea.Text = "区域:";
                MyNum.Text = "票号:";
                MyType.Text = "类型";
                MyTime.Text = "时间:";
                CvrImgMain.Source = CvrImg.Source = null;
                //CvrImgMain.Source = CvrImg.Source = MiniImg.Source = null;
                LeftImg.Visibility = Visibility.Hidden;
                icPass = false;
            });

        }
        #endregion
        #region 测试硬件连接
        protected override void TestHardConn()
        {
            base.TestHardConn();
            OnHardCompleted();
        }
        #endregion

        #region 写入数据库记录集
        /// <summary> 写入数据库 </summary>
        protected override void InSession()
        {
            try
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
                    TicketType = 2,
                    TicketNo = null,
                    Area = null,
                    Row = null,
                    Seat = null,
                    TelNo = IrAdvanced.ReadString("TelNo"),
                    TelArea = IrAdvanced.ReadString("TelArea"),
                    BuyName = Cvr.Info.Name,
                    BuyPhoto = null,
                    BuyDate = null,
                    ValidateType = 1,
                    SyncTime = null,
                    Status = 0,
                    Remark = "",
                    UserName = AftUserName,
                };

                AFT_System.Face.FaceFun.AddSessions(info);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("InSession");
            }

        }
        #endregion

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            base.Init();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MyCapture.Dispose();
            MyCapture = null;
        }
    }
}
