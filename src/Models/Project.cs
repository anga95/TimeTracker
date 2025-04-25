namespace TimeTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Navigation property - en lista av TimeEntries som hör till projektet
    public List<TimeEntry> TimeEntries { get; set; } = new();
}
