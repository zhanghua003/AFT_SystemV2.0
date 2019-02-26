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
        }


        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper.SetConfig("Upload_Interval", "20");
            ConfigHelper.GetConfig("Upload_Interval");
            this.Close();
        }
    }
}
