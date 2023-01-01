using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sechat.Service.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {

        public ChatHub()
        {

        }
    }
}
