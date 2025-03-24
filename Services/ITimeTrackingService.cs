using TimeTracker.Models;

namespace TimeTracker.Services;

public interface ITimeTrackingService
{
    Task AddWorkItemAsync(WorkItem item);
    Task<List<WorkDay>> GetWorkDaysAsync();
    Task<double> GetRoundedDailyTotalAsync(DateTime date);
    Task<List<Project>> GetProjectsAsync();
    Task CreateProjectAsync(string projectName);
    Task DeleteWorkItemAsync(int workItemId);
}
