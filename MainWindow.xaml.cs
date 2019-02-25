#region License Revision: 0 Last Revised: 2014-05-09
/******************************************************************************
 * 模块：广告主界面
 * 作者：周俊
 * 创建时间：2014-05-10
 * 修改时间：2014-05-14
******************************************************************************/
#endregion // License

using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using IrControlLibrary_Jun.Controls;
using IrControlLibrary_Jun.OtherLib.UsbBlackBoard;
using IrLibrary_Jun.PublicClass;
using IrLibrary_Jun.TCPandUDP.TCP;
using IrLibrary_Jun.Usb;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace AFT_System
{
    /// <summary>主界面窗体</summary>
    public partial class MainWindow
    {
        public bool Isclosed = false;
        public Thread MyThread;
        public static bool NeedCloseLoginWindow = true;
        private readonly bool _firstCreate=true;
        #region 实始化界面

        /// <summary> 初始化窗体 </summary>
        public MainWindow()
        {
            InitializeComponent();
            Cursor = IrAdvanced.IsDebug ? Cursors.Arrow : Cursors.None;
            //是否置顶
            Topmost = !IrAdvanced.IsDebug;
            MainGrid.Children.Add(IrAdvanced.MaintTransitionElement);
            IrAdvanced.MainGridPre = MainGridPre;
        }

        public MainWindow(bool firstCreate = true) : this()
        {
            _firstCreate = firstCreate;
        }

        #endregion

        #region 界面响应
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IrAdvanced.WriteInfo("节目页面加载");
                NeedCloseLoginWindow = true;
                IrAdvanced.LoadItemByDefault = false;
                IrAdvanced.WindowSet(this);
                TimeSpan stime,etime;
                //正常节目
                var normalitemindex = IrAdvanced.GetFirstItemId(IrAdvanced.ObjJsonItems, out stime, out etime);
                IrControlBaseJun.OpenNewProgram(normalitemindex, stime, etime, "9");
                IrAdvanced.WriteInfo("节目跳转:" + normalitemindex);

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("MainWindows.Load:");
            }
        }
 
        private void MainWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Isclosed = true;
            if (NeedCloseLoginWindow)
            {
                var frmNew = LoginWindowFactory.ConcreLoginWindow();
                frmNew.Close();
            }
        }      
        #endregion

    }
}
