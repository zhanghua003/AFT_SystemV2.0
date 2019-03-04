using System.Diagnostics;
using System.Linq;
using System.Windows;
using IrLibrary_Jun.PublicClass;

namespace AFT_System
{
    /// <summary> App.xaml 的交互逻辑 </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            ////Face.FaceFun.GetWhiteName(1);
            //if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            //{
            //    IrAdvanced.WriteError("程序已经运行了一个实例，该程序只允许有一个实例");
            //    Current.Shutdown();
            //}
            //LoginWindowFactory.ConcreLoginWindow().Show();

            int session_id = Data.CenterDataFactory.KeyTable();
            Data.CenterDataFactory.BlackName(session_id);

            CustomControl.CustomWin.WinSetting win = new CustomControl.CustomWin.WinSetting();
            win.Show();
        }
    }
}
