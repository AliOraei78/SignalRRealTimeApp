namespace SignalRRealTimeApp.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceivePrivateMessage(string sender, string message);
        Task ReceiveSystemMessage(string user, string message);
        Task UserConnected(string userName);
        Task UserDisconnected(string userName);
        Task ReceiveOnlineUsers(List<string> users);
    }
}