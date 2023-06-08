using System;
using System.IO;
using System.Security.Cryptography;

namespace Sechat.Service.Services;

public class AesEncryptor : IEncryptor
{
    public string EncryptString(string key, string plainText)
    {
        var iv = new byte[16];
        byte[] array;

        using (var aes = Aes.Create())
        {
            aes.Key = Convert.FromBase64String(key);
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

    public string DecryptString(string key, string cipherText)
    {
        var iv = new byte[16];
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public string GenerateKey()
    {
        using var aesAlgorithm = Aes.Create();
        aesAlgorithm.KeySize = 256;
        aesAlgorithm.GenerateKey();

        return Convert.ToBase64String(aesAlgorithm.Key);
    }

    public byte[] Encrypt(byte[] data, string password, byte[] salt, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var rfc = new Rfc2898DeriveBytes(password, salt, 2000, HashAlgorithmName.SHA512);
        aes.Key = rfc.GetBytes(256 / 8);
        aes.IV = iv;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);

        using (var bw = new BinaryWriter(cs))
        {
            bw.Write(data);
        }

        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] data, string password, byte[] salt, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var rfc = new Rfc2898DeriveBytes(password, salt, 2000, HashAlgorithmName.SHA512);
        aes.Key = rfc.GetBytes(256 / 8);
        aes.IV = iv;

        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);

        using var br = new BinaryReader(cs);
        return br.ReadBytes(data.Length);
    }
}

