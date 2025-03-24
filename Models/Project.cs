namespace TimeTracker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Navigation property - en lista av WorkItems som hör till projektet
    public List<WorkItem> WorkItems { get; set; } = new();
}
