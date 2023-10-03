namespace NetCore.Utils.Interfaces
{
    public interface ISecurity
    {
        string MD5Encrypt(string plainText);

        string SHA256Hash(string plainText);

        string AESEncryptString(string text, string keyString);

        string AESDecryptString(string cipherText, string keyString);

        string AESEncryptString(string text);

        string AESDecryptString(string cipherText);

        string AESEncrypt(string plainText, string phrase = "encrypt");

        string AESDecrypt(string plainText, string phrase = "encrypt");
    }
}