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

        public TimeTrackingService(
            IDbContextFactory<TimeTrackerContext> contextFactory,
            IAiService aiService,
            AiSummaryStateService summaryState)
        {
            _contextFactory = contextFactory;
            _aiService = aiService;
            _summaryState = summaryState;
        }

        public async Task AddWorkItemAsync(WorkItem item, string userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            item.UserId = userId;

            var workDay = await context.WorkDays
                .Include(d => d.WorkItems)
                .FirstOrDefaultAsync(d => d.Date.Date == item.WorkDate.Date);

            if (workDay == null)
            {
                workDay = new WorkDay { Date = item.WorkDate.Date };
                context.WorkDays.Add(workDay);
                await context.SaveChangesAsync();
            }

            item.WorkDayId = workDay.Id;
            workDay.WorkItems.Add(item);

            await context.SaveChangesAsync();
            await InvalidateAiSummaryAsync(userId);
            _summaryState.ClearAiSummary();
        }

        public async Task<List<WorkDay>> GetWorkDaysAsync(string userId)
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
                .Where(d => d.WorkItems.Any(wi => wi.UserId == userId))
                .Include(d => d.WorkItems.Where(wi => wi.UserId == userId))
                .ThenInclude(wi => wi.Project)
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            return filteredDays;
        }


        public async Task<List<WorkDay>> GetWorkDaysForMonthAsync(string userId, int year, int month)
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
                .Where(d => d.WorkItems.Any(wi => wi.UserId == userId))
                .Include(d => d.WorkItems.Where(wi => wi.UserId == userId))
                .ThenInclude(wi => wi.Project)
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            return filteredDays;
        }
        
        public async Task<List<WorkDay>> GetWorkDaysForLastNDaysAsync(string userId, int days)
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
                .Where(d => d.WorkItems.Any(wi => wi.UserId == userId))
                .Include(d => d.WorkItems.Where(wi => wi.UserId == userId))
                .ThenInclude(wi => wi.Project)
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            return filteredDays;
        }

        public async Task<double> GetRoundedDailyTotalAsync(DateTime date, string userId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var day = await context.WorkDays
                .Include(d => d.WorkItems)
                .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

            if (day == null) return 0;

            var totalMinutes = day.WorkItems
                .Where(w => w.UserId == userId)
                .Sum(w => w.DurationMinutes);
            var rounded = Math.Ceiling(totalMinutes / 30) * 30;

            return rounded / 60; // timmar
        }

        public async Task<List<Project>> GetProjectsAsync(string userId)
        {
            // if no user id is provided, return demo data
            if (string.IsNullOrEmpty(userId))
            {
                return DemoData.GetDemoProjects();
            }

            await using var context = _contextFactory.CreateDbContext();
            return await context.Projects
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task CreateProjectAsync(string projectName, string userId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var project = new Project { Name = projectName };
            context.Projects.Add(project);
            await context.SaveChangesAsync();
        }

        // Create a method to Delete Projects Async
        public async Task DeleteProjectAsync(int projectId)
        {
            await using var context = _contextFactory.CreateDbContext();
            var project = await context.Projects
                .Include(p => p.WorkItems)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
                return;

            context.Projects.Remove(project);
            await context.SaveChangesAsync();
        }

        public async Task DeleteWorkItemAsync(int workItemId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var item = await context.WorkItems
                .Include(wi => wi.WorkDay)
                .FirstOrDefaultAsync(x => x.Id == workItemId);
            if (item == null)
                return;

            context.WorkItems.Remove(item);
            await context.SaveChangesAsync();

            var day = item.WorkDay;
            if (day != null)
            {
                var updatedDay = await context.WorkDays
                    .Include(d => d.WorkItems)
                    .FirstOrDefaultAsync(d => d.Id == day.Id);
                if (updatedDay != null && !updatedDay.WorkItems.Any())
                {
                    context.WorkDays.Remove(updatedDay);
                    await context.SaveChangesAsync();
                    if (!string.IsNullOrWhiteSpace(item.UserId))
                    {
                        await InvalidateAiSummaryAsync(item.UserId);
                    }
                }
            }
        }

        private async Task InvalidateAiSummaryAsync(string userId)
        {
            await _aiService.ClearCachedSummaryAsync(userId);
        }

        
}
}