namespace TimeTracker.Models;
public class WorkItem
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = null!;
    public double HoursWorked { get; set; }  // Ny egenskap f�r inmatade timmar
    public string? Comment { get; set; }

    // Om du inte l�ngre anv�nder Start/End f�r att ber�kna arbetade timmar, kan du antingen ta bort dem
    // eller beh�lla dem f�r historik. Exempel:
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // Om du enbart vill anv�nda HoursWorked, kan du ta bort DurationMinutes eller
    // �ndra den att baseras p� HoursWorked ist�llet:
    public double DurationMinutes => HoursWorked * 60;
}

