using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.Public
{
    public sealed class IrSerialPort:IDisposable
    {
        /// <summary> 发生错误时触发事件 </summary>
        public event EventHandler<string> ErrorEvent;
        private void OnErrorEvent(string e)
        {
            EventHandler<string> handler = ErrorEvent;
            if (handler != null) handler(this, e);
        }
        /// <summary> 数据接收事件 </summary>
        public event EventHandler<string> DataReceived;
        private void OnDataReceived(string e)
        {
            EventHandler<string> handler = DataReceived;
            if (handler != null) handler(this, e);
        }
        private SerialPort _spProjector;
        public IrSerialPort(string port)
        {
            if (!port.IsNullOrEmpty())
            {
                OpenProjectorPorts(port);
            }
            else
            {
            "Setting中没有配置条码端口号.".ToSaveLog();
                OnErrorEvent("Setting中没有配置条码端口号.");
            }

        }

        void OpenProjectorPorts(string port)
        {
            try
            {
                _spProjector = new SerialPort(port)
                {
                    //波特率
                    BaudRate = 115200,
                    //数据位
                    DataBits = 8,
                    //停止位
                    StopBits = StopBits.One,
                    //校检位
                    Parity = Parity.None
                };
                _spProjector.DataReceived += SpProjector_DataReceived;
                _spProjector.Open();
                if (_spProjector.IsOpen)
                {
                    "连接成功".ToSaveLog();
                    OnErrorEvent("连接成功");
                }
                else
                {
                    "连接串口失败".ToSaveLog();
                    OnErrorEvent("连接串口失败");
                }
                
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("OpenProjectorPorts:");
                OnErrorEvent("连接串口失败");
            }
        }

        private void SpProjector_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var serialPort = sender as SerialPort;
                if (serialPort != null)
                {
                    int n = serialPort.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致  
                    var buf = new byte[n];//声明一个临时数组存储当前来的串口数据
                    serialPort.Read(buf, 0, n);//读取缓冲数据
                    OnDataReceived(Encoding.ASCII.GetString(buf));
                }
            }
            catch (Exception ex)
            {
               ex.ToSaveLog("串口接收数据时发生错误:");
            }
        }

        public void Dispose()
        {
              if(_spProjector!=null) _spProjector.Dispose();  
        }
    }
}
