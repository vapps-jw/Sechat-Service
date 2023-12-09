using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sechat.Service.Configuration;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class GamesController : SechatControllerBase
{
    public GamesController()
    {

    }
}
