using System.Collections.Generic;
using System;

namespace TimeTracker.Models;

public class WorkDay
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public List<WorkItem> WorkItems { get; set; } = new();
}
