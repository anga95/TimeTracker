namespace TimeTracker.Models;

public class WorkDay
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<TimeEntry> TimeEntries { get; set; } = new();
}