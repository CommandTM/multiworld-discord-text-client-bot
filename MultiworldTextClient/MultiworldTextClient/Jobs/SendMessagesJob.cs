using Quartz;

namespace MultiworldTextClient.Jobs;

public class SendMessagesJob : IJob
{
    public string TrackerUuid { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var tracker = Program.TrackerManagers[TrackerUuid];

        Console.WriteLine($"Sending Messages For {TrackerUuid}...");
        
        await tracker.SendItemMessaages();
    }
}