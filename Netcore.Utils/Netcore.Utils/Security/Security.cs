using NetCore.Utils.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NetCore.Utils.Security
{
    public class Security : ISecurity
    {
        public string MD5Encrypt(string plainText)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                // Get the hashed string.
                var hash = BitConverter.ToString(result).Replace("-", "").ToLower();

                // Print the string.
                return hash;
            }
        }

        public string SHA256Hash(string plainText)
        {
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainText));

                // Get the hashed string.
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // Print the string.
                return hash;
            }
        }

        public string AESEncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public string AESDecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public string AESEncryptString(string text)
        {
            return AESEncryptString(text, "E546C8DF278CD5931069B522E695D4F2");
        }

        public string AESDecryptString(string cipherText)
        {
            return AESDecryptString(cipherText, "E546C8DF278CD5931069B522E695D4F2");
        }

        public string AESEncrypt(string plainText, string phrase = "encrypt")
        {
            return new RijndaelEnhanced(phrase, MD5Encrypt(phrase).Substring(0, 16)).Encrypt(plainText);
        }

        public string AESDecrypt(string plainText, string phrase = "encrypt")
        {
            return new RijndaelEnhanced(phrase, MD5Encrypt(phrase).Substring(0, 16)).Decrypt(plainText);
        }
    }
}