using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos.CookieObjects;
using Sechat.Service.Services;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
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

    [HttpPost("decrypt-message")]
    public IActionResult DecryptMessage([FromBody] MessageDecryptionRequest messageDecryptionRequest)
    {
        var e2eKey = ExtractE2ECookieDataForRoom(messageDecryptionRequest.RoomId);
        if (e2eKey is null)
        {
            return Unauthorized("You dont have a key");
        }

        if (_cryptographyService.Decrypt(messageDecryptionRequest.Message, e2eKey.Key, out var result))
        {
            messageDecryptionRequest.Message = result;
        }
        else
        {
            messageDecryptionRequest.Message = result;
            messageDecryptionRequest.Error = true;
        }

        return Ok(messageDecryptionRequest);
    }

    [HttpPost("decrypt-direct-message")]
    public IActionResult DecryptDirectMessage([FromBody] DirectMessageDecryptionRequest messageDecryptionRequest)
    {
        var e2eKey = ExtractE2ECookieDataForContact(messageDecryptionRequest.ContactId);
        if (e2eKey is null)
        {
            return Unauthorized("You dont have a key");
        }

        if (_cryptographyService.Decrypt(messageDecryptionRequest.Message, e2eKey.Key, out var result))
        {
            messageDecryptionRequest.Message = result;
        }
        else
        {
            messageDecryptionRequest.Message = result;
            messageDecryptionRequest.Error = true;
        }

        return Ok(messageDecryptionRequest);
    }
}
