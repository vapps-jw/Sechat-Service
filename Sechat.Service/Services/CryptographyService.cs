using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Sechat.Service.Services;

public class CryptographyService
{
    private const char _segmentDelimiter = ':';
    private const string _rsaPrivateKey = "RSA PRIVATE KEY";
    private const string _subjectPublicKeyInfo = "PUBLIC KEY";

    public record Keys(string Public, string Private);

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
        using var rsa = new RSACryptoServiceProvider();

        rsa.ImportFromPem(publicKey.ToCharArray());

        var byteData = Encoding.UTF8.GetBytes(plainText);
        var encryptedData = rsa.Encrypt(byteData, false);
        return Convert.ToBase64String(encryptedData);
    }

    public string AsymmetricDecrypt(string cipherText, string privateKey)
    {
        using var rsa = new RSACryptoServiceProvider();
        rsa.ImportFromPem(privateKey.ToCharArray());

        var cipherDataAsByte = Convert.FromBase64String(cipherText);
        var encryptedData = rsa.Decrypt(cipherDataAsByte, false);
        return Encoding.UTF8.GetString(encryptedData);
    }

    public Keys GenerateAsymmetricKeys(int keySize)
    {
        using var rsa = new RSACryptoServiceProvider(keySize);
        return new Keys(rsa.ExportRSAPublicKeyPem(), rsa.ExportRSAPrivateKeyPem());
    }

    public string GenerateKey()
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var interations = new Random().Next(10000, 30000);

        using var crypto = Aes.Create();
        crypto.GenerateKey();
        using var keyGenerator = new Rfc2898DeriveBytes(Convert.ToBase64String(crypto.Key), salt, interations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(keyGenerator.GetBytes(32));
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
