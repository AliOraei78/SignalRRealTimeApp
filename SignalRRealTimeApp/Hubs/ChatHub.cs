using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRRealTimeApp.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        private readonly UserManager<IdentityUser> _userManager;

        public ChatHub(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            string userName = user?.UserName ?? "Anonymous";
            string connectionId = Context.ConnectionId;

            ConnectedUsers[connectionId] = userName;
            await Groups.AddToGroupAsync(connectionId, "General");

            await Clients.All.UserConnected(userName);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            if (ConnectedUsers.TryRemove(connectionId, out string? userName))
            {
                await Clients.All.UserDisconnected(userName);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Send message to everyone
        public async Task SendMessage(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Clients.All.ReceiveMessage(user?.UserName ?? "Anonymous", message);
        }

        // Send message to a specific user
        public async Task SendToUser(string targetUserName, string message)
        {
            var targetUser = await _userManager.FindByNameAsync(targetUserName);
            if (targetUser != null)
            {
                var sender = await _userManager.GetUserAsync(Context.User);
                await Clients.User(targetUser.Id)
                    .ReceivePrivateMessage(sender?.UserName ?? "Anonymous", message);
            }
        }

        // Send to group
        public async Task SendToGroup(string groupName, string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Clients.Group(groupName)
                .ReceiveMessage(user?.UserName ?? "Anonymous", message);
        }

        public async Task GetOnlineUsers()
        {
            var users = ConnectedUsers.Values.ToList();
            await Clients.Caller.ReceiveOnlineUsers(users);
        }
    }
}