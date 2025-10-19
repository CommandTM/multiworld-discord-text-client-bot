using Quartz;

namespace MultiworldTextClient.Jobs;

public class SendMessagesJob : IJob
{
    public string TrackerUuid { get; set; }
    public string GuildId { get; set; }
    public string ChannelId { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var tracker = Program.TrackerManagers[TrackerUuid];

        Console.WriteLine($"Sending Messages For {TrackerUuid}...");
        
        ulong guildId = ulong.Parse(GuildId);
        ulong channelId = ulong.Parse(ChannelId);
        
        await tracker.SendItemMessaages(guildId, channelId);
    }
}