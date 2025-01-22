namespace TimeTracker.Models
{
    public class TimeLogEntry
    {
        public required string ProjectName { get; set; }
        public double HoursWorked { get; set; }
        public required string Comments { get; set; }
    }
}
