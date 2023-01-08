using Sechat.Service.Services;

namespace Sechat.Tests;

public class AesTests
{
    private const string testString = "testString";

    [Fact]
    public void GenerateAesKeyTest()
    {
        var encryptor = new AesEncryptor();
        var key = encryptor.GenerateKey();
        Assert.True(!string.IsNullOrEmpty(key));
    }

    [Fact]
    public void EncryptStringTest()
    {
        var encryptor = new AesEncryptor();
        var key = encryptor.GenerateKey();
        var encryptedString = encryptor.EncryptString(key, testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));
    }

    [Fact]
    public void DecryptStringTest()
    {
        var encryptor = new AesEncryptor();
        var key = encryptor.GenerateKey();
        var encryptedString = encryptor.EncryptString(key, testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));

        var decryptedString = encryptor.DecryptString(key, encryptedString);

        Assert.Equal(testString, decryptedString);
    }
}