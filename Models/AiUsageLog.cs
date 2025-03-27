namespace TimeTracker.Models
{
    public class AiUsageLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? PromptSnippet { get; set; }
    }
}
