using System.Threading;
using AFT_System.CustomControl;
using IrControlLibrary_Jun.Controls;
using IrLibrary_Jun.PublicClass;
using Newtonsoft.Json.Linq;

namespace AFT_System
{
    public class LoginWindowFactory
    {
        #region 窗体初始化
        private static readonly LoginWindow Logwindow = new LoginWindow();
        public static LoginWindow ConcreLoginWindow()
        {
            MyControlBoxBase.AddControlEvent += MyControlBoxBase_AddControlEvent;
            return Logwindow;
        }
        public static void Show()
        {
            Logwindow.Show();
        }
        public static void Close()
        {
            Logwindow.Close();
        }
        public static void DataUpdate()
        {
            Logwindow.Show();
        }
        #endregion


        #region 新控件事件
        private static IControlBaseProperty MyControlBoxBase_AddControlEvent(JObject oTemp, int kind)
        {
            IControlBaseProperty vwT = null;
            switch (kind)
            {
                case 2018:      //查询返回
                    {
                        vwT= new MyCustomControl(oTemp);
                    }
                    break;

            }
            if (vwT != null)
            {
                if (vwT.IsControlCreateSuccess) return vwT;
            }
            return null;
        }
        #endregion

 

    }
}
