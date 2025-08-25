namespace TimeTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<TimeEntry> TimeEntries { get; set; } = new();
    
    // Soft delete flag
    public bool IsArchived { get; set; } = false;
}
