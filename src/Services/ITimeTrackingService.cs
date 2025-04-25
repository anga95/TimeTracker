using TimeTracker.Models;

namespace TimeTracker.Services;

public interface ITimeTrackingService
{
    Task AddTimeEntryAsync(TimeEntry item, string userId);
    Task<List<WorkDay>> GetWorkDaysAsync(string userId);
    Task<double> GetRoundedDailyTotalAsync(DateTime date, string userId);
    Task<List<Project>> GetProjectsAsync(string userId);
    Task<List<WorkDay>> GetWorkDaysForMonthAsync(string userId, int year, int month);
    Task<List<WorkDay>> GetWorkDaysForLastNDaysAsync(string userId, int days);


    Task CreateProjectAsync(string projectName, string userId);
    Task DeleteProjectAsync(int projectId);
    Task DeleteTimeEntryAsync(int timeEntryId);
}
