namespace TimeTracker.Models
{
    public class TimeLogEntry
    {
        public string ProjectName { get; set; }
        public double HoursWorked { get; set; }
        public string Comments { get; set; }
        public bool IsComplete { get; set; }
    }
}
