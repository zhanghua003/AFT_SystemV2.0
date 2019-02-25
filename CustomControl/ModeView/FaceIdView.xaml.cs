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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AFT_System.CustomControl.Mode;
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.ModeView
{
    /// <summary> 身份证验证，身份证人脸识别模式 </summary>
    public partial class FaceIdView
    {
        #region 构造函数
        public FaceIdView()
        {
            InitializeComponent();
        }
        //测试方法
        public override void Init()
        {
            base.Init();
            CameraPic1.Visibility = Visibility.Visible;
            Title.Text = MyMatch.SessionName;
            "身份证人脸识别模式".ToSaveLog("【调用模式】:");
        }
        #endregion

        #region 人脸识别处理
        protected override void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            base.Capture_ImageGrabbed(sender,e);
            try
            {
                if (FaceOut >= FaceOutCount)
                {
                    ShowEventMsg("对比失败", MsgType.FaceErr);
                    Dispatcher.InvokeAsync(() => { MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoErr); });
                }
                //摄像头视频流输出
                Dispatcher.Invoke(() => { CameraPic1.ImageView = CvrPass && !FacePass ? Face.FaceBitmapSource : MyMat.Bitmap; });
                //保存数据
                if (FacePass && CvrPass)   InSession();  //写入本地记录并且通知 
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceIdView.Capture_ImageGrabbed:");
            }
        }
        #endregion

        #region 身份证识别
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
                            CvrImg.Source = CvrImgMain.Source = item;
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
                
                if (FaceFun.AddSessions(info) > 0)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        MiniImg.Source = FaceFun.ByteToBitmapImage(PhotoOk);
                    });
                    base.InSession();
                }
                else
                {
                    "写入数据库失败".ToSaveLog("入场记录时：");
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("InSession");
            }

        }
        #endregion
        #endregion

        #region UI界面响应

        /// <summary> 入场校验是否允许，true通过，false未通过 </summary>
        /// <param name="idNo">身份证</param>
        /// <param name="isIdNo"></param>
        /// <returns>是否通过</returns>
        protected override bool CheckSession(string idNo, bool isIdNo = true)
        {
            var v = base.CheckSession(idNo, isIdNo);
            if (!v)
            {
                ClearValue();
                ShowEventMsg("该客户已经入场",MsgType.FaceErr);
            } 
            return v;
        }
        protected override void ShowEventMsg(string strMsg, MsgType type)
        {
            base.ShowEventMsg(strMsg,type);
            Dispatcher.InvokeAsync(() =>
            {
                switch (type)
                {
                    case MsgType.TipErr:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.Red;
                        break;
                    case MsgType.TipOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        break;
                    case MsgType.Info:
                        Tip2.Text = strMsg;
                        Tip2.Foreground = Brushes.GreenYellow;
                        break;
                    case MsgType.FaceOk:
                        Tip1.Text = strMsg;
                        Tip1.Foreground = Brushes.White;
                        Tip2.Text = "";
                        HideShow(1);
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
            Tip2.Text = "注意:请将身份证放置在识别区域.";
            CvrImg.Source = CvrImgMain.Source = MiniImg.Source = null;
            LeftImg.Visibility = Visibility.Hidden;
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
    }
}
