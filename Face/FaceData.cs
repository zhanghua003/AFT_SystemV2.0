using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using IrLibrary_Jun.PublicClass;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace AFT_System.Face
{

    public class FaceData : IDisposable
    {
        #region 私有成员
        /// <summary> 人脸检测句柄 </summary>
        private readonly IntPtr _ip;
        /// <summary> 人脸特征码句柄 </summary>
        private readonly IntPtr _ip1;

        public Bitmap FaceBitmapSource;
        /// <summary> 人脸数据句柄 </summary>
        private IntPtr _itData;

        private RrFaceT _rf = new RrFaceT();
        //private readonly string _fdModelPath = AppDomain.CurrentDomain.BaseDirectory + @"rr_facedetect.3.0.1\models";
        //private readonly string _fvModelPath = AppDomain.CurrentDomain.BaseDirectory + @"rr_faceverify.3.0.1\models";
        private readonly string _fdModelPath = AppDomain.CurrentDomain.BaseDirectory + @"rr_facedetect.3.1.2\models";
        private readonly string _fvModelPath = AppDomain.CurrentDomain.BaseDirectory + @"rr_facedetect.3.1.2\models";
        private int _w, _h;
        /// <summary> 人脸识别特征码 </summary>
        public RrFeatureT Rft;
        /// <summary> 人脸特征码提取结果 </summary>
        public IntPtr VerifyIr { get; set; }
        /// <summary> 人脸特征码提取结果 </summary>
        public IntPtr TzIr { get; set; }
        public RrFaceT Rrface { get; set; }
        private string _msg;
        public string Msg
        {
            get { return _msg; }
            set { _msg += (value + "\n"); }
        }

        private Mat invert;
        #endregion

        public FaceData()
        {
            _ip = FaceverifyDll.rr_fd_create_detector(_fdModelPath, (int)FaceverifyDll.rr_fd_get_version());
            _ip1 = FaceverifyDll.rr_fv_create_verifier(_fvModelPath, FaceverifyDll.rr_fv_get_version());
        }

        public FaceData(Bitmap bitmap) : this()
        {
            if (bitmap == null) return;
            FaceBitmapSource = FaceVerify(bitmap); //人脸检测
                                                   //FaceTz();   //提取特征码
        }
        /// <summary>人脸检测</summary>
        public Bitmap FaceVerify(Bitmap bmpImage)
        {
            try
            {
                using (var currentFrame = new Image<Bgr, Byte>(bmpImage))
                {
                    //只能这么转
                    invert = new Mat();
                    CvInvoke.BitwiseAnd(currentFrame, currentFrame, invert);
                    int c = 0;
                    _rf = new RrFaceT();
                    _w = invert.Width; _h = invert.Height;
                    _itData = invert.DataPointer;
                    var lp = new IntPtr();
                    IntPtr it = FaceverifyDll.rr_fd_detect(_ip, _itData, RrImageType.RR_IMAGE_BGR8UC3, _w, _h, ref lp, ref c);
                    if (it.ToInt32() != 0) return bmpImage;
                    if (c > 0)
                    {
                        _rf = (RrFaceT)Marshal.PtrToStructure(lp, typeof(RrFaceT));
                        Rrface = _rf;
                    }
                    FaceverifyDll.rr_fd_release_detect_result(lp);
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceData.FaceVerify:");
            }
            return bmpImage;
        }
        /// <summary>人脸检测</summary>
        public Bitmap FaceVerify(Mat mat)
        {
            if (mat == null) return null;
            try
            {
                int c = 0;
                _rf = new RrFaceT();
                _w = mat.Width; _h = mat.Height;
                _itData = mat.DataPointer;
                var lp = new IntPtr();
                "人脸检测句柄分析".ToSaveLog("FaceVerify:");
                IntPtr it = FaceverifyDll.rr_fd_detect(_ip, _itData, RrImageType.RR_IMAGE_BGR8UC3, _w, _h, ref lp, ref c);
                if (it.ToInt32() == 0)
                {
                    if (c > 0)
                    {
                        _rf = (RrFaceT)Marshal.PtrToStructure(lp, typeof(RrFaceT));
                        Rrface = _rf;

                        #region 给识别出的所有人脸画矩形框
                        //int with = _rf.rect.right - _rf.rect.left;
                        //int heigth = _rf.rect.bottom - _rf.rect.top;
                        //var face = new Rectangle(_rf.rect.left, _rf.rect.top, with, heigth);
                        //Bitmap bm = FaceFun.DrawRectangleInPicture(mat.Bitmap, face, Color.Red, 2);
                        //"释放人脸检测句柄".ToSaveLog("FaceVerify:");
                        //FaceverifyDll.rr_fd_release_detect_result(lp);
                        //return bm;
                        #endregion
                    }
                    else
                    {
                        return null;
                    }
                }
                "释放人脸检测句柄.".ToSaveLog("FaceVerify");
                FaceverifyDll.rr_fd_release_detect_result(lp);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceData.FaceVerify:");
            }
            return mat.Bitmap;
        }

        /// <summary>提取人像特征码 </summary>
        public RrFeatureT FaceTz()
        {
            try
            {
                var ft = new RrFeatureT();
                //var fp = new RrFacePrimaryT { rect = _rf.rect, landmarks = _rf.landmarks };
                //byte[] byFp = FaceFun.StructToBytes(fp);
                byte[] byFp = FaceFun.StructToBytes(_rf);
                VerifyIr = FaceverifyDll.rr_fv_extract_feature(_ip1, _itData, RrImageType.RR_IMAGE_BGR8UC3, _w, _h, byFp, ref ft);
                Rft = ft;
                return ft;
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceData.FaceTz:");
            }
            return Rft;
        }
        public RrFeatureT FaceTz(Bitmap bmpImage)
        {
            FaceBitmapSource = FaceVerify(bmpImage);
            return FaceTz();
        }
        public RrFeatureT FaceTz(Mat mat)
        {
            if (FaceBitmapSource != null)
            {
                FaceBitmapSource.Dispose();
                FaceBitmapSource = null;
            }
            FaceBitmapSource = FaceVerify(mat);
            return FaceTz();
        }
        ~FaceData()
        {
            Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    //执行基本的清理代码
                    FaceverifyDll.rr_fd_destroy_detector(_ip);  //销毁人脸检测句柄
                    FaceverifyDll.rr_fv_destroy_verifier(_ip1); //销毁人脸对比句柄
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("Dispose:");
            }

        }
    }

}
