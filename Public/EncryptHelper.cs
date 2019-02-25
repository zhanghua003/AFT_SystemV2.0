using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IrLibrary_Jun.PublicClass;

namespace AFT_System.Public
{
    public class EncryptHelper
    {
        private static readonly byte[] RgbKey = new byte[8] { 21, 129, 51, 64, 72, 96, byte.MaxValue, 238 };
        private static string Encrypt(string encryptString, byte[] keys)
        {
            try
            {
                byte[] rgbIv = keys;
                byte[] bytes = Encoding.UTF8.GetBytes(encryptString);
                var cryptoServiceProvider = new DESCryptoServiceProvider();
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateEncryptor(RgbKey, rgbIv), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                ex.ToSaveLog();
                return "";
            }
        }
        public static string Decrypt(string encryptString, string keys)
        {
            try
            {
                if (keys.Length > 0) keys = keys.Length >= 8 ? keys.Substring(0, 8) : keys.PadRight(8, 'z');

                return Decrypt(encryptString, Encoding.Default.GetBytes(keys));
            }
            catch (Exception ex)
            {
                ex.ToSaveLog();
                return "";
            }
        }
        public static string Decrypt(string decryptString, byte[] keys)
        {
            try
            {
                byte[] rgbIv = keys;
                byte[] buffer = Convert.FromBase64String(decryptString);
                var cryptoServiceProvider = new DESCryptoServiceProvider();
                using (var memoryStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(RgbKey, rgbIv), CryptoStreamMode.Write);
                    cryptoStream.Write(buffer, 0, buffer.Length);
                    cryptoStream.FlushFinalBlock();
                    memoryStream.ToArray();
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                ex.ToSaveLog("Decrypt解密二维码:");
                return "";
            }
        }

    }
}
