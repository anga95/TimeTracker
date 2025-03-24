using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly IDbContextFactory<TimeTrackerContext> _contextFactory;

    public TimeTrackingService(IDbContextFactory<TimeTrackerContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddWorkItemAsync(WorkItem item)
    {
        await using var context = _contextFactory.CreateDbContext();

        var workDay = await context.WorkDays
            .Include(d => d.WorkItems)
            .FirstOrDefaultAsync(d => d.Date.Date == item.Start.Date);

        if (workDay == null)
        {
            workDay = new WorkDay { Date = item.Start.Date };
            context.WorkDays.Add(workDay);
        }

        workDay.WorkItems.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task<List<WorkDay>> GetWorkDaysAsync()
    {
        await using var context = _contextFactory.CreateDbContext();

        return await context.WorkDays
            .Include(d => d.WorkItems)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<double> GetRoundedDailyTotalAsync(DateTime date)
    {
        await using var context = _contextFactory.CreateDbContext();

        var day = await context.WorkDays
            .Include(d => d.WorkItems)
            .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

        if (day == null) return 0;

        var totalMinutes = day.WorkItems.Sum(i => i.DurationMinutes);
        var rounded = Math.Ceiling(totalMinutes / 30) * 30;

        return rounded / 60; // Timmar
    }

}
