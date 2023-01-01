using System;
using System.IO;
using System.Security.Cryptography;

namespace Sechat.Service.Utilities;

public static class Hasher
{
    public static string EncryptString(byte[] key, string plainText)
    {
        var iv = new byte[16];
        byte[] array;

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public static string DecryptString(byte[] key, string cipherText)
    {
        var iv = new byte[16];
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public static string GenerateKey()
    {
        using var aesAlgorithm = Aes.Create();
        aesAlgorithm.KeySize = 256;
        aesAlgorithm.GenerateKey();
        var keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);

        var res = Convert.FromBase64String(keyBase64);

        return keyBase64;
    }
}

