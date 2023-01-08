namespace Sechat.Service.Services;

public interface IServerEncryptor
{
    string DecryptString(string key, string cipherText);
    string EncryptString(string key, string plainText);
    string GenerateKey();
}