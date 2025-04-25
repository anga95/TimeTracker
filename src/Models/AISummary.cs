namespace TimeTracker.Models;

public class AiSummary
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Summary { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
}