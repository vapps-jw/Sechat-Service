using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sechat.Service.Settings;

namespace Sechat.Service.Configuration.JWT;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private readonly IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration) => _configuration = configuration;

    public void Configure(JwtOptions options) => _configuration.GetSection(nameof(JwtOptions)).Bind(options);
}
