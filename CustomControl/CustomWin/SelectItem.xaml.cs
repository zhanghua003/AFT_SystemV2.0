using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AFT_System.Public;

namespace AFT_System.CustomControl.CustomWin
{
    /// <summary>
    /// SelectItem.xaml 的交互逻辑
    /// </summary>
    public partial class SelectItem : UserControl
    {
        public MatchInfo Info;
        public event EventHandler Click;
        protected virtual void OnClick()
        {
            EventHandler handler = Click;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public SelectItem()
        {
            InitializeComponent();
            MouseUp += SelectItem_MouseUp;
        }

        void SelectItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
           OnClick();
        }

        public SelectItem(MatchInfo info):this()
        {
            Info = info;
            MyName.Text = info.SessionName;
            MyTime.Text = info.DateStart.ToString("比赛时间：yyyy年MM月dd日 HH:mm");
        }
    }
}
