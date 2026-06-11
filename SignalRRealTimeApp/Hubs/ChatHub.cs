using Microsoft.AspNetCore.SignalR;

namespace SignalRRealTimeApp.Hubs
{
    public class ChatHub : Hub
    {
        // Simple method that can be called from the client
        public async Task SendMessage(string user, string message)
        {
            // Send message to all connected clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}