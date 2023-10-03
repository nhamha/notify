using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerCore.Utilities.Security
{
    public class Security
    {
        private static readonly RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

        //disable Security contructor
        private Security() { }

        public static string GetVerifyToken(ref string verifyToken)
        {
            string captcha = string.Empty;
            return GetVerifyToken(ref verifyToken, ref captcha, 6);
        }

        public static string GetVerifyToken(ref string verifyToken, int length)
        {
            string captcha = string.Empty;
            return GetVerifyToken(ref verifyToken, ref captcha, length);
        }

        public static string GetVerifyToken(ref string verifyToken, ref string captcha)
        {
            return GetVerifyToken(ref verifyToken, ref captcha, 5);
        }

        public static string GetVerifyToken(ref string verifyToken, ref string captcha, int length)
        {
            const string key = "23456789ABCDEFGHIJKLMNPQRSTUVXYZabcdefghjklmnpqrstuvxyz";
            
            byte[] randomNumber = new byte[length];
            rngCsp.GetBytes(randomNumber);

            var keyLenght = key.Length;
            var s = "";
            for (var i = 0; i < length; i++)
            {
                s = s + key[(randomNumber[i] % keyLenght)];
            }

            var time = DateTime.Now.Ticks;
            captcha = s;
            verifyToken = string.Format("{0}-{1}", time, Security.MD5Encrypt(s + time.ToString()));
           
            return Security.MD5Encrypt(s.ToUpper());
            //return System.Web.HttpUtility.UrlEncode(Security.TripleDESEncrypt(Security.MD5Encrypt(Environment.MachineName), s.ToUpper()));
        }

        public static string GetVerifyToken(ref string verifyToken, ref string captcha, int length, int accountID, string accountName)
        {
            const string key = "123456789ABCDEFGHIJKLMNPQRSTUVXYZ";

            byte[] randomNumber = new byte[length];
            rngCsp.GetBytes(randomNumber);

            var keyLenght = key.Length;
            var s = "";
            for (var i = 0; i < length; i++)
            {
                s = s + key[(randomNumber[i] % keyLenght)];
            }

            var time = DateTime.Now.Ticks;
            captcha = s;
            verifyToken = string.Format("{0}-{1}-{2}-{3}", time, Security.MD5Encrypt(s + time), accountID, accountName);
            return System.Web.HttpUtility.UrlEncode(Security.TripleDESEncrypt(Security.MD5Encrypt(Environment.MachineName), verifyToken.ToUpper()));
        }

        

        public static string GetTokenPlainText(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            return Security.TripleDESDecrypt(Security.MD5Encrypt(Environment.MachineName), System.Web.HttpUtility.UrlDecode(token).Replace(" ", "+"));
        }

        public static string GetTokenPlainText2(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return string.Empty;
            }

            string text = "";
            string textDecr = Security.TripleDESDecrypt(Security.MD5Encrypt(Environment.MachineName), System.Web.HttpUtility.UrlDecode(token).Replace(" ", "+"));
            if(textDecr.Length > 0)
            {
                text = textDecr.Split('|')[0];
            }
            return text;
        }

        public static DateTime GetTokenTime(string verify)
        {
            string s = verify;
            var timeOfCurrentToken = Convert.ToInt64(s.Split('-')[0]);
            return new DateTime(timeOfCurrentToken);
        }

        

        public static DateTime GetTokenTime2(string verify)
        {
            string timeDecr = Security.TripleDESDecrypt(Security.MD5Encrypt(Environment.MachineName), System.Web.HttpUtility.UrlDecode(verify).Replace(" ", "+"));
            return new DateTime(Convert.ToInt64(timeDecr));
        }

        public static string MD5Encrypt(string plainText)
        {
            UTF8Encoding encoding1 = new UTF8Encoding();
            MD5CryptoServiceProvider provider1 = new MD5CryptoServiceProvider();
            byte[] buffer1 = encoding1.GetBytes(plainText);
            byte[] buffer2 = provider1.ComputeHash(buffer1);
            return BitConverter.ToString(buffer2).Replace("-", "").ToLower();
        }

        public static string RandomPassword()
        {
            string text1 = string.Empty;
            Random random1 = new Random(DateTime.Now.Millisecond);
            for (int num1 = 1; num1 < 10; num1++)
            {
                text1 = string.Format("{0}{1}", text1, random1.Next(0, 9));
            }
            return text1;
        }

        public static string RandomString(int length)
        {
            string text1 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()";
            int num1 = text1.Length;
            Random random1 = new Random();
            string text2 = string.Empty;
            for (int num2 = 0; num2 < length; num2++)
            {
                text2 = string.Format("{0}{1}", text2, text1[random1.Next(num1)]);
            }
            return text2;
        }

        public static string TripleDESEncrypt(string key, string data)
        {
            data = data.Trim();

            byte[] keydata = Encoding.ASCII.GetBytes(key);

            string md5String = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();

            byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

            TripleDES tripdes = TripleDESCryptoServiceProvider.Create();

            tripdes.Mode = CipherMode.ECB;

            tripdes.Key = tripleDesKey;

            tripdes.GenerateIV();

            MemoryStream ms = new MemoryStream();

            CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
                CryptoStreamMode.Write);

            encStream.Write(Encoding.ASCII.GetBytes(data), 0, Encoding.ASCII.GetByteCount(data));

            encStream.FlushFinalBlock();

            byte[] cryptoByte = ms.ToArray();

            ms.Close();

            encStream.Close();

            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
        }

        public static string TripleDESDecrypt(string key, string data)
        {
            data = data.Replace(' ', '+');
            try
            {
                byte[] keydata = Encoding.ASCII.GetBytes(key);

                string md5String = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();

                byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

                TripleDES tripdes = TripleDESCryptoServiceProvider.Create();

                tripdes.Mode = CipherMode.ECB;

                tripdes.Key = tripleDesKey;

                byte[] cryptByte = Convert.FromBase64String(data);

                MemoryStream ms = new MemoryStream(cryptByte, 0, cryptByte.Length);

                ICryptoTransform cryptoTransform = tripdes.CreateDecryptor();

                CryptoStream decStream = new CryptoStream(ms, cryptoTransform,
                    CryptoStreamMode.Read);

                StreamReader read = new StreamReader(decStream);

                return (read.ReadToEnd());
            }
            catch
            {
                // Wrong key
                // throw new Exception("Sai key mã hóa\t key: " + key + "\t Data: " + data);
                return string.Empty;
            }
        }

        public static string Encrypt(string key, string data)
        {
            data = data.Trim();

            if (string.IsNullOrEmpty(data))
                return "Input string is empty!";

            var keydata = Encoding.ASCII.GetBytes(key);

            var md5String = BitConverter.ToString(new

            MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();

            var tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

            var tripdes = TripleDES.Create();

            tripdes.Mode = CipherMode.ECB;

            tripdes.Key = tripleDesKey;

            tripdes.GenerateIV();

            var ms = new MemoryStream();

            var encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),

                    CryptoStreamMode.Write);

            encStream.Write(Encoding.ASCII.GetBytes(data), 0, Encoding.ASCII.GetByteCount(data));

            encStream.FlushFinalBlock();

            var cryptoByte = ms.ToArray();

            ms.Close();

            encStream.Close();

            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
        }

        public static string Decrypt(string key, string data)
        {
            var keydata = Encoding.ASCII.GetBytes(key);

            var md5String = BitConverter.ToString(new

              MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").Replace(" ", "+").ToLower();

            var tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));

            var tripdes = TripleDES.Create();

            tripdes.Mode = CipherMode.ECB;

            tripdes.Key = tripleDesKey;

            var cryptByte = Convert.FromBase64String(data);

            var ms = new MemoryStream(cryptByte, 0, cryptByte.Length);

            var cryptoTransform = tripdes.CreateDecryptor();

            var decStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read);

            var read = new StreamReader(decStream);

            return (read.ReadToEnd());
        }

        public static string encode(string data)
        {
            byte[] dataArr = Encoding.UTF8.GetBytes(data);
            for(int i = 0; i < dataArr.Length; i++)
            {
                dataArr[i] = (byte)(((dataArr[i] & 0xE0) >> 5) | ((dataArr[i] & 0x07) << 5) | (dataArr[i] & 0x18));
            }
            return Convert.ToBase64String(dataArr);
        }

        public static string encodeWeb(string data)
        {
            byte[] dataArr = Encoding.UTF8.GetBytes(data);
            for(int i = 0; i < dataArr.Length; i++)
            {
                dataArr[i] = (byte)(((dataArr[i] & 0xF0) >> 4) | ((dataArr[i] & 0x0F) << 4));
            }
            return Convert.ToBase64String(dataArr);
        }

        public static string decodeWeb(string data)
        {
            byte[] dataArr = Convert.FromBase64String(data);
            for(int i = 0; i < dataArr.Length; i++)
            {
                dataArr[i] = (byte)(((dataArr[i] & 0xF0) >> 4) | ((dataArr[i] & 0x0F) << 4));
            }
            return UTF8Encoding.UTF8.GetString(dataArr);
        }

        public static string decodeApp(string data)
        {
            byte[] dataArr = Convert.FromBase64String(data);
            for(int i = 0; i < dataArr.Length; i++)
            {
                dataArr[i] = (byte)(((dataArr[i] & 0xE0) >> 5) | ((dataArr[i] & 0x07) << 5) | (dataArr[i] & 0x18));
            }
            return UTF8Encoding.UTF8.GetString(dataArr);
        }

        public static string decode(string data)
        {
            if(string.IsNullOrEmpty(data))
                return "";

            byte[] dataByte = Convert.FromBase64String(data);
            
            int len = dataByte.Length;
            byte[] dataDecode = new byte[len - 1];
            byte key = (byte)(dataByte[len - 1]);

            len -= 1;
            for(int i = 0; i < len; i++)
            {
                dataDecode[i] = (byte)(((dataByte[i] & 0xE0) >> 5) | ((dataByte[i] & 0x07) << 5) | (dataByte[i] & 0x18));
                dataDecode[i] = (byte)(dataDecode[i] - key);
            }
            return UTF8Encoding.UTF8.GetString(dataDecode);
        }

        public static string Encrypt(string data)
        {
            if(string.IsNullOrEmpty(data))
                return null;

            Random rd = new Random();
            byte key = (byte)rd.Next(1, 99);
            byte[] dataEncrypt = new byte[data.Length + 1];

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(data);
            int len = byteData.Length;

            for(int i = 0; i < len; i++)
            {
                byte tem = (byte)(byteData[i] + key);
                dataEncrypt[i] = (byte)(((tem & 0xE0) >> 5) | ((tem & 0x07) << 5) | (tem & 0x18));
            }
            dataEncrypt[data.Length] = key;
            return Convert.ToBase64String(dataEncrypt);
        }

        #region OTP APP

        public static string GetVerifyTokenOTPLogin(int length, long accountID, string accountName, string mobile, string email = "")
        {
            const string key = "123456789ABCDEFGHIJKLMNPQRSTUVXYZ";

            byte[] randomNumber = new byte[length];
            rngCsp.GetBytes(randomNumber);

            var keyLenght = key.Length;
            var s = "";
            for (var i = 0; i < length; i++)
            {
                s = s + key[(randomNumber[i] % keyLenght)];
            }

            var time = DateTime.Now.Ticks;
            string verifyToken = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", time, Security.MD5Encrypt(s + time), accountID, accountName, mobile, email);
            return System.Web.HttpUtility.UrlEncode(Security.TripleDESEncrypt(Security.MD5Encrypt(Environment.MachineName), verifyToken.ToUpper()));
        }

        public static int GetTokenAccountID(string verify)
        {
            int accountID = Convert.ToInt32(verify.Split('-')[2]);
            return accountID;
        }
        public static string GetTokenAccountName(string verify)
        {
            string accountName = verify.Split('-')[3];
            return accountName;
        }

        public static string GetTokenMobile(string verify)
        {
            string mobile = verify.Split('-')[4];
            return mobile;
        }

        public static string GetTokenEmail(string verify)
        {
            string email = verify.Split('-')[5];
            return email;
        }

        public static string SHA256Encrypt(string inputString)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        public static string SHA512Encrypt(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        public static string GetPasswordEncryp(string password, string userName)
        {
            //var watch = Stopwatch.StartNew();

            string passwordSha = SHA256Encrypt(password + "." + userName);
            string keySha = SHA256Encrypt(password);
            string passwordEncryp = TripleDESEncrypt(keySha, passwordSha);
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //NLogManager.Info(string.Format("GetPasswordEncryp service: {0}", elapsedMs));
            return passwordEncryp;
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
        #endregion
    }
}