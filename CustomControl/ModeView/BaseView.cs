using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AFT_System.CustomControl.Mode;
using AFT_System.Face;
using AFT_System.ICard;
using AFT_System.Public;
using Emgu.CV;
using Emgu.CV.CvEnum;
using IrControlLibrary_Jun.Controls.Body.Book;
using IrLibrary_Jun.PublicClass;
using Brush = System.Drawing.Brush;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Timers.Timer;

namespace AFT_System.CustomControl.ModeView
{
    public class BaseView : Grid, IDisposable
    {
        /// <summary> 是否启用白名单校验 </summary>
        public bool UseWriteList = false;
        /// <summary> 是否启用入场校验你 </summary>
        public bool UseInSession = false;
        /// <summary> 散票的身份证号码是否验证必须与所识别身份证号码一致 </summary>
        public bool IsIdSame = true;
        /// <summary> 是否校验身份证号码存在与否 </summary>
        public bool IsCheckIdNo = false;
        /// <summary> 是否校验黑名单用户 </summary>
        public bool IsCheckBlack = false;
        /// <summary> 是否语音播报 </summary>
        public bool IsAudio = false;
        /// <summary> 摄像头画面刷新速率 </summary>
        public int CamSleep { get; set; }
        protected int FaceTimeOut = 3000;
        /// <summary> 超时总次数 </summary>
        protected int FaceOutCount = 300;
        /// <summary> 人脸识别超时计数 </summary>
        protected int FaceOut = 0;
        /// <summary> 检票成功拍照结果 </summary>
        protected byte[] PhotoOk;
        /// <summary> 检票失败拍照结果 </summary>
        protected byte[] PhotoErr;
        /// <summary> 白名单信息 </summary>
       // public WhiteNameInfo? WhiteInfo { get; set; }

        /// <summary> 场次信息 </summary>
        public virtual MatchInfo MyMatch { get; set; }
        /// <summary> 摄像头对象 </summary>
        protected Capture MyCapture;   //摄像头
        protected readonly object Obj = new object();
        /// <summary> 机台登陆者 </summary>
        public string AftUserName { get; set; }
        /// <summary> 摄像机视频流 </summary>
        protected Mat MyMat;
        /// <summary> 摄像头取得的人脸识别特征码 </summary>
        protected RrFeatureT CameraRft;    //摄像头特征码
        private readonly Timer _time = new Timer() { AutoReset = false };

