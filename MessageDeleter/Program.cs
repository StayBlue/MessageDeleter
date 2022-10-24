// See https://aka.ms/new-console-template for more information

using Discord;

namespace MessageDeleter
{
    class Program
    {
        private const int RateLimit = 5;
        private const int Timeout = 3;
        private static DiscordClient? _client;
        private static int _maxQuantity;
        
        private static async void Delete(DiscordChannel channel, MessageFilters filter, int quantity)
        {
            IReadOnlyList<DiscordMessage> messages;
            
            if (channel.InGuild)
            {
                var textChannel = (TextChannel) channel;

                try
                {
                    messages = await textChannel.GetMessagesAsync(filter);
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Messages deleted successfully.");

                    return;
                }
            }
            else
            {
                var privateChannel = (PrivateChannel) channel;
                
                try
                {
                    messages = await privateChannel.GetMessagesAsync(filter);
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Messages deleted successfully.");

                    return;
                }
            }

            var count = 0;

            foreach (var message in messages)
            {
                if (message.Author.User.Id != _client?.User.Id) continue;
                
                if (_maxQuantity != -1 && _maxQuantity <= quantity)
                {
                    Console.WriteLine("Reached specified quantity of messages deleted.");

                    return;
                }
                
                await message.DeleteAsync();

                // Keep count of requests for quantity
                quantity++;
                // Keep count of requests for rate limit
                count++;

                if (count < RateLimit) continue;
                
                Thread.Sleep(Timeout * 1000);
                    
                count = 0;
            }
            
            Console.WriteLine($"Deleted {quantity} message(s) so far, continuing...");
            
            Delete(channel, filter, quantity);
        }

        private static async void Run()
        {
            Console.WriteLine("MessageDeleter. Powered by Anarchy, Made by StayBlue#0001.");
            
            Console.Write("Input your token: ");
            
            var token = Console.ReadLine();

            if (token is null)
            {
                Console.WriteLine("An error occurred while grabbing your token.");

                return;
            }

            Console.Write("Input the channel ID: ");
            
            if (!ulong.TryParse(Console.ReadLine(), out var channelId))
            {
                Console.WriteLine("An error occurred while grabbing the channel ID.");

                return;
            }
            
            Console.Write("Input the quantity. If you want it to be all messages, input -1: ");
            
            if (!int.TryParse(Console.ReadLine(), out _maxQuantity))
            {
                Console.WriteLine("An error occurred while grabbing the channel ID.");

                return;
            }
            
            _client = new DiscordClient(token, new ApiConfig
            {
                RetryOnRateLimit = true
            });
            
            Console.WriteLine($"Logged in as: {_client.User.Username}#{_client.User.Discriminator:D4}");

            var channel = await _client.GetChannelAsync(channelId);
            
            var filter = new MessageFilters
            {
                AuthorId = _client.User.Id,
                Limit = 25
            };
            
            var task = Task.Run(() => Delete(channel, filter, 0));

            task.Wait();
            
            Console.ReadKey();
        }
        
        private static void Main()
        {
            Task.Run(Run);

            Thread.Sleep(-1);
        }
    }
}