using Microsoft.EntityFrameworkCore;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services;

public class TimeTrackingService : ITimeTrackingService
{
    private readonly TimeTrackerContext _context;

    public TimeTrackingService(TimeTrackerContext context)
    {
        _context = context;
    }

    public async Task AddWorkItemAsync(WorkItem item)
    {
        var workDay = await _context.WorkDays
            .Include(d => d.WorkItems)
            .FirstOrDefaultAsync(d => d.Date.Date == item.Start.Date);

        if (workDay == null)
        {
            workDay = new WorkDay { Date = item.Start.Date };
            _context.WorkDays.Add(workDay);
        }

        workDay.WorkItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WorkDay>> GetWorkDaysAsync()
    {
        return await _context.WorkDays
            .Include(d => d.WorkItems)
            .OrderByDescending(d => d.Date)
            .ToListAsync();
    }

    public async Task<double> GetRoundedDailyTotalAsync(DateTime date)
    {
        var day = await _context.WorkDays
            .Include(d => d.WorkItems)
            .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

        if (day == null) return 0;

        var totalMinutes = day.WorkItems.Sum(i => i.DurationMinutes);
        var rounded = Math.Ceiling(totalMinutes / 30) * 30;

        return rounded / 60; // Timmar
    }
}
