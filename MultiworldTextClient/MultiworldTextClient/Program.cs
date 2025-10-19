using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MultiworldTextClient.Jobs;
using MultiworldTextClient.Managers;
using Quartz;

namespace MultiworldTextClient;

class Program
{
    private static DiscordSocketClient _client;
    private static IScheduler _scheduler;
    public static Dictionary<string, TrackerManager> TrackerManagers = new Dictionary<string, TrackerManager>();
    
    static async Task Main(string[] args)
    {
        /*
        string dbName = "multiworld.db";

        if (!File.Exists(dbName))
        {
            File.Create(dbName).Dispose();
        }

        var context = new ItemsDbContext();
        context.Database.Migrate();
        
        var tracker = new TrackerManager("https://archipelago.gg/api", "0srjqEV4Q_uU38GiI81jZw", "M6P_7kmtSUOPSU6PGKapaQ");
        await tracker.GetStaticTracker();
        await tracker.GetRoomStatus();

        TrackerManagers.Add("0srjqEV4Q_uU38GiI81jZw", tracker);
        */

        /*
        _client = new DiscordSocketClient();

        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += ProcessSlashCommand;

        var token = ""; // TODO: Make this pull from an app settings

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        */
        
        /*
        _scheduler = await SchedulerBuilder.Create()
            .UseDefaultThreadPool(x => x.MaxConcurrency = 5)
            .BuildScheduler();
        
        await _scheduler.Start();
        
        IJobDetail sendMessagesJob = JobBuilder.Create<SendMessagesJob>()
            .UsingJobData("trackerUuid", "0srjqEV4Q_uU38GiI81jZw")
            .WithIdentity("0srjqEV4Q_uU38GiI81jZw-messages")
            .Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
            .WithIdentity("0srjqEV4Q_uU38GiI81jZw-messages-trigger")
            .Build();
        
        await _scheduler.ScheduleJob(sendMessagesJob, trigger);
        */
        
        await Task.Delay(-1);
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