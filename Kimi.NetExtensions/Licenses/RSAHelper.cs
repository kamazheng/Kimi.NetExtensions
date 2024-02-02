using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public class RSAHelper
{
    public static string PublicKey { get; set; } = "<RSAKeyValue><Modulus>mlFF55YRO8o/IYyfU8t9m53JkFR5UKgek5CuL5WZ9tcup2A4m+VokFiWmoiBrt9u/o/FIcmyVstcWB0T+TMX8zVIijVKzf4M9PlOOKe7dXdqOGujhufzu34Mj5MC1B2OYcygHuIrD7fyAw2B/H0hPEi1cJ91RP8akQ2bV7i95m0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    /// <summary>
    /// 用公钥给数据进行RSA解密 
    /// </summary>
    /// <param name="xmlPublicKey"> 公钥(XML格式字符串) </param>
    /// <param name="strDecryptString"> 要解密数据 </param>
    /// <returns> 解密后的数据 </returns>
    public static string PublicKeyDecrypt(string xmlPublicKey, string strDecryptString)
    {
        //加载公钥
        RSACryptoServiceProvider publicRsa = new RSACryptoServiceProvider();
        publicRsa.FromXmlString(xmlPublicKey);
        RSAParameters rp = publicRsa.ExportParameters(false);

        //转换密钥
        AsymmetricKeyParameter pbk = DotNetUtilities.GetRsaPublicKey(rp);

        IBufferedCipher c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
        //第一个参数为true表示加密，为false表示解密；第二个参数表示密钥
        c.Init(false, pbk);
        byte[] outBytes = null!;
        byte[] dataToDecrypt = Convert.FromBase64String(strDecryptString);
        #region 分段解密
        int keySize = publicRsa.KeySize / 8;
        byte[] buffer = new byte[keySize];

        using (MemoryStream input = new MemoryStream(dataToDecrypt))
        using (MemoryStream output = new MemoryStream())
        {
            while (true)
            {
                int readLine = input.Read(buffer, 0, keySize);
                if (readLine <= 0)
                {
                    break;
                }
                byte[] temp = new byte[readLine];
                Array.Copy(buffer, 0, temp, 0, readLine);
                byte[] decrypt = c.DoFinal(temp);
                output.Write(decrypt, 0, decrypt.Length);
            }
            outBytes = output.ToArray();
        }
        #endregion
        //byte[] outBytes = c.DoFinal(DataToDecrypt);//解密

        string strDec = Encoding.UTF8.GetString(outBytes);
        return strDec;
    }
}
