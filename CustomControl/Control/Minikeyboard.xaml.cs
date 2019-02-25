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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.CustomControl.Control
{
    /// <summary>
    /// Minikeyboard.xaml 的交互逻辑
    /// </summary>
    public partial class Minikeyboard
    {
        private bool isfirst = true;
        public delegate void ChangeEvent(object sender, string str);
        public event ChangeEvent Changed;
        protected virtual void OnChanged(string str)
        {
            ChangeEvent handler = Changed;
            if (handler != null) handler(this, str);
        }
        public Minikeyboard()
        {
            InitializeComponent();
            Loaded += minikeyboard_Loaded;

        }

        void minikeyboard_Loaded(object sender, RoutedEventArgs e)
        {

            if (isfirst) SetMargin(MiniGrid);
            isfirst = false;
        }
        protected void SetZoom(Button uielement)
        {
            uielement.Width *= IrAdvanced.DblZoomWidth;
            uielement.Height *= IrAdvanced.DblZoomWidth;
            uielement.Margin = new Thickness(uielement.Margin.Left * IrAdvanced.DblZoomWidth, uielement.Margin.Top * IrAdvanced.DblZoomWidth, 0, 0);
            uielement.FontSize *= IrAdvanced.DblZoomWidth;
        }
        protected void SetMargin(Grid grid)
        {
            grid.Width *= IrAdvanced.DblZoomWidth;
            grid.Height *= IrAdvanced.DblZoomWidth;
            foreach (var child in grid.Children.OfType<Button>())
            {
                SetZoom(child);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IrAdvanced.BackTimeRun = 0;     //无人操作时间重置
            var cs = sender as Button;
            if (cs != null)
            {
                string tt = cs.Content.ToString();
                OnChanged(tt);
            }
        }

        public void Connect(int connectionId, object target)
        {

        }
    }
}
