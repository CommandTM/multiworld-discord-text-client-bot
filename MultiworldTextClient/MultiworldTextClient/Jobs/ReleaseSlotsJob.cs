using Quartz;

namespace MultiworldTextClient.Jobs;

public class ReleaseSlotsJob : IJob
{
    public string TrackerUuid { get; set; }
    public double Percentage { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var tracker = Program.TrackerManagers[TrackerUuid];

        Console.WriteLine($"Checking For {TrackerUuid} Releases...");
        
        await tracker.SendGoaledSlotsForPercentage(Percentage);
    }
}