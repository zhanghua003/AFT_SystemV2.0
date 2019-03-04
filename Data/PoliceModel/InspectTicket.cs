using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Data.PoliceModel
{
    public class InspectTicket
    {
        public List<InspectTicketInfo> dataVoList { get; set; }
        public string verification { get; set; }
    }
    /// <summary>
    /// 验票告警
    /// </summary>
    public class InspectTicketInfo
    {
        /// <summary>
        /// 当场任务id
        /// </summary>
        public string taskId { get; set; }
        /// <summary>
        /// 告警发生的时间
        /// </summary>
        public DateTime warningTime { get; set; }
        /// <summary>
        /// 告警发生的位置
        /// </summary>
        public string warningPosition { get; set; }
        /// <summary>
        /// 告警类型 告警类型 假票等于0,人票不统一等于1
        /// </summary>
        public int ticketWarningType { get; set; }
        /// <summary>
        /// 票编号，不可重复
        /// </summary>
        public string ticketNo { get; set; }
        /// <summary>
        /// 入场人员身份证号
        /// </summary>
        public string ticketHolderIdCardNo { get; set; }
        /// <summary>
        /// 持票人姓名
        /// </summary>
        public string ticketHolderName { get; set; }
        /// <summary>
        /// 入场人脸的图片路径
        /// </summary>
        public string entranceFacePic { get; set; }

    }
}
