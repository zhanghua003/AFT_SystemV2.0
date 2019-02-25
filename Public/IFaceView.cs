using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT_System.CVR;

namespace AFT_System.Public
{
    interface IFaceView
    {
       void ShowError(string msg,MsgArgs args);
       void ShowComplete();
       void ShowCamSource(Bitmap bitmap);
    }
    /// <summary>
    /// 身份证接口
    /// </summary>
    interface ICvr:IFaceView
    {
        void ShowCvr(CvrInfo cvrInfo);
    }
    /// <summary>
    /// IC年卡接口
    /// </summary>
    interface IICard : IFaceView
    {
        void ShowICard();
    }
    /// <summary>
    /// 散票接口
    /// </summary>
    interface IQr : IFaceView
    {
        void ShowQr();
    }
}
