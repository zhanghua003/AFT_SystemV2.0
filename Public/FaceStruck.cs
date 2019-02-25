using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Public
{
    public enum MsgType {  Info,InfoErr,TipErr,TipOk, FaceOk, FaceErr }

    public class MsgArgs : EventArgs
    {
        public string Msg;
        public MsgType Type;
    }
    /// <summary> 入场流水记录结构体 </summary>
    public struct SessionsInfo
    {
        /// <summary> 场次ID </summary>
        public int SessionId { get; set; }
        /// <summary> 创建日期 </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary> 场次名称 </summary>
        public string Name { get; set; }
        /// <summary> 购买证件号码 </summary>
        public string IdNo { get; set; }
        /// <summary> 身份证照片 </summary>
        public byte[] IdCardPhoto { get; set; }
        /// <summary> 入场拍照照片 </summary>
        public byte[] TakePhoto { get; set; }
        /// <summary> 人脸识别数据 </summary>
        public byte[] FaceData { get; set; }
        /// <summary> 身份证地址 </summary>
        public string IdAddress { get; set; }
        /// <summary> 票务类型:0为散票，1为年票 </summary>
        public int TicketType { get; set; }
        /// <summary> 散票号码 </summary>
        public string TicketNo { get; set; }
        /// <summary> 区 </summary>
        public string Area { get; set; }
        /// <summary> 排 </summary>
        public string Row { get; set; }
        /// <summary> 座 </summary>
        public string Seat { get; set; }
        /// <summary> 检票机号 </summary>
        public string TelNo { get; set; }
        /// <summary> 检票区域 </summary>
        public string TelArea { get; set; }
        /// <summary> 购票人姓名 </summary>
        public string BuyName { get; set; }
        /// <summary> 购票人照片 </summary>
        public byte[] BuyPhoto { get; set; }
        /// <summary> 购买日期 </summary>
        public DateTime? BuyDate { get; set; }
        /// <summary> 验证方式 </summary>
        public int ValidateType { get; set; }
        /// <summary> 同步时间 </summary>
        public DateTime? SyncTime { get; set; }
        /// <summary> 状态 </summary>
        public int Status { get; set; }
        /// <summary> 备注 </summary>
        public string Remark { get; set; }
        public string UserName { get; set; }

    }
    /// <summary> 场次记录结构体 </summary>
    public struct MatchInfo
    {
        /// <summary> 场次ID </summary>
        public int SessionId { get; set; }
        /// <summary> 创建日期 </summary>
        public DateTime CreateDate { get; set; }
        /// <summary> 场次名称 </summary>
        public string SessionName { get; set; }
        /// <summary> 场次时间 </summary>
        public DateTime SessionDate { get; set; }
        /// <summary> 开始时间 </summary>
        public DateTime DateStart { get; set; }
        /// <summary> 结束时间 </summary>
        public DateTime DateEnd { get; set; }
        /// <summary> 0为人证对比，1为套票人证对比，2为散票人证对比 </summary>
        public int CheckRule { get; set; }
        /// <summary> 0为未开始，1为开始，2为结束 </summary>
        public int Status { get; set; }
        /// <summary> 备注 </summary>
        public string Remark { get; set; }
        /// <summary> 场次秘钥 </summary>
        public string Key { get; set; }
    }
    /// <summary> 白名单记录结构体 </summary>
    public struct WhiteNameInfo
    {
        /// <summary> 场次ID </summary>
        public int SessionId { get; set; }
        /// <summary> 创建日期 </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary> 购买姓名 </summary>
        public string BuyName { get; set; }
        /// <summary> 购买证件号码 </summary>
        public string IdNo { get; set; }
        /// <summary> 身价证照片 </summary>
        public byte[] IdCardPhoto { get; set; }
        /// <summary> 年票照片 </summary>
        public byte[] YearTicketPhoto { get; set; }
        /// <summary> 地址 </summary>
        public string Address { get; set; }
        /// <summary> 票务类型:0为散票，1为年票 </summary>
        public int TicketType { get; set; }
        /// <summary> 散票号码 </summary>
        public string TicketNo { get; set; }
        /// <summary> 区 </summary>
        public string Area { get; set; }
        /// <summary> 排 </summary>
        public string Row { get; set; }
        /// <summary> 座 </summary>
        public string Seat { get; set; }
        /// <summary> 机号 </summary>
        public int TelNo { get; set; }
        /// <summary> 区域 </summary>
        public string TelArea { get; set; }
        /// <summary> 状态:0为未用,1为启用,2为停用 </summary>
        public int Status { get; set; }
        /// <summary> 备注 </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 特征码
        /// </summary>
        public FaceVerifyLib.rr_feature_t rrfeature { get; set; }
        /// <summary>
        /// 照片路径
        /// </summary>
        public string IdCardPhotoPath { get; set; }
    }

    public struct InSession
    {
        
    }
    /// <summary> 散票二维码信息 </summary>
    public struct TicketInfo
    {
        /// <summary> 散票票号 </summary>
        public string TicketNo;
        /// <summary> 身份证号码 </summary>
        public string IdNo;
        /// <summary> 区域 </summary>
        public string Area;
        /// <summary> 排 </summary>
        public string Row;
        /// <summary> 座位 </summary>
        public string Seat;
    }
}
