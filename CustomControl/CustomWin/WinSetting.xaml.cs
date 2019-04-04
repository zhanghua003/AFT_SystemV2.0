using AFT_System.Data;
using AFT_System.Public;
using IrLibrary_Jun.Common;
using IrLibrary_Jun.PublicClass;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

namespace AFT_System.CustomControl.CustomWin
{
    /// <summary>
    /// WinSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WinSetting : Window
    {
        public WinSetting()
        {
            InitializeComponent();
            Loaded += WinSetting_Loaded;
        }

        private void WinSetting_Loaded(object sender, RoutedEventArgs e)
        {
            bool Is_AutoUpload = Convert.ToBoolean(ConfigHelper.GetConfig("Is_AutoUpload"));
            string Upload_Interval = ConfigHelper.GetConfig("Upload_Interval");
            string ServerGameName = ConfigHelper.GetConfig("ServerGameName");
            this.sp_interval.Visibility = Visibility.Hidden;
            this.sp_game.Visibility = Visibility.Visible;
            this.btn_UploadPolice.Visibility = Visibility.Visible;
            this.btn_save.Visibility = Visibility.Hidden;
            this.cb_auto.IsChecked = new bool?(Is_AutoUpload);
            this.txt_interval.Text = Upload_Interval;
            this.txt_game.Text = ServerGameName;
            List<MatchInfo> matchInfos = GetMatches();
            this.cmb_game.ItemsSource = matchInfos;
            this.cmb_gagame.ItemsSource = matchInfos;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();

            MessageBox.Show("保存成功！");
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.sp_interval.Visibility = Visibility.Visible;
            this.sp_game.Visibility = Visibility.Hidden;
            this.btn_UploadPolice.Visibility = Visibility.Hidden;
            this.btn_save.Visibility = Visibility.Visible;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.sp_interval.Visibility = Visibility.Hidden;
            this.sp_game.Visibility = Visibility.Visible;
            this.btn_UploadPolice.Visibility = Visibility.Visible;
            this.btn_save.Visibility = Visibility.Hidden;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Btn_UploadCenter_Click(object sender, RoutedEventArgs e)
        {
            MatchInfo? matchInfo = this.cmb_game.SelectedItem as MatchInfo?;
            if (matchInfo != null)
            {
                SubBaseForm sb = new SubBaseForm("正在同步比赛数据,请稍候...");
                SubBaseForm.DelegateHasParmAndNonReturn my = new SubBaseForm.DelegateHasParmAndNonReturn(UploadData);
                sb.HasParmAndNonReturnMethod(my, matchInfo.Value.SessionId);
            }
            else
            {
                MessageBox.Show("请选择比赛场次");
            }
        }

        private void UploadData(object sessionId)
        {
            CenterDataFactory.Upload(sessionId);
            MessageBox.Show("上传数据成功");
        }

        private void Btn_Synchronization_Click(object sender, RoutedEventArgs e)
        {
            MatchInfo? matchInfo = this.cmb_game.SelectedItem as MatchInfo?;
            if (matchInfo != null)
            {
                CenterDataFactory.BlackName(matchInfo.Value.SessionId);
                CenterDataFactory.WhiteName(matchInfo.Value.SessionId, matchInfo.Value.SessionName);
                MessageBox.Show("同步数据成功");
            }
            else
            {
                MessageBox.Show("请选择比赛场次");
            }
        }

        private void Btn_UploadPolice_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            string gameName = txt_game.Text;
            MatchInfo? matchInfo = this.cmb_gagame.SelectedItem as MatchInfo?;
            if (matchInfo != null)
            {
                PoliceDataFactory.Entrance(matchInfo.Value.SessionId, gameName);
                PoliceDataFactory.InspectTicket(matchInfo.Value.SessionId, gameName);
                PoliceDataFactory.Ticket(matchInfo.Value.SessionId, gameName);
                MessageBox.Show("同步数据成功");
            }
            else
            {
                MessageBox.Show("请选择比赛场次");
            }
        }

        private void SaveConfig()
        {
            bool Is_AutoUpload = this.cb_auto.IsChecked.Value;
            string Upload_Interval = this.txt_interval.Text;
            string ServerGameName = this.txt_game.Text;

            ConfigHelper.SetConfig("Is_AutoUpload", Is_AutoUpload.ToString());
            ConfigHelper.SetConfig("Upload_Interval", Upload_Interval);
            ConfigHelper.SetConfig("ServerGameName", ServerGameName);
        }

        private List<MatchInfo> GetMatches()
        {
            List<MatchInfo> matchInfos = new List<MatchInfo>();
            try
            {
                MysqlHelper.ConnStr = string.Format("Data Source={0};Database={1};User ID=root;Password=root;SslMode=None;", IrCommon.ReadString("ServerIp", "", "", "Main"), IrCommon.ReadString("DbName", "", "", "Main"));
                using (MySqlDataReader pass = MysqlHelper.ExecuteReader(string.Format("SELECT * FROM sessions INNER JOIN key_table on sessions.session_id=key_table.session_id ", new object[0]), CommandType.Text, new MySqlParameter[0]))
                {
                    while (pass.HasRows)
                    {
                        try
                        {
                            bool flag = pass.Read();
                            if (!flag)
                            {
                                break;
                            }
                            MatchInfo info = new MatchInfo();
                            bool flag2 = pass["session_id"] != DBNull.Value;
                            if (flag2)
                            {
                                info.SessionId = pass.GetInt32("session_id");
                            }
                            bool flag3 = pass["create_date"] != DBNull.Value;
                            if (flag3)
                            {
                                info.CreateDate = pass.GetDateTime("create_date");
                            }
                            bool flag4 = pass["session_name"] != DBNull.Value;
                            if (flag4)
                            {
                                info.SessionName = pass.GetString("session_name");
                            }
                            bool flag5 = pass["session_date"] != DBNull.Value;
                            if (flag5)
                            {
                                info.SessionDate = pass.GetDateTime("session_date");
                            }
                            bool flag6 = pass["date_start"] != DBNull.Value;
                            if (flag6)
                            {
                                info.DateStart = pass.GetDateTime("date_start");
                            }
                            bool flag7 = pass["date_end"] != DBNull.Value;
                            if (flag7)
                            {
                                info.DateEnd = pass.GetDateTime("date_end");
                            }
                            bool flag8 = pass["check_rule"] != DBNull.Value;
                            if (flag8)
                            {
                                info.CheckRule = pass.GetInt32("check_rule");
                            }
                            bool flag9 = pass["status"] != DBNull.Value;
                            if (flag9)
                            {
                                info.Status = pass.GetInt32("status");
                            }
                            bool flag10 = pass["remark"] != DBNull.Value;
                            if (flag10)
                            {
                                info.Remark = pass.GetString("remark");
                            }
                            bool flag11 = pass["key_content"] != DBNull.Value;
                            if (flag11)
                            {
                                info.Key = pass.GetString("key_content");
                            }
                            matchInfos.Add(info);
                        }
                        catch (Exception ex)
                        {
                            ex.ToSaveLog("GetMatch:");
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                ex2.ToSaveLog("获取场次信息GetMatch:");
            }
            return matchInfos;
        }
    }
}
