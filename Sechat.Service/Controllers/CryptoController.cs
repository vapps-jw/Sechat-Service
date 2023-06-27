using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Dtos.ChatDtos;
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
    public IActionResult Decrypt([FromBody] MessageToDecrypt messageToDecrypt)
    {
        var roomKey = ExtractE2ECookieDataForRoom(messageToDecrypt.RoomId);
        if (roomKey is null)
        {
            messageToDecrypt.Text = "Room Password Missing";
            return Ok(messageToDecrypt);
        }

        return Ok();

    }
}
