namespace Sechat.Service.Services;

public interface IEncryptor
{
    string DecryptString(string key, string cipherText);
    string EncryptString(string key, string plainText);
    byte[] Encrypt(byte[] data, string password, byte[] salt, byte[] iv);
    byte[] Decrypt(byte[] data, string password, byte[] salt, byte[] iv);
    string GenerateKey();
}