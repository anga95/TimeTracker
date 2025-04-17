using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IAIService
    {
        Task<ChatResponseResult> GetChatResponseAsync(string prompt, bool creative = true);
        Task<(int monthlyCalls, int maxCalls)> GetUsageInfoAsync();
        Task<string?> GetCachedSummaryAsync(string userId);
        Task SaveOrUpdateSummaryAsync(string userId, string summary);
        Task ClearCachedSummaryAsync(string userId);
    }
}
