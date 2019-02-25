using System;
using System.Drawing;
using System.Windows.Forms;

namespace AFT_System.CustomControl.Mode
{
    public partial class VideoShow : UserControl
    {
        public VideoShow()
        {
            InitializeComponent();
        }

        public new int Width
        {
            get { return pictureBox1.Width; }
            set { pictureBox1.Width = value; }
        }

        public new int Height
        {
            get { return pictureBox1.Height; }
            set { pictureBox1.Height = value; }
        }

        public void ViewDisplay(object Img)
        {
            try
            {
                pictureBox1.Image =(Image)Img;
            }
            catch (Exception)
            {
                pictureBox1.Image = null;
            }
        }
    }
}
