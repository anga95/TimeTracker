using System;

namespace TimeTracker.Models;

public class WorkItem
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = null!;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Comment { get; set; }

    public double DurationMinutes => (End - Start).TotalMinutes;
}
