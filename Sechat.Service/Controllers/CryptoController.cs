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
    public IActionResult Decrypt([FromBody] MessageDecryptionRequest messageDecryptionRequest)
    {
        var roomKey = ExtractE2ECookieDataForRoom(messageDecryptionRequest.RoomId);
        if (roomKey is null)
        {
            return Unauthorized("You dont have a key");
        }

        if (_cryptographyService.Decrypt(messageDecryptionRequest.Message, roomKey.Key, out var result))
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
