using System.Windows;

namespace AFT_System.CustomControl.Control
{
    /// <summary>
    /// VideoUserControls.xaml 的交互逻辑
    /// </summary>
    public partial class VideoControls2
    {
        public VideoControls2()
        {
            InitializeComponent();
        }
        public object ImageView2
        {
            get { return (object)GetValue(Image2Property); }
            set { SetValue(Image2Property, value); }
        }
        public static readonly DependencyProperty Image2Property =
            DependencyProperty.Register("ImageView2", typeof(object), typeof(VideoControls2),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVideoShow2)));
        private static void OnVideoShow2(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                VideoControls2 u = (VideoControls2)d;
                u.VideoShow1.ViewDisplay(e.NewValue);
            }
        }
    }
}
