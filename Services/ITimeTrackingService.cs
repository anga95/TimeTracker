using TimeTracker.Models;

namespace TimeTracker.Services;

public interface ITimeTrackingService
{
    Task AddWorkItemAsync(WorkItem item);
    Task<List<WorkDay>> GetWorkDaysAsync();
    Task<double> GetRoundedDailyTotalAsync(DateTime date);
}
