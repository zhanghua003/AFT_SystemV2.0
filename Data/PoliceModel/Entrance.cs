using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Data.PoliceModel
{
    public class Entrance
    {
        public List<EntranceInfo> dataVoList { get; set; }
        public string verification { get; set; }
    }
    /// <summary>
    /// 验票信息
    /// </summary>
    public class EntranceInfo
    {
        /// <summary>
        /// 票对应的活动名称
        /// </summary>
        public string activityId { get; set; }
        /// <summary>
        /// 验票入口标识
        /// </summary>
        public string entranceCode { get; set; }
        /// <summary>
        /// 验票入口名字
        /// </summary>
        public string entranceName { get; set; }
        /// <summary>
        /// 通过票，验票进场人员数量
        /// </summary>
        public int ticketNumber { get; set; }
        /// <summary>
        /// 通过验证工作证数量进场数量
        /// </summary>
        public int certificationNumber { get; set; }
        /// <summary>
        /// 票的编号，不可重复
        /// </summary>
        public string ticketNo { get; set; }
        /// <summary>
        /// 入场人员身份证号码
        /// </summary>
        public string ticketHolderIdCardNo { get; set; }
        /// <summary>
        /// 入场人员姓名
        /// </summary>
        public string ticketHolderName { get; set; }
        /// <summary>
        /// 人像图片存储路径
        /// </summary>
        public string entranceFacePic { get; set; }
        /// <summary>
        /// 入口验票异常数量
        /// </summary>
        public int abnormalNumber { get; set; }
        /// <summary>
        /// 入场时间
        /// </summary>
        public DateTime entranceTime { get; set; }


    }
}
