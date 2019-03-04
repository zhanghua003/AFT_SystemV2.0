using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.Public;
using Emgu.CV;
using IrLibrary_Jun.PublicClass;
using Timer = System.Timers.Timer;

namespace AFT_System.CustomControl.ModeView
{
    public class BaseFaceView:BaseView
    {
        #region 公共成员变量


        protected RrFeatureT Rft2;    //图片特征码
        /// <summary> 二代身份证操作类 </summary>
        protected Cvr Cvr;
        protected bool CvrPass { get; set; }
        /// <summary> 人脸识别是否通过 </summary>
        protected bool FacePass { get; set; }
        /// <summary> 人脸比对结论：摄像头人脸识别置信度 </summary>
        protected float FaceIr { get; set; }
        /// <summary> 人脸识别取信值阀值 </summary>
        protected readonly float _confidence;
        protected Thread CamThread;

        /// <summary> 人脸识别对象类 </summary>
        protected FaceData Face;
        #endregion

        public BaseFaceView()
        {
            _confidence = IrAdvanced.ReadInt("Confidence", 65) / 100.0f;   //可信度
        }

        protected override void OpenCamera()
        {
           base.OpenCamera();
           FacePass = CvrPass = false; //初始值
           if (MyCapture==null) return;
           //摄像头刷新线程
           //CamThread = new Thread(Capture_ImageGrabbed) { IsBackground = true };
           //CamThread.Start();
        }


        public override void Init()
        {
            //初始化人脸识别模块
            Face = new FaceData();
            base.Init();
            //二代身份证验证线程
            OpenCvr();
        }

        #region 摄像头刷新
        protected override void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                MyMat = null;
                MyMat = new Mat();
                if(MyCapture==null) return;
                if (Cvr == null) { "身份证硬件初始化失败".ToSaveLog("FaceIdView.Cature_ImageGrabbed:"); return; }
                MyCapture.Retrieve(MyMat);
                if (CvrPass && !FacePass)
                {
                    FaceOut++;
                    if (FaceOut >= FaceOutCount)
                    {
                        Cvr.Info.Number.ToSaveLog("比对超时,比对失败:");
                        PhotoErr = FaceFun.BitmapToByte(MyMat.Bitmap);
                        
                        CvrPass = false;
                        ShowEventMsg("比对超时：当前用户与身份证对比失败", MsgType.Info);
                        return;
                    }
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
                                Cvr.Info.Number.ToSaveLog("保存检票成功照片:");
                              //  MyMat.Bitmap.Save(string.Format("{0}{1}.jpg", CamPath, Cvr.Info.Number),ImageFormat.Jpeg);
                                PhotoOk = FaceFun.BitmapToByte(MyMat.Bitmap);
                                ShowEventMsg("比对成功", MsgType.TipOk);
                                FaceIr.ToString().ToSaveLog(Cvr.Info.Number + " 比对成功，相似度：");
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
            catch (Exception ex)
            {
                ex.ToSaveLog("Capture_ImageGrabbed:");
            }
        }