        #region 硬件连接事件
        /// <summary> 硬件准备连接 </summary>
        public event EventHandler<string> HardConn;
        protected virtual void OnHardConn(string e)
        {
            EventHandler<string> handler = HardConn;
            if (handler != null) handler(this, e);
        }
        /// <summary> 硬件连接检测完成 </summary>
        public event EventHandler HardCompleted;
        protected virtual void OnHardCompleted()
        {
            OnHardConn("==================检测结束");
            EventHandler handler = HardCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        public BaseView()
        {
            Width = 1280; Height = 800;
            IdCardFunc.Port = IrAdvanced.ReadString("Port", "", "", "Communication1").ToLower();
            IdCardFunc.BeepPort = IrAdvanced.ReadString("BeepPort", "", "", "Communication1").ToLower();
            UseWriteList = IrAdvanced.ReadBoolean("Whitelist"); //是否启用白名单验证
            UseInSession = IrAdvanced.ReadBoolean("InSession"); //是否启用入场验证避免重复入场
            IsIdSame = IrAdvanced.ReadBoolean("IsIdSame");      //散票的身份证号码是否验证必须与所识别身份证号码一致
            IsCheckIdNo = IrAdvanced.ReadBoolean("IsCheckIdNo");
            IsCheckBlack = IrAdvanced.ReadBoolean("IsCheckBlack");
            IsAudio = IrAdvanced.ReadBoolean("IsAudio");
            CamSleep = IrAdvanced.ReadInt("CamSleep", 100);      //摄像头刷新速率
            FaceTimeOut = IrAdvanced.ReadInt("FaceTimeOut", 3000);  //人脸识别超时时间
            FaceOutCount = FaceTimeOut / CamSleep;
        }

        #region 初始化硬件
        public virtual void Init()
        {

            //摄像头识别线程
            "初始化摄像头硬件连接。".ToSaveLog("OpenCamera:");
            var cameraThread = new Thread(OpenCamera) { IsBackground = true };
            cameraThread.Start();
            //  OpenCamera();
        }
        #region 摄像头视频流捕捉
        /// <summary> 打开摄像头，0为第一个找到的摄像头 </summary>
        protected virtual void OpenCamera()
        {
            try
            {

                CvInvoke.UseOpenCL = false; //不使用OpneCL  
                MyCapture = new Capture(0); //初始化摄像头
                                            // MyCapture.SetCaptureProperty(CapProp.Fps, 20);
                MyCapture.SetCaptureProperty(CapProp.FrameWidth, 352);
                MyCapture.SetCaptureProperty(CapProp.FrameHeight, 288);
                MyCapture.ImageGrabbed += Capture_ImageGrabbed;   //获取帧     
                MyCapture.Start(); //开启摄像头 

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("OpenCamera:");
                ShowEventMsg("摄像头硬件初始化失败", MsgType.TipErr);
                MyCapture.Dispose();
                MyCapture = null;
            }
        }

        protected virtual void Capture_ImageGrabbed(object sender, EventArgs e)
        {

        }
        #endregion
        #endregion

        #region 入场校验与白名单校验
        public bool CheckRule(string idNo)
        {
            var white = CheckWhite(idNo);
            var session = CheckSession(idNo);
            return white && session;
        }
        /// <summary> 白名单校验是否通过，true通过，false未通过 </summary>
        /// <param name="idNo">身份证</param>
        /// <returns>是否通过</returns>
        protected virtual bool CheckWhite(string idNo)
        {
            //检查白名单验证
            if (UseWriteList)
            {
                if (!FaceFun.IsInWhiteList(idNo))
                {
                    ClearValue();
                    ShowEventMsg("错误:白名单验证失败！" + idNo, MsgType.Info); return false;
                }
            }
            return true;
        }

        /// <summary> 入场校验是否允许，true通过，false未通过 </summary>
        /// <param name="idNo">身份证</param>
        /// <param name="isIdNo">是是身份证号,否是票号</param>
        /// <returns>是否通过</returns>
        protected virtual bool CheckSession(string idNo, bool isIdNo = true)
        {
            //检查是否已经入场
            if (UseInSession)
            {
                idNo.ToSaveLog("是否已经入场校验:");
                if (FaceFun.IsInSessions(idNo, isIdNo))
                {
                    ClearValue();
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 界面字符显示
        /// <summary> 清除值至初始状态 </summary>
        protected virtual void ClearValue()
        {
            FaceOut = 0;
        }
        //显示通知
        protected virtual void ShowEventMsg(string strMsg, MsgType type)
        {
            switch (type)
            {
                case MsgType.FaceOk:
                    ClearValue();
                    if (IsAudio) Dispatcher.InvokeAsync(() => FaceFun.PlayMp3("sound\\检票成功.mp3"));
                    IdCardFunc.Beep();
                    break;
                case MsgType.FaceErr:
                    HideShow(2);
                    ClearValue();
                    if (IsAudio) Dispatcher.InvokeAsync(() => FaceFun.PlayMp3("sound\\检票失败.mp3"));
                    break;
            }
        }

        protected virtual void HideShow(double tt)
        {
            _time.Stop();
            _time.Interval = tt * 1000;
            _time.Elapsed += Time_Elapsed;
            _time.Start();
        }

        protected virtual void Time_Elapsed(object sender, ElapsedEventArgs e)
        {

        }
        /// <summary> 写入本地记录并通知检票成功 </summary>
        protected virtual void InSession()
        {
            ShowEventMsg("检票成功", MsgType.FaceOk);
        }
        #endregion

        #region 测试硬件连接
        /// <summary> 测试硬件连接 </summary>
        public void TestHard()
        {
            var t = new Thread(TestHardConn) { IsBackground = true };
            t.Start();
        }

        protected virtual void TestHardConn()
        {
            #region 2019年3月屏蔽检查模块
            ////测试闸机连接
            //OnHardConn(string.Format("闸  机连接\t{0}\n", IdCardFunc.TestDevice(IdCardFunc.BeepPort) == 0 ? "\t\t成功 √" : "失败 ×"));
            ////测试摄像头
            //OnHardConn(string.Format("摄像头连接\t{0}\n", TestCapture() ? "\t\t成功 √" : "失败 ×"));
            #endregion

            //测试本地数据库
            OnHardConn(string.Format("数据库连接\t{0}\n", FaceFun.TestMatch() > 0 ? "\t\t成功 √" : "失败 ×"));
        }
        /// <summary> 测试摄像头连接状态 </summary>
        protected bool TestCapture()
        {
            try
            {
                CvInvoke.UseOpenCL = false; //不使用OpneCL  
                MyCapture = new Capture(0); //初始化摄像头
                MyCapture.SetCaptureProperty(CapProp.FrameWidth, 352);
                MyCapture.SetCaptureProperty(CapProp.FrameHeight, 288);
                MyCapture.Start(); //开启摄像头 
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("OpenCamera:");
                ShowEventMsg("摄像头硬件初始化失败", MsgType.TipErr);
                MyCapture.Dispose();
                MyCapture = null;
                return false;
            }
            MyCapture.Dispose();
            MyCapture = null;
            return true;
        }
        /// <summary> 条码扫描枪 </summary>
        protected bool TestQr()
        {
            try
            {
                var spProjector = new SerialPort(IrAdvanced.ReadString("BarCodePort"))
                {
                    BaudRate = 115200, //波特率
                    DataBits = 8,//数据位
                    StopBits = StopBits.One,  //停止位
                    Parity = Parity.None  //校检位
                };
                spProjector.Open();
                var r = spProjector.IsOpen;
                spProjector.Close();
                spProjector.Dispose();
                return r;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region 释放资源
        public virtual void Dispose()
        {
            if (MyCapture != null)
            {
                MyCapture.Stop();
                MyCapture.Dispose();
                MyCapture = null;
            }
        }
        #endregion

    }
}
