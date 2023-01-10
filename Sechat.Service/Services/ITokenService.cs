namespace Sechat.Service.Services;

public interface ITokenService
{
    string GenerateSecretKey(int count = 64);
    string GenerateToken(string userName, string secretKey);
    bool ValidateToken(string token, string secretKey);
}