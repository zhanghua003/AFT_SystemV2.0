#region License Revision: 0 Last Revised: 2014-05-09
/******************************************************************************
 * 模块：登录界面
 * 作者：周俊
 * 创建时间：2014-05-10
 * 修改时间：2017-05-14
 * 功能：
    1、初始化窗体和变量
******************************************************************************/
#endregion // License

using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AFT_System.Face;
using AFT_System.Public;
using IrLibrary_Jun.Common;
using IrLibrary_Jun.PublicClass;
using Newtonsoft.Json.Linq;

namespace AFT_System
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        public int Count = 0;
        #region init window
        /// <summary>窗体初始并初始数据</summary>       
        public LoginWindow()
        {
            InitializeComponent();
            //必要环境初始化
            MysqlHelper.ConnStr = string.Format("Data Source={0};Database={1};User ID=root;Password=root;SslMode=None;", IrAdvanced.ReadString("ServerIp"), IrAdvanced.ReadString("DbName"));
            FaceFun.BaseDir = AppDomain.CurrentDomain.BaseDirectory;
            //初始化基础数据
            IrAdvanced.InitBaseData("AFT_SYSTEM");
            //是否调试状态
            Cursor = IrAdvanced.IsDebug ? Cursors.Arrow : Cursors.None;
            //是否置顶
            //Topmost = !IrAdvanced.IsDebug;
            if(IrAdvanced.ReadBoolean("UpData"))
            {
                IrAdvanced.RunApp(AppDomain.CurrentDomain.BaseDirectory + "updata.exe");
            }

        }

        /// <summary>窗体加载</summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                VerText.Text = string.Format("版本号:{0}",Assembly.GetExecutingAssembly().GetName().Version);
                if (IrAdvanced.CheckRegisterState) //必须注册，否则IrControlLibrary中可能有部分方法无法使用
                {
                    WelcomeText.Text = "程序加载中...\n设备号：" + IrAdvanced.StrDeviceId;
                    TurnToMainWindows();
                }
                else
                {
                    WelcomeText.Text = string.Format("程序未注册，请与供应商联系！\n设备号：{0}", IrAdvanced.StrDeviceId);
                }
            }
            catch (Exception ex)
            {
                IrAdvanced.WriteError("FrmMain.Window_Loaded:" + ex.Message);
            }

        }
        #endregion

        #region 窗体跳转
        /// <summary>完成文件下载</summary>
        private void TurnToMainWindows(bool recreate = true)
        {
            Count++;
            IrAdvanced.InitClass();            //初始本软件变量类
            if (IrAdvanced.ObjJsonData == null)            //检查JSON对象
            {
                WelcomeText.Text = "没有找到可用的数据或节目配置错误\n请与供应商联系！...\n\n设备号：" + IrAdvanced.StrDeviceId;
            }
            else
            {
                Hide();
                if (recreate)
                {
                    var frmNew = new MainWindow(Count == 1);
                    frmNew.Show();
                }
                else
                {
                    if (IrAdvanced.WinMain != null)  IrAdvanced.WinMain.Show(); 
                }
            }
        }
        #endregion

        #region 素材更新


        public void StartUpdateCheck()
        {
            Dispatcher.Invoke(() =>
            {
                if (WelcomeText != null) WelcomeText.Text = "程序加载中。。。\n";
                Grid2.Visibility = Visibility.Hidden;
            });
            Thread.Sleep(1000);
            while (IrAdvanced.WinMain != null)
            {
                var main = IrAdvanced.WinMain as MainWindow;
                if (main != null && !main.Isclosed)
                { Thread.Sleep(1000);
                }
                else
                {  break; } 
            }
            if (IrAdvanced.WinMain != null)
            {
                var main = IrAdvanced.WinMain as MainWindow;
                if (main != null)
                {
                    var thread = main.MyThread;
                    while (IrAdvanced.WinMain != null && thread != null && thread.ThreadState != ThreadState.Stopped && thread.ThreadState != ThreadState.Aborted)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        #endregion
    }
}
