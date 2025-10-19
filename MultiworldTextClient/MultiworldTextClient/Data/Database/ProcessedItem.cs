using System.ComponentModel.DataAnnotations;

namespace MultiworldTextClient.Data.Database;

public class ProcessedItem
{
    [Key]
    public int Id { get; set; }
    public string TrackerUuid { get; set; }
    public long ItemId { get; set; }
    public long LocationId { get; set; }
}