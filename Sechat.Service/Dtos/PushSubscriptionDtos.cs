using System;

namespace Sechat.Service.Dtos;

public record PushSubscriptionDto(string Endpoint, DateTime? ExpirationTime, PushSubscriptionKeys Keys);
public record PushSubscriptionKeys(string P256dh, string Auth);