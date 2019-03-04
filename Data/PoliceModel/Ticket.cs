using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Data.PoliceModel
{
    public class Ticket
    {
        public List<TicketInfo> dataVoList { get; set; }
        public string verification { get; set; }
    }
    /// <summary>
    /// 购票信息
    /// </summary>
    public class TicketInfo
    {
        /// <summary>
        /// 本次活动
        /// </summary>
        public string activityId { get; set; }
        /// <summary>
        /// 购票人员姓名
        /// </summary>
        public string purchaserName { get; set; }
        /// <summary>
        /// 购票人员身份证号码
        /// </summary>
        public string purchaserIdCardNo { get; set; }
        /// <summary>
        /// 购票人员电话
        /// </summary>
        public string purchaserTel { get; set; }
        /// <summary>
        /// 购票座位号
        /// </summary>
        public string purchaserSeatNo { get; set; }
        /// <summary>
        /// 票编号
        /// </summary>
        public string ticketNo { get; set; }
    }
}
