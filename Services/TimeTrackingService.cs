using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Demo;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class TimeTrackingService : ITimeTrackingService
    {
        private readonly IDbContextFactory<TimeTrackerContext> _contextFactory;

        public TimeTrackingService(IDbContextFactory<TimeTrackerContext> contextFactory)
        {
            _contextFactory = contextFactory;
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
        }

        public async Task<List<WorkDay>> GetWorkDaysAsync(string userId)
        {
            // if no user id is provided, return demo data
            if (string.IsNullOrEmpty(userId))
            {
                return DemoData.GetDemoWorkDays();
            }

            await using var context = _contextFactory.CreateDbContext();
            var allDays = await context.WorkDays
                .Include(d => d.WorkItems)
                    .ThenInclude(wi => wi.Project)
                .OrderByDescending(d => d.Date)
                .ToListAsync();

            var filteredDays = allDays
                .Select(day =>
                {
                    day.WorkItems = day.WorkItems.Where(w => w.UserId == userId).ToList();
                    return day;
                })
                .Where(day => day.WorkItems.Any())
                .ToList();

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
                }
            }
        }
    }
}
