using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Sechat.Service.Services;

public class CryptographyService
{
    private const char _segmentDelimiter = ':';

    public record Keys(string Public, string Private);

    public string Encrypt(string plainText, byte[] encryptionKeyBytes, byte[] iv)
    {
        byte[] array;
        using (var aes = Aes.Create())
        {
            aes.Key = encryptionKeyBytes;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            array = memoryStream.ToArray();
        }

        return Convert.ToBase64String(array);
    }

    public string Decrypt(string cipherText, byte[] encryptionKeyBytes, byte[] iv)
    {
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = encryptionKeyBytes;
        aes.IV = iv;
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new(buffer);
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public string Encrypt(string plainText, string password)
    {
        byte[] encryptedData;
        var salt = RandomNumberGenerator.GetBytes(16);
        var iv = RandomNumberGenerator.GetBytes(16);
        var interations = new Random().Next(10000, 30000);

        var key = GenerateKey(password, salt, interations);

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            encryptedData = memoryStream.ToArray();
        }

        return string.Join(
            _segmentDelimiter,
            Convert.ToHexString(encryptedData),
            Convert.ToHexString(iv),
            Convert.ToHexString(salt),
            interations
        );
    }

    public bool Decrypt(string cipherText, string password, out string decryptedString)
    {
        try
        {
            var segments = cipherText.Split(_segmentDelimiter);
            var encryptedData = Convert.FromHexString(segments[0]);
            var iv = Convert.FromHexString(segments[1]);
            var salt = Convert.FromHexString(segments[2]);
            var iterations = int.Parse(segments[3]);

            var key = GenerateKey(password, salt, iterations);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(encryptedData);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            decryptedString = streamReader.ReadToEnd();
            return true;
        }
        catch (Exception)
        {
            decryptedString = "Decryption error, check your Key";
            return false;
        }
    }

    public string AsymmetricEncrypt(string plainText, string publicKey)
    {
        using var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
        rsaCryptoServiceProvider.FromXmlString(publicKey);

        var byteData = Encoding.UTF8.GetBytes(plainText);
        var encryptedData = rsaCryptoServiceProvider.Encrypt(byteData, false);
        return Convert.ToBase64String(encryptedData);
    }
    public string AsymmetricDecrypt(string cipherText, string privateKey)
    {
        using var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
        rsaCryptoServiceProvider.FromXmlString(privateKey);

        var cipherDataAsByte = Convert.FromBase64String(cipherText);
        var encryptedData = rsaCryptoServiceProvider.Decrypt(cipherDataAsByte, false);
        return Encoding.UTF8.GetString(encryptedData);
    }

    public Keys GenetateAsymmetricKeys(int keySize)
    {
        var rsa = new RSACryptoServiceProvider(keySize);
        return new Keys(rsa.ToXmlString(false), rsa.ToXmlString(true));
    }

    public byte[] GenerateKey(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var interations = new Random().Next(10000, 30000);

        using var keyGenerator = new Rfc2898DeriveBytes(password, salt, interations, HashAlgorithmName.SHA256);
        return keyGenerator.GetBytes(32);
    }

    public byte[] GenerateKey(string password, string salt, int interations)
    {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        using var keyGenerator = new Rfc2898DeriveBytes(password, saltBytes, interations, HashAlgorithmName.SHA256);

        return keyGenerator.GetBytes(32);
    }

    public byte[] GenerateKey(string password, byte[] salt, int interations)
    {
        using var keyGenerator = new Rfc2898DeriveBytes(password, salt, interations, HashAlgorithmName.SHA256);
        return keyGenerator.GetBytes(32);
    }

    public string Hash(string input, int interations = 11234, int keySize = 32)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            interations,
            HashAlgorithmName.SHA512,
            keySize
        );
        return string.Join(
            _segmentDelimiter,
            Convert.ToHexString(hash),
            Convert.ToHexString(salt),
            interations,
            HashAlgorithmName.SHA512
        );
    }

    public bool Verify(string input, string hashString)
    {
        var segments = hashString.Split(_segmentDelimiter);
        var hash = Convert.FromHexString(segments[0]);
        var salt = Convert.FromHexString(segments[1]);
        var iterations = int.Parse(segments[2]);
        var algorithm = new HashAlgorithmName(segments[3]);
        var inputHash = Rfc2898DeriveBytes.Pbkdf2(
            input,
            salt,
            iterations,
            algorithm,
            hash.Length
        );
        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
