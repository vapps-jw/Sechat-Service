using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sechat.Service.Settings;
using System;
using System.Text;

namespace Sechat.Service.Configuration.JWT;

public class JwtBearerOptionsSetup : IConfigureOptions<JwtBearerOptions>
{
    private readonly JwtOptions _options;
    private readonly IHostEnvironment _env;

    public JwtBearerOptionsSetup(
        IHostEnvironment env,
        IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _env = env;
    }


    public void Configure(JwtBearerOptions options)
    {
        if (_env.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }

        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true
        };
    }
}
