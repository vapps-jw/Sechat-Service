using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Data.Models;
using Sechat.Service.Dtos;
using System;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class NotificationsController : SechatControllerBase
{
    private readonly ILogger<NotificationsController> _logger;
    private readonly IMapper _mapper;

    public NotificationsController(ILogger<NotificationsController> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    [HttpPost("push-subscribe"), ActionName(nameof(SubscribeToPush))]
    public IActionResult SubscribeToPush([FromBody] PushSubscriptionDto pushSubscriptionDto)
    {
        var sub = _mapper.Map<NotificationSubscription>(pushSubscriptionDto);
        sub.UserProfileId = UserId;
        Console.WriteLine(sub);
        return Ok();
    }
}
