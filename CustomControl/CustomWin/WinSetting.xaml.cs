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
            bool Is_AutoUpload = Convert.ToBoolean(ConfigHelper.GetConfig("Is_AutoUpload"));
            string Upload_Interval = ConfigHelper.GetConfig("Upload_Interval");
            string ServerGameName = ConfigHelper.GetConfig("ServerGameName");

            this.sp_interval.Visibility = Visibility.Hidden;
            this.sp_game.Visibility = Visibility.Visible;
            this.btn_UploadPolice.Visibility = Visibility.Visible;
            this.btn_save.Visibility = Visibility.Hidden;

            this.cb_auto.IsChecked = Is_AutoUpload;
            this.txt_interval.Text = Upload_Interval;
            this.txt_game.Text = ServerGameName;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();

            MessageBox.Show("保存成功！");
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
            SaveConfig();

        }

        private void SaveConfig()
        {
            bool Is_AutoUpload = this.cb_auto.IsChecked.Value;
            string Upload_Interval = this.txt_interval.Text;
            string ServerGameName = this.txt_game.Text;

            ConfigHelper.SetConfig("Is_AutoUpload", Is_AutoUpload.ToString());
            ConfigHelper.SetConfig("Upload_Interval", Upload_Interval);
            ConfigHelper.SetConfig("ServerGameName", ServerGameName);
        }
    }
}
