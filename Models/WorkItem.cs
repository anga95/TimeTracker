using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Models;

public class WorkItem
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Välj ett projekt")]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int WorkDayId { get; set; }
    public WorkDay? WorkDay { get; set; }
    
    [Range(0.25, 24, ErrorMessage = "Ange ett antal timmar")]
    public double HoursWorked { get; set; }
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string? Comment { get; set; }

    public double DurationMinutes => HoursWorked * 60;

    public string? UserId { get; set; }
}
