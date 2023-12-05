namespace XFE各类拓展.NetCore.XEAEncryption;

/// <summary>
/// XEA加密算法
/// </summary>
public static class XEAEncryption
{
    /// <summary>
    /// 进行XEA加密
    /// </summary>
    /// <param name="waitEncryptString">待加密文本</param>
    /// <param name="secretKey">加密秘钥</param>
    /// <returns>加密文本</returns>
    public static string XEAEncrypt(string waitEncryptString, string secretKey)
    {
        string randomSeed = StringExtension.StringExtension.GenerateRandomString(5);
        string encryptedRandomSeed = string.Empty;
        string encrypted = string.Empty;
        string middleKey = string.Empty;
        char middleChar = ' ';
        for (int i = 0; i < secretKey.Length / 2; i++)
        {
            middleChar ^= secretKey[i];
            middleKey += (char)(secretKey[i] ^ secretKey[secretKey.Length - i - 1]);
        }//计算中间密钥，防止出现秘钥长度大于待加密文本长度的情况
        for (int i = 0; i < randomSeed.Length; i++)
        {
            char currentChar = randomSeed[i];
            int secretKeyIndex = i % secretKey.Length;
            int middleKeyIndex = i % middleKey.Length;
            char secretKeyChar = secretKey[secretKeyIndex];
            char middleKeyChar = middleKey[middleKeyIndex];
            char encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ middleChar);
            encryptedRandomSeed += encryptedChar;
        }//对随机种子进行加密
        for (int i = 0; i < waitEncryptString.Length; i++)
        {
            char currentChar = waitEncryptString[i];
            int secretKeyIndex = i % secretKey.Length;
            int middleKeyIndex = i % middleKey.Length;
            int randomSeedIndex = i % randomSeed.Length;
            char secretKeyChar = secretKey[secretKeyIndex];
            char middleKeyChar = middleKey[middleKeyIndex];
            char randomSeedChar = randomSeed[randomSeedIndex];
            char encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ randomSeedChar ^ middleChar);
            encrypted += encryptedChar;
        }//XOR加密

        return encryptedRandomSeed + encrypted;
    }
    /// <summary>
    /// 进行XEA解密
    /// </summary>
    /// <param name="encryptedString">加密文本</param>
    /// <param name="secretKey">解密秘钥</param>
    /// <returns>解密文本</returns>
    public static string XEADecrypt(string encryptedString, string secretKey)
    {
        string decrypted = string.Empty;
        string randomSeed = encryptedString[..5];
        string decryptedRandomSeed = string.Empty;
        string encrypted = encryptedString[5..];
        string middleKey = string.Empty;
        char middleChar = ' ';
        for (int i = 0; i < secretKey.Length / 2; i++)
        {
            middleChar ^= secretKey[i];
            middleKey += (char)(secretKey[i] ^ secretKey[secretKey.Length - i - 1]);
        }//计算中间密钥
        for (int i = 0; i < randomSeed.Length; i++)
        {
            char currentChar = randomSeed[i];
            int secretKeyIndex = i % secretKey.Length;
            int middleKeyIndex = i % middleKey.Length;
            char secretKeyChar = secretKey[secretKeyIndex];
            char middleKeyChar = middleKey[middleKeyIndex];
            char encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ middleChar);
            decryptedRandomSeed += encryptedChar;
        }//对随机种子进行解密
        for (int i = 0; i < encrypted.Length; i++)
        {
            char currentChar = encrypted[i];
            int secretKeyIndex = i % secretKey.Length;
            int middleKeyIndex = i % middleKey.Length;
            int randomSeedIndex = i % decryptedRandomSeed.Length;
            char secretKeyChar = secretKey[secretKeyIndex];
            char middleKeyChar = middleKey[middleKeyIndex];
            char randomSeedChar = decryptedRandomSeed[randomSeedIndex];
            char decryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ randomSeedChar ^ middleChar);
            decrypted += decryptedChar;
        }//XOR解密
        return decrypted;
    }
}
/// <summary>
/// XEA加密算法的拓展
/// </summary>
public static class XEAEncryptionExtension
{
    /// <summary>
    /// 进行XEA加密
    /// </summary>
    /// <param name="waitEncryptString">待加密文本</param>
    /// <param name="secretKey">加密秘钥</param>
    /// <returns>加密文本</returns>
    public static string XEAEncrypt(this string waitEncryptString, string secretKey)
    {
        return XEAEncryption.XEAEncrypt(waitEncryptString, secretKey);
    }
    /// <summary>
    /// 进行XEA解密
    /// </summary>
    /// <param name="encryptedString">加密文本</param>
    /// <param name="key">解密秘钥</param>
    /// <returns>解密文本</returns>
    public static string XEADecrypt(this string encryptedString, string key)
    {
        return XEAEncryption.XEADecrypt(encryptedString, key);
    }
}
