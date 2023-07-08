using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.CookieObjects;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Sechat.Service.Controllers;

[ApiController]
public abstract class SechatControllerBase : ControllerBase
{
    protected string UserId => GetClaim(ClaimTypes.NameIdentifier);
    protected string UserName => GetClaim(ClaimTypes.Name);

    private string GetClaim(string claimType) => User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value;

    protected E2EData ExtractE2ECookieDataForRoom(string roomId)
    {
        var e2e = Request.Cookies[AppConstants.Cookies.E2E];
        if (string.IsNullOrWhiteSpace(e2e)) return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var e2eData = JsonSerializer.Deserialize<E2EData[]>(e2e, options);
        return e2eData.FirstOrDefault(k => k.RoomId.Equals(roomId));
    }

    protected E2EDMData ExtractE2ECookieDataForContact(long contactId)
    {
        var e2e = Request.Cookies[AppConstants.Cookies.E2E_DM];
        if (string.IsNullOrWhiteSpace(e2e)) return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var e2eData = JsonSerializer.Deserialize<E2EDMData[]>(e2e, options);
        return e2eData.FirstOrDefault(k => k.ContactId == contactId);
    }

    protected E2ENotebookData ExtractE2ECookieDataForNoteboook(string notebookId)
    {
        var e2e = Request.Cookies[AppConstants.Cookies.E2E_NOTEBOOK];
        if (string.IsNullOrWhiteSpace(e2e)) return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var e2eData = JsonSerializer.Deserialize<E2ENotebookData[]>(e2e, options);
        return e2eData.FirstOrDefault(k => k.NotebookId == notebookId);
    }
}
