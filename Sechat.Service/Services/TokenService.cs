using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sechat.Data;
using Sechat.Data.Models.UserDetails;
using Sechat.Service.Settings;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class TokenService
{
    private readonly IUserClaimsPrincipalFactory<IdentityUser> _userClaimsPrincipalFactory;
    private readonly SechatContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IOptionsMonitor<TokenSettings> _optionsMonitor;

    public TokenService(
        IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory,
        SechatContext context,
        UserManager<IdentityUser> userManager,
        IOptionsMonitor<TokenSettings> optionsMonitor)
    {
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _context = context;
        _userManager = userManager;
        _optionsMonitor = optionsMonitor;
    }

    public string GenerateSecretKey(int count = 64) => Convert.ToBase64String(RandomNumberGenerator.GetBytes(count));

    public async Task<string> CheckForUserAuthSecret(UserProfile profile)
    {
        var key = profile.Keys.FirstOrDefault(k => k.Type == KeyType.AuthToken);
        if (key is not null)
        {
            return key.Value;
        }

        var newSecret = GenerateSecretKey();
        profile.Keys.Add(new Key() { Type = KeyType.AuthToken, Value = newSecret });

        return await _context.SaveChangesAsync() > 0 ? newSecret : throw new Exception("Issue when saving secret for jwt");
    }

    public async Task<string> GenerateToken(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return string.Empty;
        }

        var profile = _context.UserProfiles
            .Where(p => p.Id.Equals(user.Id))
            .Include(p => p.Keys)
            .FirstOrDefault();

        if (profile is null)
        {
            return string.Empty;
        }

        var cp = await _userClaimsPrincipalFactory.CreateAsync(user);

        var secret = await CheckForUserAuthSecret(profile);
        var secretBytes = Encoding.UTF8.GetBytes(secret);
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

    public async Task<bool> ValidateToken(string userName, string token)
    {
        var receivedTokenHandler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = receivedTokenHandler.ReadJwtToken(token);

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return false;
        }

        var profile = _context.UserProfiles
            .Where(p => p.Id.Equals(user.Id))
            .Include(p => p.Keys)
            .FirstOrDefault();

        if (profile is null)
        {
            return false;
        }

        var secret = await CheckForUserAuthSecret(profile);
        var secretBytes = Encoding.UTF8.GetBytes(secret);
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
