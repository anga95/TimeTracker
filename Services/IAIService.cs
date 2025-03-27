using TimeTracker.Models;

namespace TimeTracker.Services
{
    public interface IAIService
    {
        Task<ChatResponseResult> GetChatResponseAsync(string prompt, bool creative = true);
        Task<(int monthlyCalls, int maxCalls)> GetUsageInfoAsync();

    }
}
