using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient Client;
        private CommandService Commands;
        static void Main(string[] args) => new Program().mainAsync().GetAwaiter().GetResult();

        private async Task mainAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Discord.LogSeverity.Debug
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                LogLevel = Discord.LogSeverity.Debug
            });

            Client.MessageReceived += Client_MessageReceived;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            Client.Ready += Client_Ready;
            Client.Log += Client_Log;

            await Client.LoginAsync(Discord.TokenType.Bot, [REDACTED FOR GITHUB]);

            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Log(Discord.LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source} -- {Message.Message}");
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync("Deep Dive Bot");
        }

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            int ArgPos = 0;
            char prefix = '!';

            if (!(Message.HasCharPrefix(prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;

            var Results = await Commands.ExecuteAsync(Context, ArgPos, null);
            if (!Results.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands -- Something went wrong. Text: {Context.Message.Content} | Error: {Results.ErrorReason}");
                await Context.Channel.SendMessageAsync($"{Context.Message.Author.Mention} - That didn't work, here's why: {Results.ErrorReason}");
            }
        }
    }
}
