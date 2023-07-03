using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Dtos.CookieObjects;
using Sechat.Service.Services;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class CryptoController : SechatControllerBase
{
    private readonly CryptographyService _cryptographyService;

    public CryptoController(CryptographyService cryptographyService) => _cryptographyService = cryptographyService;

    [HttpPost("encrypt-string")]
    public IActionResult Encrypt() => Ok();

    [HttpPost("decrypt-message")]
    public IActionResult DecryptMessage([FromBody] MessageDecryptionRequest messageDecryptionRequest)
    {
        var key = ExtractE2ECookieDataForRoom(messageDecryptionRequest.RoomId);
        if (key is null)
        {
            return Unauthorized("You dont have a key");
        }

        if (_cryptographyService.Decrypt(messageDecryptionRequest.Message, key.Key, out var result))
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
        var key = ExtractE2ECookieDataForContact(messageDecryptionRequest.ContactId);
        if (key is null)
        {
            return Unauthorized("You dont have a key");
        }

        if (_cryptographyService.Decrypt(messageDecryptionRequest.Message, key.Key, out var result))
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
