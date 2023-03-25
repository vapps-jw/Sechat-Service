using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Data.Models;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class NotificationsController : SechatControllerBase
{
    private readonly UserRepository _userRepository;
    private readonly ILogger<NotificationsController> _logger;
    private readonly IMapper _mapper;

    public NotificationsController(
        UserRepository userRepository,
        ILogger<NotificationsController> logger,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("push-subscribe"), ActionName(nameof(SubscribePush))]
    public async Task<IActionResult> SubscribePush([FromBody] PushSubscriptionDto pushSubscriptionDto)
    {
        var sub = _mapper.Map<NotificationSubscription>(pushSubscriptionDto);
        sub.UserProfileId = UserId;
        _userRepository.AddPushNotificationSubscription(sub);

        return await _userRepository.SaveChanges() > 0 ? Ok() : Problem();
    }

    [HttpDelete("push-unubscribe"), ActionName(nameof(UnsubscribePush))]
    public async Task<IActionResult> UnsubscribePush()
    {
        _userRepository.RemovePushNotificationSubscriptions(UserId);
        return await _userRepository.SaveChanges() > 0 ? Ok() : Problem();
    }
}
