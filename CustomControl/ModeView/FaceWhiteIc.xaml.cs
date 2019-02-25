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
using AFT_System.Face;
using AFT_System.ICard;
using AFT_System.Public;
using Emgu.CV;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.ModeView
{
    /// <summary> FaceWhiteIc.xaml 的交互逻辑 </summary>
    public partial class FaceWhiteIc
    {
        #region 私有成员变量
        private Thread _icThread;
        private bool icPass = false;
        private RrFeatureT Rft2;    //图片特征码
        private WhiteNameInfo? white;
        /// <summary> 人脸比对结论：摄像头人脸识别置信度 </summary>
        protected float FaceIr { get; set; }
        /// <summary> 人脸识别取信值阀值 </summary>
        protected readonly float _confidence;

        private bool FacePass = false;
        #endregion

        #region 构造函数
        public FaceWhiteIc()
        {
            InitializeComponent();
            _confidence = IrAdvanced.ReadInt("Confidence", 65) / 100.0f;   //可信度
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
        protected override void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                MyMat = null;
                MyMat = new Mat();
                if (MyCapture == null) return;
                MyCapture.Retrieve(MyMat, 0);
                if (icPass&&white!=null)
                {
                    FaceOut++;
                    if (FaceOut >= FaceOutCount)
                    {
                       white.Value.IdNo.ToSaveLog("比对超时,比对失败:");
                        PhotoErr = FaceFun.BitmapToByte(MyMat.Bitmap);
                        icPass = false;
                        ShowEventMsg("对比失败", MsgType.FaceErr);
                        Dispatcher.InvokeAsync(() => { MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoErr); });
                        ShowEventMsg("失败：当前用户与身份证对比失败", MsgType.Info);
                    }
                    else
                    {
                    ShowEventMsg("识别中" + (FaceOutCount - FaceOut), MsgType.TipErr);
                    CameraRft = Face.FaceTz(MyMat);
                    if (Face.VerifyIr.ToInt32() == 0)
                    {
                        FaceIr = FaceFun.FaceResult(CameraRft, Rft2);
                        if (FaceIr > _confidence)
                        {
                            try
                            {
                                //保存识别到的摄像头图片
                                white.Value.IdNo.ToSaveLog("保存检票成功照片:");
                                //  MyMat.Bitmap.Save(string.Format("{0}{1}.jpg", CamPath, Cvr.Info.Number),ImageFormat.Jpeg);
                                PhotoOk = FaceFun.BitmapToByte(MyMat.Bitmap);
                                ShowEventMsg("比对成功", MsgType.TipOk);
                                FaceIr.ToString().ToSaveLog(white.Value.IdNo + " 比对成功，相似度：");
                                FacePass = true; //当取信值大于80%认为是同一个人比对成功
                               
                            }
                            catch (Exception ex)
                            {
                                ex.ToSaveLog("OnComplete：");
                            }
                        }
                    }                        
                    }

                }
                //摄像头视频流输出
                Dispatcher.Invoke(() => { CameraPic1.ImageView = icPass && !FacePass ? Face.FaceBitmapSource : MyMat.Bitmap; });
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("Capture_ImageGrabbed:");
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
                if (icInfo.IDCard.IsNullOrEmpty()){"读卡成功,但身份证号码为空.".ToSaveLog();return 1;}
                //黑名单检查
                if (IsCheckBlack && FaceFun.IsInBlack(icInfo.IDCard)) LeftImg.Visibility = Visibility.Visible;
                //判断是否超过时限
                    var now = DateTime.Now;
                    var time = IrAdvanced.StringToDateTime(icInfo.Reserved2);
                    if ((time.Year <= now.Year && time.Month <= now.Month && time.Day < now.Day))
                    {
                            //读取数据库白名单记录
                            white= FaceFun.CheckWhiteName(icInfo.IDCard);
                            if (white != null)
                            {
                                Bitmap photobitmap = IrAdvanced.GetBitmapFormByte(white.Value.IdCardPhoto);
                                Dispatcher.Invoke(() =>
                                {
                                    BitmapImage photoImg = IrAdvanced.GetBitmapImageFormByte(white.Value.IdCardPhoto);
                                    CvrImgMain.Source = CvrImg.Source = photoImg;
                                });
                                Rft2 = Face.FaceTz(photobitmap);
                                if (Face.TzIr.ToInt32() == 0)
                                {
                                    icPass = true;
                                    ShowEventMsg("正在进行人脸识别",MsgType.Info);
                                }
                                else
                                {
                                    ShowEventMsg("无法识别该照片人脸特征码",MsgType.InfoErr);
                                }
                            }
                            else
                            {
                                ShowEventMsg("白名单不存在此年票记录.",MsgType.InfoErr);
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
                        Tip2.Text = "请将IC卡放置刷卡感应区域!";
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
    }
}
