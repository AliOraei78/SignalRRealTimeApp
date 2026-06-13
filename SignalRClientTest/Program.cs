using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClientTest
{
    class Program
    {
        private static HubConnection? connection;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== SignalR .NET Client Test ===\n");

            string hubUrl = "https://localhost:7063/chatHub";   // Check your own port

            connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            // Receive messages
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"📨 {user}: {message}");
            });

            connection.On<string, string>("ReceiveSystemMessage", (user, message) =>
            {
                Console.WriteLine($"[{user}] {message}");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("✅ Connection established successfully!\n");

                Console.WriteLine("Commands:");
                Console.WriteLine("1 - Send message to everyone (without login)");
                Console.WriteLine("Q - Quit\n");

                while (true)
                {
                    Console.Write("Select: ");
                    var input = Console.ReadLine()?.Trim().ToUpper();

                    if (input == "Q")
                        break;

                    if (input == "1")
                    {
                        Console.Write("Your name: ");
                        var user = Console.ReadLine();

                        Console.Write("Message: ");
                        var msg = Console.ReadLine();

                        await connection.InvokeAsync("SendMessageFromClient", user, msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
            finally
            {
                if (connection != null)
                    await connection.DisposeAsync();
            }
        }
    }
}