namespace Sechat.Tests;

public class CryptographyTests
{

    [Fact]
    public void AsymmetricEncryptionTest()
    {
        var service = new Service.Services.CryptographyService();
        var keys = service.GenerateAsymmetricKeys(4096);
        var secretString = "test-secret";

        var encrypted = service.AsymmetricEncrypt(secretString, keys.Public);
        var decrypted = service.AsymmetricDecrypt(encrypted, keys.Private);

        Assert.Equal(secretString, decrypted);
    }

    [Fact]
    public void PasswordHasherTest()
    {
        var service = new Service.Services.CryptographyService();
        var secret = "test-password";

        var hash = service.Hash(secret);
        var res = service.Verify(secret, hash);

        Assert.True(res);
    }

    [Fact]
    public void GenerateRandomStringTest()
    {
        var service = new Service.Services.CryptographyService();
        var text = service.GenerateKey();

        Assert.NotNull(text);
    }

    [Fact]
    public void DirectMessageTest()
    {
        var service = new Service.Services.CryptographyService();
        var secretString = "test-message";

        var key = service.GenerateKey();

        var encrypted = service.Encrypt(secretString, key);
        _ = service.Decrypt(encrypted, key, out var decrypted);

        Assert.Equal(secretString, decrypted);
    }
}