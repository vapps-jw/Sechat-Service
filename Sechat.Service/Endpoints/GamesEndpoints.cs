using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using System.Security.Claims;
using System.Threading;

namespace Sechat.Service.Endpoints;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("games").RequireAuthorization();
        _ = group.MapGet("activeSessions", GetActiveSessions).WithName(nameof(GetActiveSessions));
    }

    public static IResult GetActiveSessions(
        HttpContext http,
        ClaimsPrincipal user,
        IMapper mapper,
        CancellationToken cancellationToken,
        IDbContextFactory<SechatContext> contextFactory) => Results.Ok();
}
