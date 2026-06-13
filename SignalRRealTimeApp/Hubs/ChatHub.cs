using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SignalRRealTimeApp.Hubs
{
    // Remove [Authorize] from the class level
    public class ChatHub : Hub<IChatClient>
    {
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               ILogger<ChatHub> logger)       
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        public override async Task OnConnectedAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(Context.User);

                string userName = user?.UserName ?? "Anonymous";
                string connectionId = Context.ConnectionId;

                ConnectedUsers[connectionId] = userName;

                await Groups.AddToGroupAsync(connectionId, "General");

                _logger.LogInformation(
                    "User {UserName} connected with ID {ConnectionId}",
                    userName,
                    connectionId);

                await Clients.All.UserConnected(userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");

                await Clients.Caller.ReceiveSystemMessage(
                    "System",
                    "An error occurred while connecting.");
            }

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
        // Example of error handling in a hub method
        public async Task SendMessage(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    throw new ArgumentException("Message cannot be empty.");

                var user = await _userManager.GetUserAsync(Context.User);

                await Clients.All.ReceiveMessage(
                    user?.UserName ?? "Anonymous",
                    message);

                _logger.LogInformation(
                    "Message sent by {User}",
                    user?.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");

                await Clients.Caller.ReceiveSystemMessage(
                    "System",
                    $"Error: {ex.Message}");
            }
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

        // This method can now be called by your Console App
        [AllowAnonymous]
        public async Task SendMessageFromClient(string user, string message)
        {
            await Clients.All.ReceiveMessage(user ?? "ConsoleClient", message);
        }

        // Streaming with IAsyncEnumerable
        public async IAsyncEnumerable<string> StreamMessages(
            int count,
            CancellationToken cancellationToken = default)
        {
            for (int i = 1; i <= count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(700); // Simulate delay

                yield return $"Streaming message #{i} at {DateTime.Now:HH:mm:ss}";
            }
        }

        // Progress Reporting
        public async Task StartLongOperation(int steps)
        {
            var user = await _userManager.GetUserAsync(Context.User);

            var progress = new Progress<int>(p =>
            {
                // Send progress updates to the caller
                Clients.Caller.ReceiveProgress($"Operation in progress: {p}%").Wait();
            });

            await PerformLongOperation(steps, progress);

            await Clients.Caller.ReceiveSystemMessage(
                "System",
                "The operation completed successfully."
            );
        }

        private async Task PerformLongOperation(int steps, IProgress<int> progress)
        {
            for (int i = 1; i <= steps; i++)
            {
                await Task.Delay(600);

                progress.Report((i * 100) / steps);
            }
        }
    }
}