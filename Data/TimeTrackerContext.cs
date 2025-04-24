using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Models;

namespace TimeTracker.Data;

public class TimeTrackerContext : IdentityDbContext
{
    public TimeTrackerContext(DbContextOptions<TimeTrackerContext> options)
    : base(options) { }

    public DbSet<WorkDay> WorkDays { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<AiUsageLog> AiUsageLogs { get; set; }
    
    public DbSet<AiSummary> AiSummaries { get; set; }
}
