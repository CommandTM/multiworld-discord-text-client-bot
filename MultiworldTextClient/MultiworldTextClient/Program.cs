using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using MultiworldTextClient.Data;
using MultiworldTextClient.Data.Database;
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
        string dbName = "multiworld.db";
        string tokenName = "token";

        if (!File.Exists(dbName))
        {
            File.Create(dbName).Dispose();
        }
        var context = new ItemsDbContext();
        context.Database.Migrate();

        if (!File.Exists(tokenName))
        {
            File.Create(tokenName).Dispose();
            Console.WriteLine($"Bot token not present, enter token into the newly created '{tokenName}' file.");
            Environment.Exit(0);
        }
        string token = File.ReadAllText(tokenName);
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"Bot token not present, enter token into the '{tokenName}' file.");
            Environment.Exit(0);
        }
        
        _client = new DiscordSocketClient();

        _client.Log += Log;
        _client.Ready += Ready;
        _client.SlashCommandExecuted += ProcessSlashCommand;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }

    private static async Task CreateScheduler()
    {
        _scheduler = await SchedulerBuilder.Create()
            .UseDefaultThreadPool(x => x.MaxConcurrency = 5)
            .BuildScheduler();
        
        await _scheduler.Start();
    }

    private static async Task PopulateTrackedWorlds()
    {
        using (var context = new ItemsDbContext())
        {
            var worlds = context.TrackedWorlds.ToList();
            foreach (var world in worlds)
            {
                var tracker = new TrackerManager(world.BaseUrl, world.TrackerUuid, world.RoomUuid);
                await tracker.GetStaticTracker();
                await tracker.GetRoomStatus();
                
                TrackerManagers.Add(world.TrackerUuid, tracker);
                
                IJobDetail sendMessagesJob = JobBuilder.Create<SendMessagesJob>()
                    .UsingJobData("trackerUuid", world.TrackerUuid)
                    .UsingJobData("guildId", $"{world.GuildId}")
                    .UsingJobData("channelId", $"{world.ChannelId}")
                    .WithIdentity($"{world.TrackerUuid}-messages")
                    .Build();
        
                ITrigger trigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
                    .WithIdentity($"{world.TrackerUuid}-messages-trigger")
                    .Build();
        
                await _scheduler.ScheduleJob(sendMessagesJob, trigger);
            }
        }
    }

    private static async Task ProcessSlashCommand(SocketSlashCommand arg)
    {
        try
        {
            switch (arg.Data.Name)
            {
                case "starttracking":
                    await StartTrackingRoom(arg);
                    break;
                case "requestlocationsend":
                    await RequestLocationSend(arg);
                    break;
                case "requestslotrelease":
                    await RequestSlotRelease(arg);
                    break;
            }
        }
        catch (Exception ex)
        {
            await arg.RespondAsync(ex.Message, ephemeral: true);
        }
    }

    private static async Task StartTrackingRoom(SocketSlashCommand arg)
    {
        await arg.RespondAsync("Starting tracking...", ephemeral: true);

        string baseUrl = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("baseurl")).Value.ToString();
        baseUrl += "/api";
        baseUrl = "https://" + baseUrl;
        string trackerUuid = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("trackeruuid")).Value.ToString();
        string roomUuid = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("roomuuid")).Value.ToString();
            
        var tracker = new TrackerManager(baseUrl, trackerUuid, roomUuid);
        await tracker.GetStaticTracker();
        await tracker.GetRoomStatus();

        TrackerManagers.Add(trackerUuid, tracker);
        
        IJobDetail sendMessagesJob = JobBuilder.Create<SendMessagesJob>()
            .UsingJobData("trackerUuid", trackerUuid)
            .UsingJobData("guildId", $"{arg.GuildId}")
            .UsingJobData("channelId", $"{arg.ChannelId}")
            .WithIdentity($"{trackerUuid}-messages")
            .Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .StartNow()
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(1).RepeatForever())
            .WithIdentity($"{trackerUuid}-messages-trigger")
            .Build();
        
        await _scheduler.ScheduleJob(sendMessagesJob, trigger);
        
        TrackedWorld world =  new TrackedWorld()
        {
            BaseUrl = baseUrl,
            TrackerUuid = trackerUuid,
            RoomUuid = roomUuid,
            GuildId = arg.GuildId ?? 0,
            ChannelId = arg.ChannelId ?? 0
        };

        using (var context = new ItemsDbContext())
        {
            context.Add(world);
            context.SaveChanges();
        }
    }

    private static async Task RequestLocationSend(SocketSlashCommand arg)
    {
        var guildId = arg.GuildId ?? 0;
        string slotName = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("slotname")).Value.ToString();
        string locationName = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("locationname")).Value.ToString();
        
        await arg.RespondAsync($"Sending {slotName}'s {locationName}...", ephemeral: false);
        
        using (var context = new ItemsDbContext())
        {
            TrackedWorld? world;
            
            if (context.TrackedWorlds.Where(tw => tw.GuildId == guildId).Count() > 1)
            {
                string? roomUuid = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("roomuuid"))?.Value.ToString();
                if (roomUuid == null)
                {
                    await SendMessage("Must Specify A Room UUID", arg.GuildId ?? 0, arg.ChannelId ?? 0);
                    return;
                }
                world = context.TrackedWorlds.FirstOrDefault(t => t.RoomUuid == roomUuid);
            }
            else
            {
                world = context.TrackedWorlds.FirstOrDefault(w => w.GuildId == guildId);
            }

            if (world == null)
            {
                await SendMessage("No Valid Room Found", arg.GuildId ?? 0, arg.ChannelId ?? 0);
                return;
            }
            
            var trackerManager = TrackerManagers[world.TrackerUuid];
            
            await trackerManager.SendLocation(guildId, arg.ChannelId ?? 0, slotName, locationName);
        }
    }
    
    private static async Task RequestSlotRelease(SocketSlashCommand arg)
    {
        var guildId = arg.GuildId ?? 0;
        string slotName = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("slotname")).Value.ToString();
        
        await arg.RespondAsync($"Releasing {slotName}...", ephemeral: false);
        
        using (var context = new ItemsDbContext())
        {
            TrackedWorld? world;
            
            if (context.TrackedWorlds.Where(tw => tw.GuildId == guildId).Count() > 1)
            {
                string? roomUuid = arg.Data.Options.FirstOrDefault(o => o.Name.Equals("roomuuid"))?.Value.ToString();
                if (roomUuid == null)
                {
                    await SendMessage("Must Specify A Room UUID", arg.GuildId ?? 0, arg.ChannelId ?? 0);
                    return;
                }
                world = context.TrackedWorlds.FirstOrDefault(t => t.RoomUuid == roomUuid);
            }
            else
            {
                world = context.TrackedWorlds.FirstOrDefault(w => w.GuildId == guildId);
            }

            if (world == null)
            {
                await SendMessage("No Valid Room Found", arg.GuildId ?? 0, arg.ChannelId ?? 0);
                return;
            }
            
            var trackerManager = TrackerManagers[world.TrackerUuid];
            
            await trackerManager.ReleaseSlot(guildId, arg.ChannelId ?? 0, slotName);
        }
    }

    private static async Task Ready()
    {
        var startTrackingCommand = new SlashCommandBuilder()
            .WithName("starttracking")
            .WithDescription("Starts Tracking A Multiworld Room")
            .WithDefaultMemberPermissions(GuildPermission.Administrator)
            .AddOption("baseurl", ApplicationCommandOptionType.String, "Base URL of Multiworld room host", true)
            .AddOption("trackeruuid", ApplicationCommandOptionType.String, "Tracker UUID of room", true)
            .AddOption("roomuuid", ApplicationCommandOptionType.String, "Room UUID of room", true);

        var requestLocationSendCommand = new SlashCommandBuilder()
            .WithName("requestlocationsend")
            .WithDescription("Requests A Location Release")
            .WithDefaultMemberPermissions(GuildPermission.Administrator)
            .AddOption("slotname", ApplicationCommandOptionType.String, "Slot Name the location belongs to", true)
            .AddOption("locationname", ApplicationCommandOptionType.String, "EXACT location name to release", true)
            .AddOption("roomuuid", ApplicationCommandOptionType.String, "Room UUID of room, must specify if multiple worlds are tracked in one guild", false);
        
        var requestSlotReleaseCommand = new SlashCommandBuilder()
            .WithName("requestslotrelease")
            .WithDescription("Requests A Slot Release")
            .WithDefaultMemberPermissions(GuildPermission.Administrator)
            .AddOption("slotname", ApplicationCommandOptionType.String, "Slot Name to be released", true)
            .AddOption("roomuuid", ApplicationCommandOptionType.String, "Room UUID of room, must specify if multiple worlds are tracked in one guild", false);
        
        await _client.CreateGlobalApplicationCommandAsync(startTrackingCommand.Build());
        await _client.CreateGlobalApplicationCommandAsync(requestLocationSendCommand.Build());
        await _client.CreateGlobalApplicationCommandAsync(requestSlotReleaseCommand.Build());
        
        await CreateScheduler();
        await PopulateTrackedWorlds();
    }

    private static async Task Log(LogMessage arg)
    {
        Console.WriteLine(arg.Message);
    }

    public static async Task SendMessage(string message, ulong guildId, ulong channelId)
    {
        var guild = _client.GetGuild(guildId);
        
        var channel = guild.GetTextChannel(channelId);

        await channel.SendMessageAsync(message);
    }
}