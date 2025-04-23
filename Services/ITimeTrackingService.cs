using TimeTracker.Models;

namespace TimeTracker.Services;

public interface ITimeTrackingService
{
    Task AddWorkItemAsync(WorkItem item, string userId);
    Task<List<WorkDay>> GetWorkDaysAsync(string userId);
    Task<double> GetRoundedDailyTotalAsync(DateTime date, string userId);
    Task<List<Project>> GetProjectsAsync(string userId);
    Task CreateProjectAsync(string projectName, string userId);
    Task DeleteProjectAsync(int projectId);
    Task DeleteWorkItemAsync(int workItemId);
}
