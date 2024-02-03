using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Services;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[Authorize(AppConstants.AuthorizationPolicy.ChatPolicy)]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class CryptoController : SechatControllerBase
{
    private readonly CryptographyService _cryptographyService;
    private readonly UserRepository _userRepository;

    public CryptoController(
        CryptographyService cryptographyService,
        UserRepository userRepository)
    {
        _cryptographyService = cryptographyService;
        _userRepository = userRepository;
    }

    [HttpPatch("reset-default-key")]
    public async Task<IActionResult> ResetDefaultKey()
    {
        var newKey = _cryptographyService.GenerateKey();
        _userRepository.UpdatKey(UserId, Data.KeyType.DefaultEncryption, newKey);

        return await _userRepository.SaveChanges() > 0 ? Ok("Key reset successfull") : Problem("Issue when resetting the Key");
    }

    [HttpGet("new-key")]
    public IActionResult GetNewKey() => Ok(_cryptographyService.GenerateKey());
}
