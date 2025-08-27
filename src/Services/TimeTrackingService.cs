using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Demo;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly IDbContextFactory<TimeTrackerContext> _contextFactory;
        private readonly IAiService _aiService;
        private readonly AiSummaryStateService _summaryState;
        private readonly SafeExecutor _safeExecutor;

        public TimeTrackingService(
            IDbContextFactory<TimeTrackerContext> contextFactory,
            IAiService aiService,
            AiSummaryStateService summaryState,
            SafeExecutor safeExecutor)
        {
            _contextFactory = contextFactory;
            _aiService = aiService;
            _summaryState = summaryState;
            _safeExecutor = safeExecutor;
        }

        public async Task AddTimeEntryAsync(TimeEntry item, string userId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();

                item.UserId = userId;

                var workDay = await context.WorkDays
                    .Include(d => d.TimeEntries)
                    .FirstOrDefaultAsync(d => d.Date.Date == item.WorkDate.Date);

                if (workDay == null)
                {
                    workDay = new WorkDay { Date = item.WorkDate.Date };
                    context.WorkDays.Add(workDay);
                    await context.SaveChangesAsync();
                }

                item.WorkDayId = workDay.Id;
                workDay.TimeEntries.Add(item);

                await context.SaveChangesAsync();
                await InvalidateAiSummaryAsync(userId);
                _summaryState.ClearAiSummary();
            });
        }

        public async Task<List<WorkDay>> GetWorkDaysAsync(string userId)
        {
            return await _safeExecutor.ExecuteListAsync(async () =>
            {
                // if no user id is provided, return demo data
                if (string.IsNullOrEmpty(userId))
                {
                    return DemoData.GetDemoWorkDays();
                }

                // Default to last 90 days to prevent excessive data loading
                var threeMonthsAgo = DateTime.Today.AddDays(-90);
        
                await using var context = _contextFactory.CreateDbContext();
                var filteredDays = await context.WorkDays
                    .Where(d => d.Date >= threeMonthsAgo)
                    .Where(d => d.TimeEntries.Any(wi => wi.UserId == userId))
                    .Include(d => d.TimeEntries.Where(wi => wi.UserId == userId))
                    .ThenInclude(wi => wi.Project)
                    .OrderByDescending(d => d.Date)
                    .ToListAsync();

                return filteredDays;
            });
        }

        public async Task<List<WorkDay>> GetWorkDaysForMonthAsync(string userId, int year, int month)
        {
            return await _safeExecutor.ExecuteListAsync(async () =>
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var demo = DemoData.GetDemoWorkDays();
                    return demo.Where(d => d.Date.Year == year && d.Date.Month == month).ToList();
                }

                var firstDay = new DateTime(year, month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                await using var context = _contextFactory.CreateDbContext();
                var filteredDays = await context.WorkDays
                    .Where(d => d.Date >= firstDay && d.Date <= lastDay)
                    .Where(d => d.TimeEntries.Any(wi => wi.UserId == userId))
                    .Include(d => d.TimeEntries.Where(wi => wi.UserId == userId))
                    .ThenInclude(wi => wi.Project)
                    .OrderByDescending(d => d.Date)
                    .ToListAsync();

                return filteredDays;
            });
        }
        
        public async Task<List<WorkDay>> GetWorkDaysForLastNDaysAsync(string userId, int days)
        {
            return await _safeExecutor.ExecuteListAsync(async () =>
            {
                // If no user id is provided, return demo data
                DateTime cutoff = DateTime.Today.AddDays(-days);
                if (string.IsNullOrEmpty(userId))
                {
                    var demo = DemoData.GetDemoWorkDays();
                    return demo.Where(d => d.Date >= cutoff).ToList();
                }

                await using var context = _contextFactory.CreateDbContext();
                var filteredDays = await context.WorkDays
                    .Where(d => d.Date >= cutoff)
                    .Where(d => d.TimeEntries.Any(wi => wi.UserId == userId))
                    .Include(d => d.TimeEntries.Where(wi => wi.UserId == userId))
                    .ThenInclude(wi => wi.Project)
                    .OrderByDescending(d => d.Date)
                    .ToListAsync();

                return filteredDays;
            });
        }

        public async Task ArchiveProjectAsync(int projectId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();
                
                var project = await context.Projects.FirstOrDefaultAsync( p => p.Id == projectId);
                if (project == null) return;
                if (project.IsArchived) return;
                
                project.IsArchived = true;
                await context.SaveChangesAsync();
            });
        }

        public async Task UnarchiveProjectAsync(int projectId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();
                var project = await context.Projects.FirstOrDefaultAsync( p => p.Id == projectId);
                if (project is null) return;
                project.IsArchived = false;
                await context.SaveChangesAsync();
            });
        }

        public async Task<double> GetRoundedDailyTotalAsync(DateTime date, string userId)
        {
            return await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();

                var day = await context.WorkDays
                    .Include(d => d.TimeEntries)
                    .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

                if (day == null) return 0;

                var totalMinutes = day.TimeEntries
                    .Where(w => w.UserId == userId)
                    .Sum(w => w.DurationMinutes);
                var rounded = Math.Ceiling(totalMinutes / 30) * 30;

                return rounded / 60; // timmar
            },
                fallback: static () => 0d);
        }

        public async Task<List<Project>> GetProjectsAsync(string userId)
        {
            return await _safeExecutor.ExecuteListAsync(async () =>
            {
                // if no user id is provided, return demo data
                if (string.IsNullOrEmpty(userId))
                {
                    return DemoData.GetDemoProjects();
                }

                await using var context = _contextFactory.CreateDbContext();
                return await context.Projects
                    .Where( p => !p.IsArchived)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            });
        }

        public async Task CreateProjectAsync(string projectName, string userId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();
                var project = new Project { Name = projectName };
                context.Projects.Add(project);
                await context.SaveChangesAsync();
            });
        }

        public async Task DeleteProjectAsync(int projectId)
        {
            await ArchiveProjectAsync(projectId);
        }

        public async Task DeleteTimeEntryAsync(int TimeEntryId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var context = _contextFactory.CreateDbContext();

                var item = await context.TimeEntries
                    .Include(wi => wi.WorkDay)
                    .FirstOrDefaultAsync(x => x.Id == TimeEntryId);
                if (item == null)
                    return;

                context.TimeEntries
                    .Remove(item);
                await context.SaveChangesAsync();

                var day = item.WorkDay;
                if (day != null)
                {
                    var updatedDay = await context.WorkDays
                        .Include(d => d.TimeEntries)
                        .FirstOrDefaultAsync(d => d.Id == day.Id);
                    if (updatedDay != null && !updatedDay.TimeEntries.Any())
                    {
                        context.WorkDays.Remove(updatedDay);
                        await context.SaveChangesAsync();
                        if (!string.IsNullOrWhiteSpace(item.UserId))
                        {
                            await InvalidateAiSummaryAsync(item.UserId);
                        }
                    }
                }
            });
        }

        private async Task InvalidateAiSummaryAsync(string userId)
        {

            try
            {
                await _aiService.ClearCachedSummaryAsync(userId);
            }
            catch (Exception)
            {
                // Ignore errors when clearing the cache
                // This is a best-effort operation and should not affect the main functionality
            }
        }
    }
}