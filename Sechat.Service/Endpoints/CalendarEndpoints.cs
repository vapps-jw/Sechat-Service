using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Dtos.CalendarDtos;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Endpoints;

public static class CalendarEndpoints
{
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("v2/calendar").RequireAuthorization();
        _ = group.MapPost("events", CreateEvent).WithName(nameof(CreateEvent));
        _ = group.MapGet("", GetCalendar).WithName(nameof(GetCalendar));
    }

    public static async Task<IResult> GetCalendar(
        HttpContext http,
        ClaimsPrincipal user,
        IMapper mapper,
        CancellationToken cancellationToken,
        IDbContextFactory<SechatContext> contextFactory)
    {
        using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var userId = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value;

        var calendar = ctx.Calendars
            .Where(c => c.UserProfileId.Equals(userId))
            .AsSplitQuery()
            .Include(c => c.CalendarEvents)
            .ThenInclude(ce => ce.Reminders)
            .FirstOrDefault();
        if (calendar is null) return Results.BadRequest();

        http.Response.Headers.CacheControl = Utilities.ResponseHeaders.NoStore;
        var dto = mapper.Map<CalendarDto>(calendar);
        return Results.Ok(dto);

    }

    public static async Task<IResult> CreateEvent(
        HttpContext http,
        ClaimsPrincipal user,
        IMapper mapper,
        CancellationToken cancellationToken,
        IDbContextFactory<SechatContext> contextFactory,
        NewEventForm form)
    {
        using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
        var userId = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value;

        var calendar = ctx.Calendars.FirstOrDefault(c => c.UserProfileId.Equals(userId));
        var newEvent = new CalendarEvent()
        {
            Id = Guid.NewGuid().ToString(),
            Data = form.Data,
        };

        calendar.CalendarEvents.Add(newEvent);
        var result = mapper.Map<CalendarEventDto>(newEvent);

        http.Response.Headers.CacheControl = Utilities.ResponseHeaders.NoStore;
        return await ctx.SaveChangesAsync(cancellationToken) > 0 ? Results.Ok(result) : Results.BadRequest();
    }
}

public class NewEventForm
{
    public string Data { get; set; }
}