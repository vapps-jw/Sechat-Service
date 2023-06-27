namespace Sechat.Tests;

public class CryptographyTests
{
    [Fact]
    public void EncryptionWithPasswordTest()
    {
        var service = new Service.Services.CryptographyService();
        var secretString = "test-secret";

        var key = service.GenerateKey($"{Guid.NewGuid()}test123", "asasas12", 500);

        var iv = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80, 80, 80, 80, 80, 80, 80, 80, 80 };

        var encrypted = service.Encrypt(secretString, key, iv);
        var decrypted = service.Decrypt(encrypted, key, iv);

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
}