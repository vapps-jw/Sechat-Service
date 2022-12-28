using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sechat.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthForecastController : ControllerBase
    {
        private readonly ILogger<AuthForecastController> _logger;

        public AuthForecastController(ILogger<AuthForecastController> logger) => _logger = logger;

    }
}
