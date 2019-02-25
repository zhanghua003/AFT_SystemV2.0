using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.ICard
{
  //ERRORCARD_CONNECT_DEVICE=1;        //连接设备错误
  //ERRORCARD_VERIFY_PASSWORD=2;       //校验卡密码错误
  //ERRORCARD_INVAILD_CARD=3;          //无效卡
  //ERRORCARD_READ_IDCARD=4;           //读卡错误
  //ERRORCARD_EMPTY_CARD =5;           //空白卡
  //ERRORCARD_VERIFY_CARD=6;           //校验卡错误
  //ERRORCARD_CANFIND_PASSFILE =7;     //未找到密码文件
  //ERRORCARD_WRITE_IDCARD = 8 	     //写卡错误	 

    public class IdCardFunc
    {
        #region TicketCardInfo结构体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct TicketCardInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string IDCard;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
            public string Name;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string CardNo;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
            public string ActiveDate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string StadiumArea;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string Row;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string Position;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
            public string BuyYear;

            public UInt16 CardType;
            public UInt16 WatchTimes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string Reserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
            public string Reserved2;
        }
        #endregion

        #region 外部DLL引用
        /// <summary> 读卡 </summary>
        [DllImport("D7Reader.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int GetCardData_New(IntPtr inptr);
        /// <summary> 蜂鸣器 </summary>
        [DllImport("D7.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int TestDevice([MarshalAs(UnmanagedType.LPStr)]string device);
        /// <summary> 写卡 </summary>
        [DllImport("D7Reader.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int WriteData_Ext([MarshalAs(UnmanagedType.LPStr)]string ext1, [MarshalAs(UnmanagedType.LPStr)]string ext2);        
        #endregion

        /// <summary> IC卡年卡读卡器端口 </summary>
        public static string Port { get; set; }
        /// <summary> 蜂鸣器端口 </summary>
        public static string BeepPort { get; set; }
        /// <summary> 读卡操作 </summary>
        public static int GetTicketData_New(ref TicketCardInfo cardInfo)
        {
            try
            {
                int len = Marshal.SizeOf(cardInfo);//计算对象大小  
                IntPtr ptr = Marshal.AllocHGlobal(len);//从非托管内存中分配内存  
                Marshal.StructureToPtr(cardInfo, ptr, true);//将数据从托管对象封送到非托管内存块  
                int res = GetCardData_New(ptr);//调用声明的方法  
                //将数据从非托管内存块封送到新分配的指定类型的托管对象  
                cardInfo = (TicketCardInfo)Marshal.PtrToStructure(ptr, typeof(TicketCardInfo));
                Marshal.FreeHGlobal(ptr);//释放从非托管内存中分配的内存 
                return res;
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("GetTicketData_New");
            }
            return 8;
        }

        /// <summary> 写卡操作 </summary>
        public static int SetTicketCardData_Ext(string ext1, string ext2)
        {
            try
            {
                return WriteData_Ext(ext1, ext2);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("SetTicketCardData_Ext:");
            }
            return 8;
        }

        public static void Beep()
        {
            BeepPort.ToSaveLog("调用蜂鸣器方法:");
            try
            {
                if (!Port.IsNullOrEmpty()) TestDevice(BeepPort); 
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("(闸机)蜂鸣器错误:");
            }


        }
    }
}
