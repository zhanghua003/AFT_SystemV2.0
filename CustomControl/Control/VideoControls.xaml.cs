using System.Windows;

namespace AFT_System.CustomControl.Control
{
    /// <summary>
    /// VideoUserControls.xaml 的交互逻辑
    /// </summary>
    public partial class VideoControls
    {
        public VideoControls()
        {
            InitializeComponent();
        }
        public object ImageView
        {
            get { return (object)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("ImageView", typeof(object), typeof(VideoControls),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVideoShow)));
        private static void OnVideoShow(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                VideoControls u = (VideoControls)d;
                u.VideoShow1.ViewDisplay(e.NewValue);
            }
        }
    }
}
