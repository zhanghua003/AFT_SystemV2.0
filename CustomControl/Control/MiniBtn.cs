using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AFT_System.CustomControl.Control
{
    public class MiniBtn:Button
    {
        public MiniBtn()
        {
            FontSize = 20;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Width = 54;
            Height = 40;
        }
    }
}
