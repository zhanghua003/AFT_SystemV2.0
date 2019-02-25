using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace AFT_System.CustomControl.Control
{
    /// <summary>
    /// FrmLoading.xaml 的交互逻辑
    /// </summary>
    public partial class FrmLoading : Window
    {
        public BackgroundWorker backgroundWorker1;

        public FrmLoading()
        {
            InitializeComponent();
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Thread.Sleep(5000);
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.backgroundWorker1.IsBusy)
            {
                this.backgroundWorker1.RunWorkerAsync();
            }

        }

        public void SetProgressText(string desc)
        {

            //this.message.Content = desc;

            this.Dispatcher.Invoke(new Action(() =>
            {
                this.message.Content = desc;
            }));

            //new Thread(() =>
            //{
            //    this.Dispatcher.Invoke(new Action(() =>
            //    {
            //        this.message.Content = desc;
            //    }));
            //}).Start();

        }
    }
}
