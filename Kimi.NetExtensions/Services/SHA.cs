using System.Security.Cryptography;
using System.Text;

namespace Kimi.NetExtensions.Services;

public static class SHA
{
    public static string SHAmd5Encrypt(string normalTxt)
    {
        byte[] bytes = Encoding.Default.GetBytes(normalTxt);
        MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
        byte[] buffer = mD5CryptoServiceProvider.ComputeHash(bytes);
        return Base64To16(buffer);
    }

    public static string SHA1Encrypt(string normalTxt)
    {
        byte[] bytes = Encoding.Default.GetBytes(normalTxt);
        SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
        byte[] buffer = sHA1CryptoServiceProvider.ComputeHash(bytes);
        return Base64To16(buffer);
    }

    public static string SHA256Encrypt(string normalTxt)
    {
        byte[] bytes = Encoding.Default.GetBytes(normalTxt);
        SHA256CryptoServiceProvider sHA256CryptoServiceProvider = new SHA256CryptoServiceProvider();
        byte[] buffer = sHA256CryptoServiceProvider.ComputeHash(bytes);
        return Base64To16(buffer);
    }

    public static string SHA384Encrypt(string normalTxt)
    {
        byte[] bytes = Encoding.Default.GetBytes(normalTxt);
        SHA384CryptoServiceProvider sHA384CryptoServiceProvider = new SHA384CryptoServiceProvider();
        byte[] buffer = sHA384CryptoServiceProvider.ComputeHash(bytes);
        return Base64To16(buffer);
    }

    public static string SHA512Encrypt(string normalTxt)
    {
        byte[] bytes = Encoding.Default.GetBytes(normalTxt);
        SHA512CryptoServiceProvider sHA512CryptoServiceProvider = new SHA512CryptoServiceProvider();
        byte[] buffer = sHA512CryptoServiceProvider.ComputeHash(bytes);
        return Base64To16(buffer);
    }

    private static string Base64To16(byte[] buffer)
    {
        string text = string.Empty;
        for (int i = 0; i < buffer.Length; i++)
        {
            text += buffer[i].ToString("x2");
        }

        return text;
    }
}
