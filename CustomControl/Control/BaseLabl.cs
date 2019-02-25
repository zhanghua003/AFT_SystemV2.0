using System.Windows;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace AFT_System.CustomControl.Control
{
    public class BaseLabl:TextBlock
    {
        public BaseLabl()
        {
            TextAlignment =TextAlignment.Left;
            Foreground =Brushes.White;
            FontSize =26;
            HorizontalAlignment =HorizontalAlignment.Left;
            VerticalAlignment =VerticalAlignment.Top;
            Height = 38;
        }
    }
}
