using Microsoft.AspNetCore.SignalR;

namespace SignalRRealTimeApp.Hubs
{
    public class ChatHub : Hub
    {
        // Main method: broadcast a message to all connected clients
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // New method: send a message only to the caller
        public async Task SendToCaller(string message)
        {
            await Clients.Caller.SendAsync(
                "ReceivePrivateMessage",
                $"Private message from the server: {message}");
        }

        // New method: send a message to everyone except the caller
        public async Task SendToOthers(string user, string message)
        {
            await Clients.Others.SendAsync(
                "ReceiveMessage",
                $"{user} (to others)",
                message);
        }

        // New method: broadcast a system message (for testing)
        public async Task BroadcastSystemMessage(string message)
        {
            await Clients.All.SendAsync(
                "ReceiveSystemMessage",
                "System",
                message);
        }
    }
}