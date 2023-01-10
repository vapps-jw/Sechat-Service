namespace Sechat.Service.Services;

public interface IEncryptor
{
    string DecryptString(string key, string cipherText);
    string EncryptString(string key, string plainText);
    string GenerateKey();
}