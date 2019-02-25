using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AFT_System.CVR
{
    #region class身份证阅读类
    /// <summary>身份证阅读类</summary>
   public class Cvrsdk
    {
        #region 研腾二代证接口
        //[DllImport("termb.dll", EntryPoint = "YT_InitComm")]
        //public static extern int YT_InitComm(int Port);//声明外部的标准动态库, 跟Win32API是一样的
        //[DllImport("termb.dll", EntryPoint = "YT_Authenticate")]
        //public static extern int YT_Authenticate();
        //[DllImport("termb.dll")]
        //public static extern int YT_Read_Content(int Active);
        //[DllImport("termb.dll", EntryPoint = "YT_Read_FPContent", CharSet = CharSet.Auto, SetLastError = false)]
        //public static extern int YT_Read_FPContent();
        //[DllImport("termb.dll", EntryPoint = "YT_CloseComm", CharSet = CharSet.Auto, SetLastError = false)]
        //public static extern int YT_CloseComm();
        //[DllImport("termb.dll", EntryPoint = "YT_GetState", CharSet = CharSet.Auto, SetLastError = false)]
        //public static extern int YT_GetState();
        //[DllImport("termb.dll", EntryPoint = "GetPeopleName", CharSet = CharSet.Ansi, SetLastError = false)]
        //public static extern int GetPeopleName(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetPeopleNation", CharSet = CharSet.Ansi, SetLastError = false)]
        //public static extern int GetPeopleNation(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetPeopleBirthday", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetPeopleBirthday(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetPeopleAddress", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetPeopleAddress(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetPeopleIDCode", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetPeopleIDCode(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetDepartment", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetDepartment(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetStartDate", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetStartDate(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetEndDate", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetEndDate(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetPeopleSex", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetPeopleSex(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "GetFPDate", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetFPDate(ref byte strTmp, ref int strLen);
        //[DllImport("termb.dll", EntryPoint = "YT_GetSAMID", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int YT_GetSAMID(ref byte strTmp);
        //[DllImport("termb.dll", EntryPoint = "GetManuID", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int GetManuID(ref byte strTmp);
        //[DllImport("termb.dll", EntryPoint = "YT_GetIDCardUID", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.StdCall)]
        //public static extern int YT_GetIDCardUID(StringBuilder puidbuff, int nbufflen);
        #endregion

        #region 威尔二代证接口
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_InitComm(int Port);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_Authenticate();
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_ReadContent();
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_CloseComm();
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetName(ref byte strName, ref int len);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetSex(ref byte SexStr, ref int SexLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetNation(ref byte NationStr, ref int NationLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetBirthday(ref byte BirthdayStr, ref int BirthdayLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetAddress(ref byte AddressStr, ref int AddressLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetIdCard(ref byte IdCardStr, ref int IdCardLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetDepart(ref byte DepartStr, ref int DepartLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetBeginDate(ref byte BeginDateStr, ref int BeginDateLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetEndDate(ref byte EndDateStr, ref int EndDateLen);
        [DllImport("WellIdCard.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int Well_GetSAMIDToStr(ref byte SamIdStr);
        #endregion
    }
    #endregion
    #region struct结构体
    //扫描结构：
    public struct CvrInfo
    {
        /// <summary> 姓名 </summary>
        public string Name { get; set; }
        /// <summary> 地址，在识别护照时导出的是国籍简码 </summary>
        public string Address { get; set; }
        /// <summary> 身份证号 </summary>
        public string Number { get; set; }
        /// <summary> 身份证照片 </summary>
        public string PeopleImg { get; set; }
        /* 暂不使用
         *         /// <summary> 性别 </summary>
        public string Sex { get; set; }
        /// <summary> 民族，护照识别时此项为空 </summary>
        public string People { get; set; }
        /// <summary> 出生日期 </summary>
        public string Birthday { get; set; }
        /// <summary> 签发日期，在识别护照时导出的是有效期至 </summary>
        public string Signdate { get; set; }
        /// <summary> 有效起始日期，在识别护照时为空 </summary>
        public string ValidtermOfStart { get; set; }
        /// <summary> 有效截止日期，在识别护照时为空 </summary>
        public string ValidtermOfEnd { get; set; }
        /// <summary> 安全模块号 </summary>
        public string Samid { get; set; }
         * */
    }
    #endregion

}
