namespace TimeTracker.Services
{
    public interface IAIService
    {
        Task<string> GetSummaryAsync(string prompt);
    }
}
