using Sechat.Data.DataServices;
using System.Text;

namespace Sechat.Tests;

public class AesTests
{
    private const string _testString = "testString";

    [Fact]
    public void GenerateAesKeyTest()
    {
        var encryptor = new DataEncryptor();
        var key = encryptor.GenerateKey();
        Assert.True(!string.IsNullOrEmpty(key));
    }

    [Fact]
    public void EncryptStringTest()
    {
        var encryptor = new DataEncryptor();
        var key = encryptor.GenerateKey();
        var encryptedString = encryptor.EncryptString(key, _testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));
    }

    [Fact]
    public void DecryptStringTest()
    {
        var encryptor = new DataEncryptor();
        var key = encryptor.GenerateKey();
        var encryptedString = encryptor.EncryptString(key, _testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));

        var decryptedString = encryptor.DecryptString(key, encryptedString);

        Assert.Equal(_testString, decryptedString);
    }

    [Fact]
    public void EncryptionWithPasswordTest()
    {
        var encryptor = new DataEncryptor();
        var data = "test string to encrypt";
        var saltString = "sadasdasd";
        var iv = new byte[16];
        var password = "pass123";

        var encrypted = encryptor.Encrypt(Encoding.UTF8.GetBytes(data), password, Encoding.UTF8.GetBytes(saltString), iv);
        var decrypted = Encoding.UTF8.GetString(encryptor.Decrypt(encrypted, password, Encoding.UTF8.GetBytes(saltString), iv));

        Assert.Equal(data, decrypted);
    }
}