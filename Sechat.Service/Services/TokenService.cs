using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sechat.Service.Settings;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class TokenService
{
    private readonly IUserClaimsPrincipalFactory<IdentityUser> _userClaimsPrincipalFactory;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptionsMonitor<JwtOptions> _optionsMonitor;

    public TokenService(
        IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory,
        UserManager<IdentityUser> userManager,
        IOptionsMonitor<JwtOptions> optionsMonitor)
    {
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _userManager = userManager;
        _optionsMonitor = optionsMonitor;
    }

    public string GenerateSecretKey(int count = 64) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));

    public async Task<string> GenerateToken(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return string.Empty;
        }

        var cp = await _userClaimsPrincipalFactory.CreateAsync(user);

        var secretBytes = Encoding.UTF8.GetBytes(_optionsMonitor.CurrentValue.SecretKey);
        var key = new SymmetricSecurityKey(secretBytes);

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _optionsMonitor.CurrentValue.Issuer,
            _optionsMonitor.CurrentValue.Audience,
            cp.Claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(30),
            signingCredentials);

        var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenJson;
    }

    public bool ValidateToken(string token)
    {
        var secretBytes = Encoding.UTF8.GetBytes(_optionsMonitor.CurrentValue.SecretKey);
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
