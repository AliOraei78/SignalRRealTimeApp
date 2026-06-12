using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRRealTimeApp.Hubs
{
    public class ChatHub : Hub
    {
        // In-memory storage of connection IDs for online users
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            string? userName = Context.GetHttpContext()?.Request.Query["user"]
                              ?? $"User_{connectionId.Substring(0, 5)}";

            ConnectedUsers[connectionId] = userName;

            // Notify all clients about the new user
            await Clients.All.SendAsync("UserConnected", userName, connectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;

            if (ConnectedUsers.TryRemove(connectionId, out string? userName))
            {
                // Notify all clients about user disconnection
                await Clients.All.SendAsync("UserDisconnected", userName, connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Existing SendMessage method (for compatibility)
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // New method: get list of online users
        public async Task GetOnlineUsers()
        {
            var users = ConnectedUsers.Values.ToList();
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", users);
        }
    }
}