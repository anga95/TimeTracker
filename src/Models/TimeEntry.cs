using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTracker.Models;

public class TimeEntry
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Välj ett projekt")]
    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int WorkDayId { get; set; }
    public WorkDay? WorkDay { get; set; }
    
    [Range(0.25, 24, ErrorMessage = "Ange ett antal timmar")]
    public double HoursWorked { get; set; }
    
    [Column(TypeName = "date")]
    public DateTime WorkDate { get; set; } = DateTime.Today;
    public string? Comment { get; set; }
    
    [MaxLength(64)]
    public string? TicketKey { get; set; }
    [MaxLength(512)]
    public string? TicketUrl { get; set; }
    
    [Column(TypeName = "datetimeoffset(7)")]
    public DateTimeOffset LoggedAt { get; set; } = DateTimeOffset.UtcNow;

    public double DurationMinutes => HoursWorked * 60;

    public string? UserId { get; set; }
}