        #region 线程刷新
        private void Capture_ImageGrabbed()
        {
            while (true)
            {
                try
                {
                    GetCamBitmap();
                    Thread.Sleep(CamSleep);
                }
                catch (Exception ex)
                {
                    ex.ToSaveLog("Capture_ImageGrabbed:");
                }

            }
        }
        /// <summary> 获取实时摄像头视频流 </summary>
        protected virtual Bitmap GetCamBitmap()
        {
            try
            {
                lock (Obj)
                {
                    var mat = MyCapture.QueryFrame();
                    if (CvrPass && !FacePass)
                    {
                        FaceOut++;
                        if (FaceOut >= FaceOutCount)
                        {
                            FaceOut = 0;
                            //var errPhoto = string.Format("{0}CamPhoto\\Err_{1}.jpg", FaceFun.BaseDir, Cvr.Info.Number);
                            //mat.Bitmap.Save(errPhoto, ImageFormat.Jpeg);
                            //Thread.Sleep(100);
                            return mat.Bitmap;
                        }
                        ShowEventMsg("识别中" + (FaceOutCount - FaceOut), MsgType.TipErr);
                        CameraRft = Face.FaceTz(mat);
                        if (Face.VerifyIr.ToInt32() == 0)
                        {
                            FaceIr = FaceFun.FaceResult(CameraRft, Rft2);
                            if (FaceIr > _confidence)
                            {
                                try
                                {
                                    //保存识别到的摄像头图片
                                 //   mat.Bitmap.Save(string.Format("{0}CamPhoto\\{1}.jpg", FaceFun.BaseDir, Cvr.Info.Number),ImageFormat.Jpeg);
                                    ShowEventMsg("比对成功", MsgType.TipOk);
                                    FaceIr.ToString().ToSaveLog(Cvr.Info.Number + " 比对成功，相似度：");
                                    FacePass = true; //当取信值大于80%认为是同一个人比对成功
                                }
                                catch (Exception ex)
                                {
                                    ex.ToSaveLog("OnComplete：");
                                }

                            }
                        }
                        return Face.FaceBitmapSource;
                    }
                    return mat.Bitmap;  
                }
              
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("Capture_ImageGrabbed:");
            }
            return null;
        }
        #endregion

        #endregion

        #region 二代身份证识别
        private void OpenCvr()
        {
            "初始化二代身份证硬件连接。".ToSaveLog("OpenCvr:");
            try
            {
                Cvr = new Cvr();
                Cvr.GetNewDataEvent += Cvr_GetNewDataEvent;
            }
            catch (Exception ex)
            {
                ex.ToSaveLog();
            }
        }
        //二代身份证识别触发
        protected virtual void Cvr_GetNewDataEvent(object sender, CvrInfo e)
        {
            if(e.Number.IsNullOrEmpty()) return;
            //当识别到新身份证，清空之前认证结果
            e.Number.ToSaveLog("新的身份证信息被发现:");
           ClearValue();
            try
            {
                if (CheckRule(e.Number))
                {
                    //当识别到新身份证，清空之前认证结果
                    var item = IrAdvanced.GetBitmapFromFile(e.PeopleImg);
                    if (item != null)
                    {
                        FaceFun.TimeStart();
                        Rft2 = Face.FaceTz(item);
                        if (Face.TzIr.ToInt32() == 0)
                        {
                            CvrPass = true;
                        }
                        else
                        {
                            "身份证照片人脸检测失败".ToSaveLog("Cvr_GetNewDataEvent");
                        }
                        FaceFun.TimeStop("读取身份证人脸数据耗时:");
                    }
                }

            }
            catch (Exception ex)
            {
              ex.ToSaveLog("BaseFaceView:");
            }

        }

        #endregion
        #region 测试硬件连接
        protected override void TestHardConn()
        {
            base.TestHardConn();
            #region 2019年3月屏蔽检查模块
            //测试身份证模块
            //OnHardConn(string.Format("身份证连接\t{0}\n", TestCvr()? "\t\t成功 √" : "失败 ×"));
            #endregion
        }

        private bool TestCvr()
        {
            try
            {
                var port = IrAdvanced.ReadInt("CvrCom");
                var r= Cvrsdk.Well_InitComm(port) == 1;    //返回是否初始化成功
                if(r) Cvrsdk.Well_CloseComm();
                return r;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion
        #region 释放资源
        protected override void ClearValue()
        {
            base.ClearValue();
            CvrPass = FacePass = false;
          
        }
        public override void Dispose()
        {
           base.Dispose();
            if (CamThread != null)
            {
                CamThread.Abort();
                CamThread = null;
            }
            if (Cvr != null)
            {
                Cvr.Dispose();
                Cvr = null;
            }
            if (Face != null)
            {
                Face.Dispose();
            }
        }
        #endregion
    }
}
