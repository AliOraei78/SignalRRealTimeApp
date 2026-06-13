using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRRealTimeApp.Hubs
{
    // Remove [Authorize] from the class level
    public class ChatHub : Hub<IChatClient>
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ChatHub(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public override async Task OnConnectedAsync()
        {
            // This will now safely handle both logged-in and anonymous users
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

        // Add [Authorize] to specific methods that require login
        [Authorize]
        public async Task SendMessage(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Clients.All.ReceiveMessage(user?.UserName ?? "Anonymous", message);
        }

        [Authorize(Roles = "Admin")]
        public async Task SendAdminAnnouncement(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Clients.All.ReceiveSystemMessage("Admin", $"Important announcement: {message}");
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task AdminOnlyMethod()
        {
            await Clients.Caller.ReceiveSystemMessage(
                "System",
                "This method is only accessible to admins."
            );
        }

        [Authorize]
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

        [Authorize]
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

        public async Task<string> EchoMessage(string message)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            return $"Echo from {user?.UserName ?? "Anonymous"}: {message}";
        }

        public async IAsyncEnumerable<string> StreamMessages(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                await Task.Delay(800); // simulated delay
                yield return $"Message {i} from server (Streaming)";
            }
        }

        // This method can now be called by your Console App
        [AllowAnonymous]
        public async Task SendMessageFromClient(string user, string message)
        {
            await Clients.All.ReceiveMessage(user ?? "ConsoleClient", message);
        }
    }
}