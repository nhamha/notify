using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ServerCore.Utilities.Utils
{
    public class StringUtil
    {
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static string MaskUserName(string username)
        {
            return "";
            if (string.IsNullOrEmpty(username))
            {
                return "";
            }

            int length = username.Length;
            if (length < 4)
                username = string.Format("{0}***", username.Substring(0, 2));
            else
                username = string.Format("{0}***", username.Substring(0, 3));

            //neu dai it nhat 11 ky tu -> lay 8 ky tu dau và ***
            //if (length > 10)
            //{
            //    username = string.Format("{0}***", username.Substring(0, 8));
            //}
            ////neu dai it nhat 7 ky tu -> thi *** 3 ky tu cuoi
            //else if (length > 6)
            //{
            //    username = string.Format("{0}***", username.Substring(0, length - 3));
            //}
            ////neu dai it nhat 4 ky tu -> lay 3 ky tu dau va ***
            //else if (length > 3)
            //{
            //    username = string.Format("{0}***", username.Substring(0, 3));
            //}
            //else
            //{
            //    username = string.Format("{0}***", username.Substring(0, 1));
            //}

            return username;
        }

        public static string MaskIpAddress(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return "";
            try
            {
                string[] splitIp = ip.Split('.');
                string newIp = string.Format("{0}.{1}.***.***", splitIp[0], splitIp[1]);
                return newIp;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return "";
            try
            {
                string[] splitIp = email.Split('@');
                string newEmail = string.Format("{0}*********@{1}", splitIp[0].Substring(0, 3), splitIp[1]);
                return newEmail;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string MaskMobile(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return "";
            try
            {
                string strMask = "";
                strMask = strMask.PadRight(phone.Length - 3, '*');

                string newPhone = string.Format("{0}{1}", strMask, phone.Substring(phone.Length - 3));
                return newPhone;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static string ConvertUnicodeToString(string unicodeString)
        {
            // Create two different encodings.
            Encoding utf8Str = Encoding.UTF8;
            Encoding unicode = Encoding.Unicode;

            // Convert the string into a byte array.
            byte[] unicodeBytes = unicode.GetBytes(unicodeString);

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(unicode, utf8Str, unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            char[] asciiChars = new char[utf8Str.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            utf8Str.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);

            return asciiString;
        }
    }

    public static class StringExt
    {
        public static string Replace(this string s, string oldValue, string newValue, StringComparison comparisonType)
        {
            if (s == null)
                return null;

            if (String.IsNullOrEmpty(oldValue))
                return s;

            StringBuilder result = new StringBuilder(Math.Min(4096, s.Length));
            int pos = 0;

            while (true)
            {
                int i = s.IndexOf(oldValue, pos, comparisonType);
                if (i < 0)
                    break;

                result.Append(s, pos, i - pos);
                result.Append(newValue);

                pos = i + oldValue.Length;
            }
            result.Append(s, pos, s.Length - pos);

            return result.ToString();
        }
    }
 

}