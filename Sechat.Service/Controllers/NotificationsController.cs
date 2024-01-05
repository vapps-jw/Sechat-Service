using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Data.Models.UserDetails;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
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

        if (_userRepository.AlreadySubscribed(sub)) return BadRequest("Already subscribed");

        _userRepository.AddPushNotificationSubscription(sub);

        return await _userRepository.SaveChanges() > 0 ? Ok() : Problem();
    }

    [HttpPost("is-subscribed"), ActionName(nameof(SubscribePush))]
    public bool CheckSubscriptionPush([FromBody] PushSubscriptionDto pushSubscriptionDto)
    {
        var sub = _mapper.Map<NotificationSubscription>(pushSubscriptionDto);
        sub.UserProfileId = UserId;

        return _userRepository.AlreadySubscribed(sub);
    }

    [HttpDelete("push-unubscribe"), ActionName(nameof(UnsubscribePush))]
    public async Task<IActionResult> UnsubscribePush()
    {
        _userRepository.RemovePushNotificationSubscriptions(UserId);
        var res = await _userRepository.SaveChanges();
        return Ok(res);
    }
}
