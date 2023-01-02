using Sechat.Service.Utilities;

namespace Sechat.Tests;

public class AesTests
{
    private const string testString = "testString";

    [Fact]
    public void GenerateAesKeyTest()
    {
        var key = Crypto.GenerateKey();
        Assert.True(!string.IsNullOrEmpty(key));
    }

    [Fact]
    public void EncryptStringTest()
    {
        var key = Crypto.GenerateKey();
        var encryptedString = Crypto.EncryptString(Convert.FromBase64String(key), testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));
    }

    [Fact]
    public void DecryptStringTest()
    {
        var key = Crypto.GenerateKey();
        var encryptedString = Crypto.EncryptString(Convert.FromBase64String(key), testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));

        var decryptedString = Crypto.DecryptString(Convert.FromBase64String(key), encryptedString);

        Assert.Equal(testString, decryptedString);
    }
}