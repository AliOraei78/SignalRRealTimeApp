using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRRealTimeApp.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            string? userName = Context.GetHttpContext()?.Request.Query["user"]
                              ?? $"User_{connectionId.Substring(0, 5)}";

            ConnectedUsers[connectionId] = userName;

            // Automatically join default group "General"
            await Groups.AddToGroupAsync(connectionId, "General");

            await Clients.All.SendAsync("UserConnected", userName, connectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;

            if (ConnectedUsers.TryRemove(connectionId, out string? userName))
            {
                await Clients.All.SendAsync("UserDisconnected", userName, connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // New method: join group
        public async Task JoinGroup(string groupName)
        {
            string userName = ConnectedUsers[Context.ConnectionId];

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync(
                "ReceiveSystemMessage",
                "System",
                $"{userName} joined group {groupName}.");
        }

        // New method: leave group
        public async Task LeaveGroup(string groupName)
        {
            string userName = ConnectedUsers[Context.ConnectionId];

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync(
                "ReceiveSystemMessage",
                "System",
                $"{userName} left group {groupName}.");
        }

        // New method: send message to a specific group
        public async Task SendToGroup(string groupName, string message)
        {
            string userName = ConnectedUsers[Context.ConnectionId];

            await Clients.Group(groupName).SendAsync("ReceiveMessage", userName, message);
        }

        // Existing compatibility methods
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task GetOnlineUsers()
        {
            var users = ConnectedUsers.Values.ToList();
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", users);
        }
    }
}