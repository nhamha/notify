using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetCore.Utils.Security
{
    public class DesSecurity
    {
        private const string salt = "hywebpg5";
        private readonly ILogger<DesSecurity> _logger;

        public DesSecurity(ILogger<DesSecurity> logger)
        {
            _logger = logger;
        }

        public string Byte2Hex(byte[] b)
        {
            string str = "";
            string str1 = "";
            for (int i = 0; i < (int)b.Length; i++)
            {
                str1 = Convert.ToString(b[i] & 255, 16);
                str = (str1.Length != 1 ? string.Concat(str, str1) : string.Concat(str, "0", str1));
                if (i < (int)b.Length - 1)
                {
                    str = str ?? "";
                }
            }
            return str.ToUpper();
        }

        public string Byte2Hex_24(byte[] b)
        {
            string str = "";
            string str1 = "";
            for (int i = (int)b.Length - 32; i < (int)b.Length - 8; i++)
            {
                str1 = Convert.ToString(b[i] & 255, 16);
                str = (str1.Length != 1 ? string.Concat(str, str1) : string.Concat(str, "0", str1));
                if (i < (int)b.Length - 1)
                {
                    str = str ?? "";
                }
            }
            return str.ToUpper();
        }

        public string Byte2Hex_24_en(byte[] b)
        {
            string str = "";
            string str1 = "";
            for (int i = (int)b.Length - 24; i < (int)b.Length; i++)
            {
                str1 = Convert.ToString(b[i] & 255, 16);
                str = (str1.Length != 1 ? string.Concat(str, str1) : string.Concat(str, "0", str1));
                if (i < (int)b.Length - 1)
                {
                    str = str ?? "";
                }
            }
            return str.ToUpper();
        }

        public string Byte2Hex1(byte[] b)
        {
            string str = "";
            string str1 = "";
            for (int i = (int)b.Length - 16; i < (int)b.Length - 8; i++)
            {
                str1 = Convert.ToString(b[i] & 255, 16);
                str = (str1.Length != 1 ? string.Concat(str, str1) : string.Concat(str, "0", str1));
                if (i < (int)b.Length - 1)
                {
                    str = str ?? "";
                }
            }
            return str.ToUpper();
        }

        public string Decrypt(string msg, string key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("hywebpg5");
            byte[] numArray = this.Hex2Byte(msg);
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] numArray1 = this.Decrypt(numArray, bytes1, bytes);
            return Encoding.UTF8.GetString(numArray1);
        }

        public byte[] Decrypt(byte[] Data, byte[] Key, byte[] IV)
        {
            byte[] array;
            try
            {
                DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider()
                {
                    Key = Key,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    IV = IV
                };
                MemoryStream memoryStream = new MemoryStream(Data);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Read);
                byte[] numArray = new byte[(int)Data.Length];
                cryptoStream.Read(numArray, 0, (int)numArray.Length);
                List<byte> nums = new List<byte>();
                byte[] numArray1 = numArray;
                for (int i = 0; i < (int)numArray1.Length; i++)
                {
                    byte num = numArray1[i];
                    if (num != 0)
                    {
                        nums.Add(num);
                    }
                }
                array = nums.ToArray();
            }
            catch (CryptographicException cryptographicException)
            {
                array = null;
                _logger.LogError("error: ", cryptographicException);
            }
            catch (Exception exception)
            {
                array = null;
                _logger.LogError("error: ", exception);
            }
            return array;
        }

        public string Des3Decrypt(string data, string key)
        {
            ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
            byte[] numArray = null;
            byte[] numArray1 = this.Hex2Byte(data);
            byte[] bytes = aSCIIEncoding.GetBytes(key);
            byte[] numArray2 = DesSecurity.RemovePadding(this.Des3Decrypt(numArray1, bytes, numArray));
            return aSCIIEncoding.GetString(numArray2);
        }

        public byte[] Des3Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] numArray;
            try
            {
                TripleDES tripleDE = TripleDES.Create();
                if (iv != null)
                {
                    tripleDE.IV = iv;
                }
                tripleDE.Key = key;
                tripleDE.Mode = CipherMode.ECB;
                tripleDE.Padding = PaddingMode.None;
                byte[] numArray1 = new byte[0];
                numArray = tripleDE.CreateDecryptor().TransformFinalBlock(data, 0, (int)data.Length);
            }
            catch (CryptographicException cryptographicException)
            {
                numArray = null;
                _logger.LogError("error: ", cryptographicException);
            }
            catch (Exception exception)
            {
                numArray = null;
                _logger.LogError("error: ", exception);
            }
            return numArray;
        }

        public string Des3Encrypt(string data, string key)
        {
            byte[] numArray = null;
            byte[] bytes = Encoding.UTF8.GetBytes(DesSecurity.Padding(data));
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] numArray1 = this.Des3Encrypt(bytes, bytes1, numArray);
            return this.Byte2Hex(numArray1);
        }

        public byte[] Des3Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            byte[] numArray;
            byte[] numArray1 = new byte[0];
            try
            {
                TripleDES tripleDE = TripleDES.Create();
                if (iv != null)
                {
                    tripleDE.IV = iv;
                }
                tripleDE.Key = key;
                tripleDE.Mode = CipherMode.ECB;
                tripleDE.Padding = PaddingMode.None;
                numArray = tripleDE.CreateEncryptor().TransformFinalBlock(data, 0, (int)data.Length);
                return numArray;
            }
            catch (CryptographicException cryptographicException)
            {
                _logger.LogError("error: ", cryptographicException);
            }
            numArray = numArray1;
            return numArray;
        }

        public string DESEDEMAC_24(string msg, string key)
        {
            int length = msg.Length % 8;
            if (length != 0)
            {
                for (int i = 0; i < 8 - length; i++)
                {
                    msg = string.Concat(msg, "0");
                }
            }
            ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
            byte[] bytes = aSCIIEncoding.GetBytes("hywebpg5");
            byte[] numArray = aSCIIEncoding.GetBytes(msg);
            byte[] bytes1 = aSCIIEncoding.GetBytes(key);
            byte[] numArray1 = this.Des3Encrypt(numArray, bytes1, bytes);
            return this.Byte2Hex1(numArray1);
        }

        public string DESMAC(string msg, string key)
        {
            byte[] numArray = null;
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] numArray1 = this.Padding((new SHA1CryptoServiceProvider()).ComputeHash(bytes));
            byte[] numArray2 = this.Des3Encrypt(numArray1, bytes1, numArray);
            return this.Byte2Hex(numArray2);
        }

        public string Encrypt(string msg, string key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("hywebpg5");
            byte[] numArray = Encoding.UTF8.GetBytes(msg);
            byte[] bytes1 = Encoding.UTF8.GetBytes(key);
            byte[] numArray1 = this.Encrypt(numArray, bytes1, bytes);
            return this.Byte2Hex(numArray1);
        }

        public byte[] Encrypt(byte[] Data, byte[] Key, byte[] IV)
        {
            byte[] numArray;
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider()
                {
                    Key = Key,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    IV = IV
                };
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(), CryptoStreamMode.Write);
                byte[] data = Data;
                cryptoStream.Write(data, 0, (int)data.Length);
                cryptoStream.FlushFinalBlock();
                byte[] array = memoryStream.ToArray();
                cryptoStream.Close();
                memoryStream.Close();
                numArray = array;
            }
            catch (CryptographicException cryptographicException)
            {
                numArray = null;
                _logger.LogError("error: ", cryptographicException);
            }
            return numArray;
        }

        public byte[] Hex2Byte(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException();
            }
            char[] charArray = hex.ToCharArray();
            byte[] num = new byte[hex.Length / 2];
            int num1 = 0;
            int num2 = 0;
            int length = hex.Length;
            while (num1 < length)
            {
                int num3 = num1;
                num1 = num3 + 1;
                int num4 = Convert.ToInt16(string.Concat(charArray[num3], charArray[num1]), 16) & 255;
                num[num2] = Convert.ToByte(num4);
                num1++;
                num2++;
            }
            return num;
        }

        private byte[] Padding(byte[] hashedBytes)
        {
            byte[] numArray;
            try
            {
                byte[] numArray1 = hashedBytes;
                int length = 8 - (int)numArray1.Length % 8;
                byte[] numArray2 = new byte[(int)numArray1.Length + length];
                Array.Copy(numArray1, numArray2, (int)numArray1.Length);
                for (int i = (int)numArray1.Length; i < (int)numArray2.Length; i++)
                {
                    numArray2[i] = 0;
                }
                numArray = numArray2;
                return numArray;
            }
            catch (Exception ex)
            {
                _logger.LogError("error: ", ex);
            }
            numArray = null;
            return numArray;
        }

        public static string Padding(string str)
        {
            string str1;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                int length = 8 - (int)bytes.Length % 8;
                byte[] numArray = new byte[(int)bytes.Length + length];
                Array.Copy(bytes, numArray, (int)bytes.Length);
                for (int i = (int)bytes.Length; i < (int)numArray.Length; i++)
                {
                    numArray[i] = 0;
                }
                str1 = Encoding.UTF8.GetString(numArray);
                return str1;
            }
            catch (Exception ex)
            {
                //_logger.LogError("error: ", ex);
            }
            str1 = null;
            return str1;
        }

        public static byte[] RemovePadding(byte[] oldByteArray)
        {
            int length = 0;
            int num = (int)oldByteArray.Length;
            while (num >= 0)
            {
                if (oldByteArray[num - 1] == 0)
                {
                    num--;
                }
                else
                {
                    length = (int)oldByteArray.Length - num;
                    break;
                }
            }
            byte[] numArray = new byte[(int)oldByteArray.Length - length];
            Array.Copy(oldByteArray, numArray, (int)numArray.Length);
            return numArray;
        }
    }
}