using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AFT_System.CustomControl.CustomWin;
using AFT_System.Face;
using AFT_System.Public;
using IrControlLibrary_Jun.Controls;
using IrLibrary_Jun.PublicClass;
using Newtonsoft.Json.Linq;

namespace AFT_System.CustomControl
{
    public class MyCustomControl:MyBaseControl
    {
        private readonly MainControl _myControl=new MainControl();
        /// <summary> 自定义控件 </summary>
        public MyCustomControl(JObject oJson): base(oJson)
        {
            try
            {
                //检查程序是否注册，防止盗用
                if (!IrAdvanced.CheckRegisterState) return;

                //获取图片文件，并检查文件是否有效
                _myControl.Mode = IrAdvanced.ReadInt("Mode",1);
                //_myControl.Init();
                Children.Add(_myControl);
                //设置控件ID号
                Tag = string.IsNullOrEmpty(SId) ? "2018_0" : SId;
                IsControlCreateSuccess = true;

            }
            catch (Exception ex)
            {
                ex.ToSaveLog();
            }
        }
        public override void Resizecontrol(JObject oJson, bool firstLoad)
        {
            try
            {
                base.Resizecontrol(oJson, firstLoad);
                //控件内容

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("MyU25CacheMediaPlay.Resizecontrol:");
            }
        }

    }
}
