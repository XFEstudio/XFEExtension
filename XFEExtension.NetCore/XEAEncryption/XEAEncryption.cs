namespace XFEExtension.NetCore.XEAEncryption;

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
        var randomSeed = StringExtension.StringExtension.GenerateRandomString(5);
        var encryptedRandomSeed = string.Empty;
        var encrypted = string.Empty;
        var middleKey = string.Empty;
        var middleChar = ' ';
        for (var i = 0; i < secretKey.Length / 2; i++)
        {
            middleChar ^= secretKey[i];
            middleKey += (char)(secretKey[i] ^ secretKey[secretKey.Length - i - 1]);
        }//计算中间密钥，防止出现秘钥长度大于待加密文本长度的情况
        for (var i = 0; i < randomSeed.Length; i++)
        {
            var currentChar = randomSeed[i];
            var secretKeyIndex = i % secretKey.Length;
            var middleKeyIndex = i % middleKey.Length;
            var secretKeyChar = secretKey[secretKeyIndex];
            var middleKeyChar = middleKey[middleKeyIndex];
            var encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ middleChar);
            encryptedRandomSeed += encryptedChar;
        }//对随机种子进行加密
        for (var i = 0; i < waitEncryptString.Length; i++)
        {
            var currentChar = waitEncryptString[i];
            var secretKeyIndex = i % secretKey.Length;
            var middleKeyIndex = i % middleKey.Length;
            var randomSeedIndex = i % randomSeed.Length;
            var secretKeyChar = secretKey[secretKeyIndex];
            var middleKeyChar = middleKey[middleKeyIndex];
            var randomSeedChar = randomSeed[randomSeedIndex];
            var encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ randomSeedChar ^ middleChar);
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
        var decrypted = string.Empty;
        var randomSeed = encryptedString[..5];
        var decryptedRandomSeed = string.Empty;
        var encrypted = encryptedString[5..];
        var middleKey = string.Empty;
        var middleChar = ' ';
        for (var i = 0; i < secretKey.Length / 2; i++)
        {
            middleChar ^= secretKey[i];
            middleKey += (char)(secretKey[i] ^ secretKey[secretKey.Length - i - 1]);
        }//计算中间密钥
        for (var i = 0; i < randomSeed.Length; i++)
        {
            var currentChar = randomSeed[i];
            var secretKeyIndex = i % secretKey.Length;
            var middleKeyIndex = i % middleKey.Length;
            var secretKeyChar = secretKey[secretKeyIndex];
            var middleKeyChar = middleKey[middleKeyIndex];
            var encryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ middleChar);
            decryptedRandomSeed += encryptedChar;
        }//对随机种子进行解密
        for (var i = 0; i < encrypted.Length; i++)
        {
            var currentChar = encrypted[i];
            var secretKeyIndex = i % secretKey.Length;
            var middleKeyIndex = i % middleKey.Length;
            var randomSeedIndex = i % decryptedRandomSeed.Length;
            var secretKeyChar = secretKey[secretKeyIndex];
            var middleKeyChar = middleKey[middleKeyIndex];
            var randomSeedChar = decryptedRandomSeed[randomSeedIndex];
            var decryptedChar = (char)(currentChar ^ secretKeyChar ^ middleKeyChar ^ randomSeedChar ^ middleChar);
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
