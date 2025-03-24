namespace TimeTracker.Models;
public class WorkItem
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = null!;
    public double HoursWorked { get; set; }  // Ny egenskap för inmatade timmar
    public string? Comment { get; set; }

    // Om du inte längre använder Start/End för att beräkna arbetade timmar, kan du antingen ta bort dem
    // eller behålla dem för historik. Exempel:
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // Om du enbart vill använda HoursWorked, kan du ta bort DurationMinutes eller
    // ändra den att baseras på HoursWorked istället:
    public double DurationMinutes => HoursWorked * 60;
}

