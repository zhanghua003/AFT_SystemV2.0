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
using AFT_System.CVR;
using AFT_System.Face;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.CustomControl.CustomWin
{
    /// <summary>
    /// SelectMatch.xaml 的交互逻辑
    /// </summary>
    public partial class SelectMatch
    {
        private bool isfirst = true;
        private List<MatchInfo> _matchList = new List<MatchInfo>();
        public MatchInfo ThisMath;
        public event EventHandler<MatchInfo> Click;
        protected virtual void OnClick(MatchInfo info)
        {
            EventHandler<MatchInfo> handler = Click;
            if (handler != null) handler(this, info);
        }

        public SelectMatch()
        {
            InitializeComponent();
            Loaded += SelectMatch_Loaded;
        }

        void SelectMatch_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isfirst)
                {
                    //获取场次
                    _matchList = FaceFun.GetMatch();
                    foreach (var child in _matchList)
                    {
                        var item = new SelectItem(child);
                        item.Click += item_Click;
                        Spanel.Children.Add(item);
                    }
                    var firstOrDefault = Spanel.Children.OfType<SelectItem>().FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        firstOrDefault.Effect = new DropShadowEffect() { Color = Colors.White, BlurRadius = 20, Opacity = 1 };
                        ThisMath = firstOrDefault.Info;
                    }
                    isfirst = false;

                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("SelectMatch_Loaded:");
            }


        }



        void item_Click(object sender, EventArgs e)
        {
            var item = sender as SelectItem;
            if (item != null)
            {
                ThisMath = item.Info;
                foreach (var child in Spanel.Children.OfType<SelectItem>())
                {
                    child.Effect = Equals(child, item) ? new DropShadowEffect() { Color = Colors.White, BlurRadius = 20, Opacity = 1 } : null;
                }
            }

        }

        private void BtnOk_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            OnClick(ThisMath);
        }
    }
}
