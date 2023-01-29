using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Sechat.Data.Repositories;
using Sechat.Service.Services;
using Sechat.Tests.Utils;

namespace Sechat.Tests;
public class ChatRepositoryTests
{
    [Fact]
    public async Task CreateRoomTest()
    {
        using var masterApp = new MockedApi();
        using var scope = masterApp.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var sechatRepo = scope.ServiceProvider.GetRequiredService<ChatRepository>();
        var encryptor = scope.ServiceProvider.GetRequiredService<IEncryptor>();

        var newRoomName = Guid.NewGuid().ToString();
        var newRoomKey = encryptor.GenerateKey();

        var inviter = await userManager.FindByNameAsync("u1");
        _ = sechatRepo.CreateRoom(newRoomName, inviter?.Id, inviter?.UserName, newRoomKey);

        _ = await sechatRepo.SaveChanges();
        sechatRepo.ClearTracker();

        var res = (await sechatRepo.GetRooms(inviter?.Id)).FirstOrDefault();
        var member = res?.Members.FirstOrDefault();

        Assert.NotNull(res);
        Assert.NotNull(member);
        Assert.Equal(inviter?.Id, res.CreatorId);
        Assert.Equal(inviter?.Id, member.Id);
    }

    [Fact]
    public async Task IsRoomAllowedTest()
    {
        using var masterApp = new MockedApi();
        using var scope = masterApp.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var sechatRepo = scope.ServiceProvider.GetRequiredService<ChatRepository>();
        var encryptor = scope.ServiceProvider.GetRequiredService<IEncryptor>();

        var newRoomName = Guid.NewGuid().ToString();
        var newRoomKey = encryptor.GenerateKey();

        var inviter = await userManager.FindByNameAsync("u1");
        var roomId = sechatRepo.CreateRoom(newRoomName, inviter?.Id, inviter?.UserName, newRoomKey).Id;

        _ = await sechatRepo.SaveChanges();
        sechatRepo.ClearTracker();

        var res = sechatRepo.IsRoomAllowed(inviter?.Id, roomId);

        Assert.True(res);
    }
}
