using Discord;
using Discord.Net;
using Discord.WebSocket;
using MultiworldTextClient.Data;
using MultiworldTextClient.Managers;

namespace MultiworldTextClient;

class Program
{
    private static DiscordSocketClient _client;
    
    static async Task Main(string[] args)
    {
        var tracker = new TrackerManager("https://archipelago.gg/api", "eDJlIS87SaCwKcTyQM-ZXg", "hmrTjtQhSKu4tDDOCYGkaw");
        await tracker.GetStaticTracker();
        await tracker.GetRoomStatus();

        await tracker.SendItemMessaages();

        /*
        _client = new DiscordSocketClient();

        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += ProcessSlashCommand;

        var token = ""; // TODO: Make this pull from an app settings

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
        */
    }

    private static async Task ProcessSlashCommand(SocketSlashCommand arg)
    {
        switch (arg.Data.Name)
        {
            case "say":
                await arg.RespondAsync("Hello!");
                break;
        }
    }

    private static async Task Ready()
    {
        var globalCommand = new SlashCommandBuilder()
            .WithName("say")
            .WithDescription("Responds With Something");
        
        await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
    }

    private static async Task Log(LogMessage arg)
    {
        Console.WriteLine(arg.Message);
    }
}