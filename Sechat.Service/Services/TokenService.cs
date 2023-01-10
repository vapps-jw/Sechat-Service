using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sechat.Service.Settings;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sechat.Service.Services;

public class TokenService : ITokenService
{
    private readonly IOptionsMonitor<TokenSettings> _optionsMonitor;

    public TokenService(IOptionsMonitor<TokenSettings> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public string GenerateSecretKey(int count = 64) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));

    public string GenerateToken(string userName, string secretKey)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userName),
        };

        var secretBytes = Encoding.UTF8.GetBytes(secretKey);
        var key = new SymmetricSecurityKey(secretBytes);

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _optionsMonitor.CurrentValue.Issuer,
            _optionsMonitor.CurrentValue.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(2),
            signingCredentials);

        var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenJson;
    }

    public bool ValidateToken(string token, string secretKey)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secretKey);
        var key = new SymmetricSecurityKey(secretBytes);
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            _ = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _optionsMonitor.CurrentValue.Issuer,
                ValidAudience = _optionsMonitor.CurrentValue.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero,
            }, out var validatedToken);
        }
        catch
        {
            return false;
        }
        return true;
    }
}
