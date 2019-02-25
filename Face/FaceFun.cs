using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AFT_System.ICard;
using AFT_System.Public;
using IrLibrary_Jun.PublicClass;
using MySql.Data.MySqlClient;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace AFT_System.Face
{


    public static class FaceFun
    {
        public static string BaseDir;
        public static List<WhiteNameInfo> WhiteList;
        public static MediaPlayer Player = new MediaPlayer();

        private static string DirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "Temp\\";

        #region 计时器
        public static Stopwatch TimeStopWatch = new Stopwatch();
        /// <summary> 计时器开始 </summary>
        public static void TimeStart()
        {
            TimeStopWatch.Restart();
        }
        /// <summary> 计时器结束 </summary>
        public static void TimeStop(string tip = "")
        {
            TimeStopWatch.Stop();
            TimeStopWatch.ElapsedMilliseconds.ToString().ToSaveLog(tip);
        }
        #endregion

        public static Bitmap DrawRectangleInPicture(Bitmap bmp, Rectangle rect, Color rectColor, int lineWidth)
        {
            try
            {
                "给人脸识别区域画框".ToSaveLog();
                if (bmp == null) return null;
                Graphics g = Graphics.FromImage(bmp);
                "准备笔刷".ToSaveLog("画框:");
                Brush brush = new SolidBrush(rectColor);
                "准备pan对象".ToSaveLog("画框:");
                var pen = new Pen(brush, lineWidth);
                "绘制矩形".ToSaveLog("画框:");
                g.DrawRectangle(pen, rect);
                "释放Graphics对象".ToSaveLog("画框:");
                g.Dispose();
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("DrawRectangleInPicture:");
            }
            "返回Bitmap".ToSaveLog("画框:");
            return bmp;
        }
        public static Bitmap DrawRectangleInPicture(Bitmap bmp, RrFaceT rf, Color rectColor, int lineWidth, float faceIr = 0f)
        {
            try
            {
                if (bmp == null) return null;
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    Brush brush = new SolidBrush(rectColor);
                    var pen = new Pen(brush, lineWidth);
                    int with = rf.rect.right - rf.rect.left;
                    int heigth = rf.rect.bottom - rf.rect.top;
                    var face = new Rectangle(rf.rect.left, rf.rect.top, with, heigth);
                    g.DrawRectangle(pen, face);
                    var text = string.Format("信值：{0}", Math.Round(faceIr * 100f, 2));
                    var myFont = new Font("微软雅黑", 10);
                    g.DrawString(text, myFont, brush, face.Left, face.Top - 20);
                }

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("DrawRectangleInPicture:");
            }
            return bmp;
        }

        #region Bitmap转Byte数组
        /// <summary>  Bitmap转Byte数组 </summary>
        public static byte[] BitmapToByte(Bitmap bitmap)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    var data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    return data;
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("BitmapToByte:");
            }
            return null;
        }
        public static Bitmap BytesToBitmap(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return new Bitmap(stream);
            }
        }
        public static BitmapImage ByteToBitmapImage(byte[] bytearray)
        {
            if (bytearray == null) return null;
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(bytearray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }
            return bmp;
        }
        #endregion


        /// <summary> 结构体转byte数组 </summary>
        /// <param name="structObj">要转换的结构体</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }
        /// <summary> byte数组转结构体 </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="type">结构体类型</param>
        /// <returns>转换后的结构体</returns>
        public static object BytesToStuct(byte[] bytes, Type type)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length) return null;//返回空 
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, 0, structPtr, size);
            //将内存空间转换为目标结构体
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构体
            return obj;
        }
        public static Bitmap SourceToBitmap(ImageSource imagesource)
        {
            var ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imagesource));
            encoder.Save(ms);
            var bp = new Bitmap(ms);
            ms.Close();
            return bp;
        }
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            var bitmapImage = new BitmapImage();
            var ms = new MemoryStream();
            bitmap.Save(ms, bitmap.RawFormat);
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        /// <summary> 人脸比对结论 </summary>
        /// <param name="rf1">人脸特征码1</param>
        /// <param name="rf2">人脸特征码2</param>
        public static Single FaceResult(RrFeatureT rf1, RrFeatureT rf2)
        {
            try
            {
                byte[] byFp1 = StructToBytes(rf1);
                byte[] byFp2 = StructToBytes(rf2);
                return FaceverifyDll.rr_fv_compare_features(byFp1, byFp2);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("FaceResult:");
            }
            return 0;
        }




        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        public static BitmapSource ToBitmapSource(Bitmap image)
        {
            BitmapSource bs = null;
            try
            {
                IntPtr ptr = image.GetHbitmap();
                bs = Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(ptr);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("ToBitmapSource");
            }
            return bs;
        }
        public static Bitmap DeepCopyBitmap(Bitmap bitmap)
        {
            try
            {
                Bitmap dstBitmap = null;
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, bitmap);
                    ms.Seek(0, SeekOrigin.Begin);
                    dstBitmap = (Bitmap)bf.Deserialize(ms);
                    ms.Close();
                }
                return dstBitmap;
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("ToBitmapSource");
            }
            return null;
        }
        public static RenderTargetBitmap GetUiBitmap(Visual vsual, int width, int height)
        {
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(vsual);
            return rtb;
        }
        public static void PlayMp3(string filePath, bool ispass = false)
        {
            try
            {
                filePath.ToSaveLog("语音播报.");
                var path = IrAdvanced.GetAbsolutePath(filePath);
                if (!path.IsNullOrEmpty())
                {
                    Player.Open(new Uri(path, UriKind.Relative));
                    Player.Play();
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("PlayMp3:");
            }
        }

        #region 数据库存取函数
        /// <summary> 检查身份证号是否在白名单中 </summary>
        /// <param name="idNo">身份证号</param>
        public static bool IsInWhiteList(string idNo)
        {
            try
            {
                idNo.ToSaveLog("白名单校验:");
                using (var hashim = MysqlHelper.ExecuteReader(string.Format("select id_no from white_names where id_no like '{0}'", idNo)))
                {
                    if (!hashim.HasRows) idNo.ToSaveLog("白名单未通过：");
                    return hashim.HasRows;
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("IsInWhiteList:");
                return false;
            }

        }
        /// <summary> 检查是否存在于白名单 </summary>
        /// <param name="idNo">身份证号码</param>
        /// <returns></returns>
        public static WhiteNameInfo? CheckWhiteName(string idNo)
        {
            idNo.ToSaveLog("查询数据库白名单:");
            var info = new WhiteNameInfo();
            try
            {
                using (var whiteName = MysqlHelper.ExecuteReader(string.Format("select * from white_names where id_no = '{0}'", idNo)))
                {
                    if (whiteName.HasRows)
                    {
                        if (whiteName.Read())
                        {
                            if (whiteName[1] != DBNull.Value) info.SessionId = whiteName.GetInt32("session_id");
                            if (whiteName[2] != DBNull.Value) info.CreateDate = whiteName.GetDateTime("create_date");
                            if (whiteName[3] != DBNull.Value) info.BuyName = whiteName.GetString("buy_name");
                            if (whiteName[4] != DBNull.Value) info.IdNo = whiteName.GetString("id_no");
                            if (whiteName[5] != DBNull.Value) info.IdCardPhoto = (byte[])whiteName["id_card_photo"];
                            if (whiteName[6] != DBNull.Value) info.YearTicketPhoto = (byte[])whiteName["year_ticket_photo"];
                            if (whiteName[7] != DBNull.Value) info.Address = whiteName.GetString("address");
                            if (whiteName[8] != DBNull.Value) info.TicketType = whiteName.GetInt32("ticket_type");
                            if (whiteName[9] != DBNull.Value) info.TicketNo = whiteName.GetString("ticket_no");
                            if (whiteName[10] != DBNull.Value) info.Area = whiteName.GetString("area");
                            if (whiteName[11] != DBNull.Value) info.Row = whiteName.GetString("row");
                            if (whiteName[12] != DBNull.Value) info.Seat = whiteName.GetString("seat");
                            if (whiteName[13] != DBNull.Value) info.TelNo = whiteName.GetInt32("tel_no");
                            if (whiteName[14] != DBNull.Value) info.TelArea = whiteName.GetString("tel_area");
                            if (whiteName[15] != DBNull.Value) info.Status = whiteName.GetInt32("status");
                            if (whiteName[16] != DBNull.Value) info.Remark = whiteName.GetString("remark");
                        }
                        return info;
                    }
                    idNo.ToSaveLog("白名单未通过：");
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("CheckWhiteName:");
            }
            return null;
        }


        /// <summary> 是否已经存在入场流水表里 </summary>
        /// <param name="idNo">身份证号</param>
        /// <param name="isIdNo">是否是身份证号</param>
        public static bool IsInSessions(string idNo, bool isIdNo = true)
        {
            try
            {
                var str = isIdNo ? "id_no" : "ticket_no";
                str = string.Format("select id_no from in_sessions where {0} like '{1}'", str, idNo);
                using (var pass = MysqlHelper.ExecuteReader(str))
                {
                    return pass.HasRows;
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("入场校验IsInSessions:");
            }
            return false;
        }
        /// <summary> 是否已经存在黑名单表里 </summary>
        /// <param name="idNo">身份证号</param>
        public static bool IsInBlack(string idNo)
        {
            try
            {
                idNo.ToSaveLog("黑名单检查:");
                if (!idNo.IsNullOrEmpty())
                {
                    var str = string.Format("select id_no from black_names where id_no like '{0}'", idNo);
                    using (var pass = MysqlHelper.ExecuteReader(str))
                    {
                        return pass.HasRows;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("黑名单校验IsInBlack:");
            }
            return false;
        }
        /// <summary> 添加入场流水记录进数据库 </summary>
        /// <param name="info">入场记录</param>
        public static int AddSessions(SessionsInfo info)
        {
            try
            {
                info.IdNo.ToSaveLog("写入场流水记录:");
                var sb = new StringBuilder();
                sb.Append("insert into in_sessions(session_id,create_date,name,id_no,id_card_photo,take_photo,face_data,id_address,ticket_type,ticket_no,area,row,seat,tel_no,tel_area,buy_name,buy_photo,buy_date,validate_type,sync_time,status,remark) ");
                sb.Append("VALUES(?session_id,?create_date,?name,?id_no,?id_card_photo,?take_photo,?face_data,?id_address,?ticket_type,?ticket_no,?area,?row,?seat,?tel_no,?tel_area,?buy_name,?buy_photo,?buy_date,?validate_type,?sync_time,?status,?remark) ");
                MySqlParameter[] parameters = {
                                             new MySqlParameter("?session_id", MySqlDbType.Int32),
                                             new MySqlParameter("?create_date", MySqlDbType.DateTime),
                                             new MySqlParameter("?name", MySqlDbType.VarChar),
                                             new MySqlParameter("?id_no", MySqlDbType.VarChar),
                                             new MySqlParameter("?id_card_photo", MySqlDbType.MediumBlob),
                                             new MySqlParameter("?take_photo", MySqlDbType.MediumBlob),
                                             new MySqlParameter("?face_data", MySqlDbType.MediumBlob),
                                             new MySqlParameter("?id_address", MySqlDbType.VarChar),
                                             new MySqlParameter("?ticket_type", MySqlDbType.Int32),
                                             new MySqlParameter("?ticket_no", MySqlDbType.VarChar),
                                             new MySqlParameter("?area", MySqlDbType.VarChar),
                                             new MySqlParameter("?row", MySqlDbType.VarChar),
                                             new MySqlParameter("?seat", MySqlDbType.VarChar),
                                             new MySqlParameter("?tel_no", MySqlDbType.VarChar),
                                             new MySqlParameter("?tel_area", MySqlDbType.VarChar),
                                             new MySqlParameter("?buy_name", MySqlDbType.VarChar),
                                             new MySqlParameter("?buy_photo", MySqlDbType.MediumBlob),
                                             new MySqlParameter("?buy_date", MySqlDbType.DateTime),
                                             new MySqlParameter("?validate_type", MySqlDbType.Int32),
                                             new MySqlParameter("?sync_time", MySqlDbType.DateTime),
                                             new MySqlParameter("?status", MySqlDbType.Int32),
                                             new MySqlParameter("?remark", MySqlDbType.VarChar),
                                         };
                parameters[0].Value = info.SessionId;
                parameters[1].Value = info.CreateDate;
                parameters[2].Value = info.Name;
                parameters[3].Value = info.IdNo;
                parameters[4].Value = info.IdCardPhoto;
                parameters[5].Value = info.TakePhoto;
                parameters[6].Value = info.FaceData;
                parameters[7].Value = info.IdAddress;
                parameters[8].Value = info.TicketType;
                parameters[9].Value = info.TicketNo;
                parameters[10].Value = info.Area;
                parameters[11].Value = info.Row;
                parameters[12].Value = info.Seat;
                parameters[13].Value = info.TelNo;
                parameters[14].Value = info.TelArea;
                parameters[15].Value = info.BuyName;
                parameters[16].Value = info.BuyPhoto;
                parameters[17].Value = info.BuyDate;
                parameters[18].Value = info.ValidateType;
                parameters[19].Value = info.SyncTime;
                parameters[20].Value = info.Status;
                parameters[21].Value = info.Remark;
                return MysqlHelper.ExecuteNonQuery(sb.ToString(), CommandType.Text, parameters);
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("写入数据库失败AddSessions:");
            }
            return 0;
        }

        /// <summary> 获取场次信息 </summary>
        /// <param name="type">0为未开始，1为开始，2为结束</param>
        public static List<MatchInfo> GetMatch(int type = 0)
        {
            var matchInfos = new List<MatchInfo>();
            try
            {
                using (var pass = MysqlHelper.ExecuteReader(string.Format("SELECT * FROM sessions INNER JOIN key_table on sessions.session_id=key_table.session_id and sessions.`status`={0}", type)))
                {
                    while (pass.HasRows)
                    {
                        try
                        {
                            if (pass.Read())
                            {
                                var info = new MatchInfo();
                                if (pass["session_id"] != DBNull.Value) info.SessionId = pass.GetInt32("session_id");
                                if (pass["create_date"] != DBNull.Value) info.CreateDate = pass.GetDateTime("create_date");
                                if (pass["session_name"] != DBNull.Value) info.SessionName = pass.GetString("session_name");
                                if (pass["session_date"] != DBNull.Value) info.SessionDate = pass.GetDateTime("session_date");
                                if (pass["date_start"] != DBNull.Value) info.DateStart = pass.GetDateTime("date_start");
                                if (pass["date_end"] != DBNull.Value) info.DateEnd = pass.GetDateTime("date_end");
                                if (pass["check_rule"] != DBNull.Value) info.CheckRule = pass.GetInt32("check_rule");
                                if (pass["status"] != DBNull.Value) info.Status = pass.GetInt32("status");
                                if (pass["remark"] != DBNull.Value) info.Remark = pass.GetString("remark");
                                if (pass["key_content"] != DBNull.Value) info.Key = pass.GetString("key_content");
                                matchInfos.Add(info);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ToSaveLog("GetMatch:");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ex.ToSaveLog("获取场次信息GetMatch:");
            }

            return matchInfos;
        }

        public static List<WhiteNameInfo> GetWhiteName(int sessionid)
        {
            List<WhiteNameInfo> list = new List<WhiteNameInfo>();
            try
            {
                if (Directory.Exists(DirectoryPath))
                    Directory.Delete(DirectoryPath, true);
                DirectoryInfo dinfo = Directory.CreateDirectory(DirectoryPath);
                using (MySqlDataReader reader = MysqlHelper.ExecuteReader(string.Format("SELECT * from white_names where session_id ={0}", sessionid)))
                {
                    try
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                byte[] IdCardPhoto = (byte[])reader["id_card_photo"];
                                WhiteNameInfo info = new WhiteNameInfo()
                                {
                                    BuyName = reader.GetString("buy_name"),
                                    IdNo = reader.GetString("id_no"),
                                    rrfeature = Newtonsoft.Json.JsonConvert.DeserializeObject<FaceVerifyLib.rr_feature_t>(reader.GetString("rrfeaturet"))
                                };
                                info.IdCardPhotoPath = SaveImage(IdCardPhoto, info.IdNo + ".jpg");
                                list.Add(info);
                            }
                            catch (Exception ex)
                            {
                                ex.ToSaveLog("GetWhiteName---reader.Read:");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToSaveLog("GetWhiteName:");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("获取白名单信息GetWhiteName:");
            }
            WhiteList = list;
            return list;
        }

        public static bool IsInSessions(string idNo, int sessionid, out DateTime checkTime)
        {
            checkTime = DateTime.Now;
            try
            {
                string str = string.Format("select id,create_date from in_sessions where session_id = {0} and id_no='{1}'", sessionid, idNo);
                using (MySqlDataReader pass = MysqlHelper.ExecuteReader(str))
                {
                    if (pass.Read())
                    {
                        checkTime = Convert.ToDateTime(pass.GetString("create_date"));
                    }
                    return pass.HasRows;
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("入场校验IsInSessions:");
            }
            return false;
        }

        public static string SaveImage(byte[] imagebyte, string imageName)
        {
            MemoryStream fs = new MemoryStream((int)imagebyte.Length);
            fs.Write(imagebyte, 0, (int)imagebyte.Length);
            Image image = System.Drawing.Image.FromStream(fs);
            string filename = DirectoryPath + imageName;
            image.Save(filename, ImageFormat.Png);
            return filename;
        }
        public static int TestMatch()
        {
            return IrAdvanced.StringToInt(MysqlHelper.ExecuteScalar("SELECT count(*) FROM sessions").ToString(), 0);
        }
        #endregion
    }
}
