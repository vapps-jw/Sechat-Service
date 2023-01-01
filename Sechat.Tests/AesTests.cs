using Sechat.Service.Utilities;

namespace Sechat.Tests;

public class AesTests
{
    private const string testString = "testString";

    [Fact]
    public void GenerateAesKeyTest()
    {
        var key = Hasher.GenerateKey();
        Assert.True(!string.IsNullOrEmpty(key));
    }

    [Fact]
    public void EncryptStringTest()
    {
        var key = Hasher.GenerateKey();
        var encryptedString = Hasher.EncryptString(Convert.FromBase64String(key), testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));
    }

    [Fact]
    public void DecryptStringTest()
    {
        var key = Hasher.GenerateKey();
        var encryptedString = Hasher.EncryptString(Convert.FromBase64String(key), testString);

        Assert.True(!string.IsNullOrEmpty(encryptedString));

        var decryptedString = Hasher.DecryptString(Convert.FromBase64String(key), encryptedString);

        Assert.Equal(testString, decryptedString);
    }
}