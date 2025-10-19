using System.ComponentModel.DataAnnotations;

namespace MultiworldTextClient.Data.Database;

public class TrackedWorld
{
    [Key]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string BaseUrl { get; set; }
    public string TrackerUuid { get; set; }
    public string RoomUuid { get; set; }
}