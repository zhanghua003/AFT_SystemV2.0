using System;
using System.Runtime.InteropServices;

namespace AFT_System.Face
{
    #region 结构体
     
   [StructLayout(LayoutKind.Sequential)]
    public struct RrFeatureT
    {
        public int len;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public float[] data;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RrRectT
    {
        [FieldOffset(0)]
        public Int32 left;	//< 矩形左边的坐标
        [FieldOffset(4)]
        public Int32 top;	//< 矩形顶边的坐标
        [FieldOffset(8)]
        public Int32 right;	//< 矩形右边的坐标
        [FieldOffset(12)]
        public Int32 bottom;	//< 矩形底边的坐标
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct RrPointfT
    {
        [FieldOffset(0)]
        public Single x;//< 点的x坐标，float类型
        [FieldOffset(4)]
        public Single y;//< 点的y坐标，float类型
    }

    /// <summary>
    /// 人脸信息结构体
    /// </summary> 
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct RrFaceT
    {
        [FieldOffset(0)]
        public RrRectT rect;				//< 人脸的矩形区域，在图片中的位置。
        [FieldOffset(16)]
        public Single confidence;
        [FieldOffset(20)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]//< 本矩形区域是人脸的置信度。
        public RrPointfT[] landmarks;	//< 人脸5关键点的数组。依次为 左眼中心，右眼中心，鼻尖，左嘴角，右嘴角。
        [FieldOffset(60)]
        public int yaw;			//< 水平转角的度数。正值代表脸向右看。
        [FieldOffset(64)]
        public int pitch;			//< 俯仰角的度数。正值代表脸向上看。
        [FieldOffset(68)]
        public int roll;			//< 旋转角的度数。正值代表脸向右肩倾斜。
        [FieldOffset(72)]
        public int id;				//< 不同帧中的同一个人的id
        [FieldOffset(76)]
        public Single quality;		//< 人脸质量。本版本不提供。                
    }

    /// <summary> 人脸的矩形区域及数据 </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RrFacePrimaryT
    {
        /// <summary> 人脸的矩形区域，在图片中的位置。 </summary>
        public RrRectT rect;
        /// <summary> 人脸5关键点的数组。依次为 左眼中心，右眼中心，鼻尖，左嘴角，右嘴角。 </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public RrPointfT[] landmarks;
    }
    #endregion

    public class FaceverifyDll
    {
        #region 人脸检测rr_facedetect_3_1_2.dll
        /// <summary> 创建人脸检测句柄 </summary>
        /// <param name="model_path">模型文件夹的绝对路径或相对路径</param>
        /// <param name="sdk_version">程序编译时使用的sdk版本，必须传入RR_FD_VERSION</param>
        /// <returns>成功返回人脸检测句柄，失败返回NULL</returns>
        [DllImport("rr_facedetect_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rr_fd_create_detector(string model_path, int sdk_version);
        /// <summary> 销毁人脸检测句柄 </summary>
        /// <param name="detector_handle">人脸检测句柄</param>
        [DllImport("rr_facedetect_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rr_fd_destroy_detector(IntPtr detector_handle);

        /// <summary> 人脸检测 </summary>
        /// <param name="detector_handle">人脸检测句柄</param>
        /// <param name="image_data">用于检测的图像数据</param>
        /// <param name="image_type">用于检测的图像数据的格式</param>
        /// <param name="image_width">用于检测的图像的宽度(以像素为单位)</param>
        /// <param name="image_height">用于检测的图像的高度(以像素为单位)</param>
        /// <param name="p_faces_array">检测到的人脸信息数组。api内部分配内存，需要调用rr_fd_release_detect_result函数释放。</param>
        /// <param name="p_faces_count">检测到的人脸数量。api内部分配内存，需要调用rr_fd_release_detect_result函数释放。</param>
        /// <returns>成功返回RR_OK，否则返回错误代码</returns>
        [DllImport("rr_facedetect_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr rr_fd_detect(IntPtr detector_handle, [MarshalAs(UnmanagedType.LPArray)]byte[] image_data, rr_image_type image_type, int image_width, int image_height, ref rr_face_t p_faces_array, ref int p_faces_count);
        public static extern IntPtr rr_fd_detect(IntPtr detector_handle, IntPtr image_data, RrImageType image_type, int image_width, int image_height, ref IntPtr p_faces_array, ref int p_faces_count);
        /// <summary>释放人脸检测返回结果时分配的内存</summary>
        /// <param name="faces_array">检测到的人脸信息数组</param>
        [DllImport("rr_facedetect_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rr_fd_release_detect_result(IntPtr faces_array);
        /// <summary> 获得当前使用SDK的版本</summary>
        /// <returns>返回当前使用SDK的版本信息。可以对返回结果使用宏RR_GET_MAJOR_VERSION()，RR_GET_MINOR_VERSION()，RR_GET_PATCH_VERSION()获得更具体的版本信息</returns>
        [DllImport("rr_facedetect_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rr_fd_get_version();
        #endregion

        #region 人像比对rr_faceverify_3_1_2.dll
        /// <summary> 创建人脸比对句柄 </summary>
        /// <param name="model_path">模型文件夹的绝对路径或相对路径</param>
        /// <param name="sdk_version">程序编译时使用的sdk版本，必须传入RR_FV_VERSION</param>
        /// <returns>成功返回人脸比对句柄，失败返回NULL</returns>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rr_fv_create_verifier(string model_path, int sdk_version);
        /// <summary> 销毁人脸比对句柄 </summary>
        /// <param name="verifier_handle">人脸比对句柄</param>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void rr_fv_destroy_verifier(IntPtr verifier_handle);

        /// <summary> 提取人脸特征 </summary>
        /// <param name="verifier_handle">人脸比对句柄</param>
        /// <param name="image_data">用于提取特征的图像数据</param>
        /// <param name="image_type">用于提取特征的图像数据的格式</param>
        /// <param name="image_width">用于提取特征的图像的宽度(以像素为单位)</param>
        /// <param name="image_height">用于提取特征的图像的高度(以像素为单位)</param>
        /// <param name="face">人脸的landmarks及矩形区域。相关信息可以通过人脸检测得到</param>
        /// <param name="p_feature">人脸特征</param>
        /// <returns>成功返回RR_OK，否则返回错误代码</returns>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rr_fv_extract_feature(IntPtr verifier_handle, IntPtr image_data, RrImageType image_type, int image_width, int image_height, byte[] face, ref RrFeatureT p_feature);

        /// <summary> 比较两个人脸特征 </summary>
        /// <param name="feature1">第一张人脸的特征信息</param>
        /// <param name="feature2">第二张人脸的特征信息</param>
        /// <returns>返回两个人脸特征的相似程度</returns>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Single rr_fv_compare_features(byte[] feature1, byte[] feature2);
        /// <summary> 获得特征的长度 </summary>
        /// <param name="verifier_handle">人脸比对句柄</param>
        /// <returns>返回特征的长度</returns>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rr_fv_get_feature_length(IntPtr verifier_handle);
        /// <summary> 获得当前使用SDK的版本 </summary>
        /// <returns>返回当前使用SDK的版本信息。可以对返回结果使用宏RR_GET_MAJOR_VERSION()，RR_GET_MINOR_VERSION()，RR_GET_PATCH_VERSION()获得更具体的版本信息</returns>
        [DllImport("rr_faceverify_3_1_2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int rr_fv_get_version();

        #endregion
    }

    public enum RrImageType
    {
        /// <summary>
        /// BGR  8:8:8   24bpp ( 3通道24bit BGR 像素 )
        /// </summary>
        RR_IMAGE_BGR8UC3 = 0,
        /// <summary>
        /// BGRA 8:8:8:8 32bpp ( 4通道32bit BGRA 像素 )
        /// </summary>
        RR_IMAGE_BGRA8UC4 = 1,
        /// <summary>
        /// Y18bpp ( 单通道8bit灰度像素 )
        /// </summary>
        RR_IMAGE_GRAY8UC1 = 2,
        RR_IMAGE_MAX
    }
}
