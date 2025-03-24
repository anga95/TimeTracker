using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.Data;

public class TimeTrackerContext : DbContext
{
    public TimeTrackerContext(DbContextOptions<TimeTrackerContext> options)
    : base(options) { }

    public DbSet<WorkDay> WorkDays { get; set; }
    public DbSet<WorkItem> WorkItems { get; set; }
}
