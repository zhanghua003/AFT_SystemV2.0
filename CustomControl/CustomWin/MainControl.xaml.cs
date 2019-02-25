using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AFT_System.CustomControl.Mode;
using AFT_System.CustomControl.ModeView;
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.ICard;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;
using Timer = System.Timers.Timer;

namespace AFT_System.CustomControl.CustomWin
{
    /// <summary>
    /// MainControl.xaml 的交互逻辑
    /// </summary>
    public partial class MainControl:IDisposable
    {
        private BaseView _faceModel;
        public string UserName { get; set; }
        public bool IsInit { get; set; }
        /// <summary> 验证模式 </summary>
        public int Mode { get; set; }
        public MainControl()
        {
            InitializeComponent();
            Loaded += MainControl_Loaded;
        }

        void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Visible;
            TipGrid.Visibility = Visibility.Hidden;
            FaceGrid.Visibility = Visibility.Visible;
            SelectControl.Visibility = Visibility.Visible;
        }



        public void Dispose()
        {
            if (_faceModel != null) _faceModel.Dispose();
        }

        private void SelectMatch_OnClick(object sender, MatchInfo e)
        {
            if (Mode == 8)
            {
                SubBaseForm sb = new SubBaseForm("正在加载白名单人员数据,请稍候...");
                SubBaseForm.DelegateHasParmAndNonReturn my = new SubBaseForm.DelegateHasParmAndNonReturn(searchData);
                sb.HasParmAndNonReturnMethod(my, e.SessionId);
            }
            if (_faceModel != null)
            {
                _faceModel.MyMatch = e;
                MyGrid.Children.Add(_faceModel);
                _faceModel.Init(); 
            }
            SelectControl.Visibility = Visibility.Hidden;            
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (FaceGrid.Visibility == Visibility.Visible)
            {
                if (LoginGrid.Visibility == Visibility.Hidden)
                {
                    LoginGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    MyGrid.Children.ClearAll();
                    Environment.Exit(0);
                }
                
            }
            else
            {
                MyGrid.Children.ClearAll();

                FaceGrid.Visibility = Visibility.Visible;
                TipGrid.Visibility = Visibility.Hidden;
            }
           
        }

        private void ChooseMode(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                MyGrid.Children.ClearAll();
                TipGrid.Visibility = Visibility.Visible;
                Mode = IrAdvanced.StringToInt(btn.Tag.ToString(),1);
                switch (Mode)
                {
                    case 1: //身份证人脸识别验证
                        _faceModel = new FaceIdView(); break;
                    case 2://身份证人脸识别+年卡验证
                        _faceModel = new FaceIcView();break;
                    case 3: //身份证人脸识别+散票验证
                        _faceModel = new FaceQrView();break;
                    case 4://年卡验证，匹配年卡合法且摄像头拍照取证
                        _faceModel = new OnlyIcView();break;
                    case 5: //散票验证，匹配散票合法且摄像头拍照取证
                        _faceModel = new OnlyQrView();break;
                    case 6: //年卡白名单验证，匹配数据库中照片与前台摄像头照片是否一致
                        _faceModel = new FaceWhiteIc();break;
                    case 7: //二维码散票与年卡兼容模式
                        _faceModel = new OnlyQrOrIc();break;
                    case 8: //散票白名单验证，匹配数据库中照片与前台摄像头照片是否一致
                        _faceModel = new FaceWhiteQr(); break;
                }
                TxtBox.Text = "测试硬件连接状态.......\n";
                
                if (_faceModel != null)
                {
                    _faceModel.HardConn += (hardsender, harde) => Dispatcher.Invoke(() => { TxtBox.Text += harde; });
                    _faceModel.HardCompleted += delegate
                    {
                        Dispatcher.Invoke(() => { IsInit = !TxtBox.Text.Contains("失败"); });
                    };
                    _faceModel.TestHard();
                }
            }
        }



        private void LoginBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AftUser.Text != null)
                {
                    if (PwBox.Password == "123456")
                    {
                        UserName = AftUser.Text.Trim();
                        LoginGrid.Visibility = Visibility.Hidden;
                        TipGrid.Visibility = Visibility.Hidden;
                        FaceGrid.Visibility = Visibility.Visible;
                        SelectControl.Visibility = Visibility.Visible;
                        PwBox.Password = "";
                    }
                    else
                    {
                        MessageBox.Show("密码不正确", "错误");
                    }
                }

            }
            catch (Exception ex)
            {ex.ToSaveLog();}

        }

        private void searchData(object sessionid)
        {
            FaceFun.GetWhiteName((int)sessionid);
        }
        
        private void SysInit_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsInit)
            {
                //进入系统
                SelectControl.Visibility = Visibility.Visible;
                FaceGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                MyGrid.Children.ClearAll();
                _faceModel = null;
                TipGrid.Visibility = Visibility.Hidden;
            }
        }
    }
}
