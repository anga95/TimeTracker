namespace TimeTracker.Models;

public class WorkItem
{
    public int Id { get; set; }

    public int WorkDayId { get; set; }
    public WorkDay? WorkDay { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public double HoursWorked { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string? Comment { get; set; }

    public double DurationMinutes => HoursWorked * 60;
}
