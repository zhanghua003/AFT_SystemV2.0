using AFT_System.Public;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AFT_System.CustomControl.CustomWin
{
    /// <summary>
    /// WinSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WinSetting : Window
    {
        public WinSetting()
        {
            InitializeComponent();
            Loaded += WinSetting_Loaded;
        }

        private void WinSetting_Loaded(object sender, RoutedEventArgs e)
        {
            this.cb_auto.IsChecked = true;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper.SetConfig("Upload_Interval", "20");
            ConfigHelper.GetConfig("Upload_Interval");
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.sp_interval.Visibility = Visibility.Visible;
            this.sp_game.Visibility = Visibility.Hidden;
            this.btn_UploadPolice.Visibility = Visibility.Hidden;
            this.btn_save.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.sp_interval.Visibility = Visibility.Hidden;
            this.sp_game.Visibility = Visibility.Visible;
            this.btn_UploadPolice.Visibility = Visibility.Visible;
            this.btn_save.Visibility = Visibility.Hidden;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Btn_UploadCenter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_Synchronization_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_UploadPolice_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
