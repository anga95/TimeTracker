namespace TimeTracker.Models
{
    public class ChatResponseResult
    {
        public bool IsRateLimited { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
