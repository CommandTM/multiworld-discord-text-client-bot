using System.ComponentModel.DataAnnotations;

namespace MultiworldTextClient.Data.Database;

public class Request
{
    [Key]
    public int Id { get; set; }
    public string SlotName { get; set; }
    public long LocationId { get; set; }
}