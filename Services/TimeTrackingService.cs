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

        // Hitta (eller skapa) en WorkDay baserat på item.WorkDate
        var workDay = await context.WorkDays
            .Include(d => d.WorkItems)
            .FirstOrDefaultAsync(d => d.Date.Date == item.WorkDate.Date);

        if (workDay == null)
        {
            workDay = new WorkDay { Date = item.WorkDate.Date };
            context.WorkDays.Add(workDay);
            await context.SaveChangesAsync();
        }

        // Koppla WorkItem till WorkDay
        item.WorkDayId = workDay.Id;

        // Lägg till WorkItem i den listan
        workDay.WorkItems.Add(item);

        await context.SaveChangesAsync();
    }




    public async Task<List<WorkDay>> GetWorkDaysAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.WorkDays
            .Include(d => d.WorkItems)
                .ThenInclude(wi => wi.Project)
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

    public async Task<List<Project>> GetProjectsAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Projects
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task CreateProjectAsync(string projectName)
    {
        await using var context = _contextFactory.CreateDbContext();
        var project = new Project { Name = projectName };
        context.Projects.Add(project);
        await context.SaveChangesAsync();
    }

    public async Task DeleteWorkItemAsync(int workItemId)
    {
        await using var context = _contextFactory.CreateDbContext();

        // Ladda item + dess WorkDay
        var item = await context.WorkItems
            .Include(wi => wi.WorkDay)
            .FirstOrDefaultAsync(x => x.Id == workItemId);

        if (item == null) return;

        // Ta bort item
        context.WorkItems.Remove(item);
        await context.SaveChangesAsync();

        // Kolla om dagen är tom
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
