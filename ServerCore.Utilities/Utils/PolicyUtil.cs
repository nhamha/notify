using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ServerCore.Utilities.Security;
using System.IO;

namespace ServerCore.Utilities.Utils
{
    public static class PolicyUtil
    {
        //private static string RuleNickNameFile = ConfigurationManager.AppSettings["RuleNickNameFile"];
        //private static string RuleUserNameFile = ConfigurationManager.AppSettings["RuleUserNameFile"];
        private static string RuleNickName;

        static PolicyUtil()
        {
            RuleNickName = File.ReadAllText(Directory.GetCurrentDirectory() + "/AppData/RuleNickNameFile.txt");
        }

        public static bool CheckPassword(string password)
        {
            if (password.Length < 6)
                return false;

            return password.Select(c => (byte)c).All(code => !((code < 33) | (code > 122)));
        }

        public static bool CheckUserName(string userName)
        {
            try
            {
                NLogManager.Info("CheckUserName 0" );
                // Độ dài từ 6-18
                if (userName.Length < 6 || userName.Length > 18)
                    return false;

                //Kí tự đầu tiên phải là chữ cái
                var fillterChar = "abcdefghijklmnopqrstuvxyzw0123456789._";
                if(fillterChar.IndexOf(userName[0]) < 0)
                    return false;
                NLogManager.Info("CheckUserName 2");
                //Kí tự '.' không được xuất hiện liền nhau
                if (userName.IndexOf("..") >= 0)
                    return false;

                // Ký tự '.' không được ở sau cùng
                if(userName.EndsWith("."))
                    return false;

                // Kiem tra userName co vi phạm các từ khóa hay không
                //var file = new System.IO.StreamReader(HttpContext.Current.Server.MapPath(RuleUserNameFile));
                //string ruleData = file.ReadToEnd();
                //file.Close();

                //if(ruleData.IndexOf(userNameLower) > 0)
                //    return false;

                string userNameLower = userName.ToLower();
                if(userNameLower.Contains("daili") || userNameLower.Contains("admin"))
                    return false;

                //Chuỗi hợp lệ   abcdefghijklmnopqrstuvxyzw012345678.
                fillterChar = "abcdefghijklmnopqrstuvxyzw0123456789._";
                return userName.All(t => fillterChar.IndexOf(t) >= 0);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                return false;
            }
        }

        public static bool CheckNickName(string nickName)
        {
            try
            {
                if(nickName.Length < 4 || nickName.Length > 100)
                    return false;

                //Kí tự đầu tiên phải là chữ cái
                //var fillterChar = "abcdefghijklmnopqrstuvxyzwABCDEFGHIJKLMNOPQRSTUVXYZW";
                //if (fillterChar.IndexOf(nickName[0]) < 0)
                //    return false;

                //Kí tự '.' không được xuất hiện liền nhau
                if(nickName.IndexOf("..") >= 0)
                    return false;

                // Ký tự '.' không được ở sau cùng
                if(nickName.EndsWith("."))
                    return false;

                string nickNameLower = nickName.ToLower();
                if(nickNameLower.Contains("daili") || nickNameLower.Contains("admin"))
                    return false;

                //string ruleData = string.Empty;

                //var file = new System.IO.StreamReader(Directory.GetCurrentDirectory() + "/AppData/RuleNickNameFile.txt");
                //ruleData = file.ReadToEnd();
                //file.Close();

                if(RuleNickName.Contains(nickName))
                    return false;

                string fillterChar = "abcdefghijklmnopqrstuvxyzwABCDEFGHIJKLMNOPQRSTUVXYZW0123456789._";
                return nickName.All(t => fillterChar.IndexOf(t) >= 0);
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
                return false;
            }
        }

        public static bool CheckEmail(string email)
        {
            //Kiểm tra định dạng email
            return Regex.IsMatch(email, @"^([0-9a-z]+[-._+&])*[0-9a-z]+@([-0-9a-z]+[.])+[a-z]{2,6}$", RegexOptions.IgnoreCase);
        }

        public static bool CheckMobile(string mobile)
        {
            //Kiểm tra định dạng mobile
            return Regex.IsMatch(mobile, @"^([0-9])", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Get ra kiểu email (yahoo, gmail, msn) từ địa chỉ email
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns>
        /// 0: Loại khác
        /// 1: Yahoo
        /// 2: gmail
        /// 3: msn
        /// </returns>
        public static int GetEmailType(string emailAddress)
        {
            var listyahoo = new List<string> { "yahoo", "ymail.com", "rocketmail.com" };
            var listgmail = new List<string> { "gmail.com", "googlemail.com" };
            var listmsn = new List<string> { "hotmail.com", "msn.com", "live.com" };
            var domain = emailAddress.Substring(emailAddress.IndexOf('@') + 1);
            if(listyahoo.Exists(e => domain.ToLower().StartsWith(e)))
                return 1;
            if(listgmail.Exists(e => domain.ToLower().StartsWith(e)))
                return 2;
            return listmsn.Exists(e => domain.ToLower().StartsWith(e)) ? 3 : 0;
        }

        /// <summary>
        /// Kiểm tra độ mạnh của password
        /// pass phải từ 6-16 ký tự, bao gồm cả chữ cái và chữ số
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordStrong(string password)
        {
            return Regex.IsMatch(password, @"^(?=.{6,16})(?=.*\d)(?=.*[a-zA-Z]).*$");
        }

        public static bool IsServiceKeyOk(string key, long accountId)
        {
            bool res = false;
            try
            {
                if(string.IsNullOrEmpty(key) || accountId <= 0)
                    res = false;

                DateTime currTime = DateTime.Now;
                string serverKey = "";//Security.Security.MD5Encrypt(Config.ServiceKeyReport + currTime.Hour + currTime.Day + currTime.Month + currTime.Year + accountId);
                NLogManager.Info("serverKey = " + serverKey);
                NLogManager.Info("key = " + key);
                if(serverKey.Equals(key))
                    res = true;
            }
            catch(Exception ex)
            {
                NLogManager.Error(ex.ToString());
                res = false;
            }
            return res;
        }

        public static bool IsServiceKeyLogin(string key)
        {
            bool res = false;
            try
            {
                if(string.IsNullOrEmpty(key))
                    res = false;

                DateTime currTime = DateTime.Now;
                string serverKey = Security.Security.MD5Encrypt("Game@2020" + currTime.Hour + currTime.Day + currTime.Month + currTime.Year);
                if(serverKey.CompareTo(key) == 0)
                    res = true;
            }
            catch(Exception ex)
            {
                NLogManager.Error(ex.ToString());
                res = false;
            }
            return res;
        }
    }
}
