using TimeTracker.Models;

namespace TimeTracker.Demo
{
    public static class DemoData
    {
        public static List<Project> GetDemoProjects() => new()
        {
            new Project { Id = 1, Name = "Azure App Service Migration" },
            new Project { Id = 2, Name = ".NET 8 Uppgradering" }
        };

        public static List<WorkDay> GetDemoWorkDays()
        {
            var demoProjects = GetDemoProjects();

            var demoDays = new List<WorkDay>
            {
                // Dag 1 med 3 uppdrag
                new WorkDay
                {
                    Id = 1,
                    Date = DateTime.Today.AddDays(-1),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 1, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 3, WorkDate = DateTime.Today.AddDays(-1), Comment = "Konfigurerade Azure App Service-inställningar för produktion.", UserId = "demo" },
                        new TimeEntry { Id = 2, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2.5, WorkDate = DateTime.Today.AddDays(-1), Comment = "Uppdaterade NuGet-paket för kompatibilitet med .NET 8.", UserId = "demo" },
                        new TimeEntry { Id = 3, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 2.5, WorkDate = DateTime.Today.AddDays(-1), Comment = "Implementerade Azure Key Vault för säker hantering av hemligheter.", UserId = "demo" }
                    }
                },
                // Dag 2 med 2 uppdrag
                new WorkDay
                {
                    Id = 2,
                    Date = DateTime.Today.AddDays(-2),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 4, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-2), Comment = "Skapade Azure DevOps-pipeline för kontinuerlig leverans.", UserId = "demo" },
                        new TimeEntry { Id = 5, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2, WorkDate = DateTime.Today.AddDays(-2), Comment = "Implementerade nya .NET 8 minimal API-funktioner.", UserId = "demo" }
                    }
                },
                // Dag 3 med 2 uppdrag
                new WorkDay
                {
                    Id = 3,
                    Date = DateTime.Today.AddDays(-3),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 6, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 5, WorkDate = DateTime.Today.AddDays(-3), Comment = "Implementerade Azure Application Insights för övervakning.", UserId = "demo" },
                        new TimeEntry { Id = 7, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2, WorkDate = DateTime.Today.AddDays(-3), Comment = "Uppdaterade applikationen till .NET 8 Identity-systemet.", UserId = "demo" }
                    }
                },
                // Dag 4 med 2 uppdrag
                new WorkDay
                {
                    Id = 4,
                    Date = DateTime.Today.AddDays(-4),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 8, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 3.5, WorkDate = DateTime.Today.AddDays(-4), Comment = "Optimerade Azure SQL Database-prestanda med indexeringsstrategier.", UserId = "demo" },
                        new TimeEntry { Id = 9, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 1, WorkDate = DateTime.Today.AddDays(-4), Comment = "Åtgärdade buggar relaterade till .NET 8 nullability-funktioner.", UserId = "demo" }
                    }
                },
                // Dag 5 med 3 uppdrag (Exempel: totalt 8 h)
                new WorkDay
                {
                    Id = 5,
                    Date = DateTime.Today.AddDays(-5),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 10, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-5), Comment = "Konfigurerade Azure Blob Storage för filhantering.", UserId = "demo" },
                        new TimeEntry { Id = 11, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 3, WorkDate = DateTime.Today.AddDays(-5), Comment = "Implementerade .NET 8 native AOT-kompilering för förbättrad prestanda.", UserId = "demo" },
                        new TimeEntry { Id = 12, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 1, WorkDate = DateTime.Today.AddDays(-5), Comment = "Dokumenterade Azure-infrastrukturuppsättning.", UserId = "demo" }
                    }
                },
                // Dag 6 med 2 uppdrag
                new WorkDay
                {
                    Id = 6,
                    Date = DateTime.Today.AddDays(-6),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 13, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-6), Comment = "Testade integrationen med .NET 8:s nya HTTP-klient.", UserId = "demo" },
                        new TimeEntry { Id = 14, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 5, WorkDate = DateTime.Today.AddDays(-6), Comment = "Implementerade Azure Function för schemalagda bakgrundsuppgifter.", UserId = "demo" }
                    }
                },
                // Dag 7 med 2 uppdrag
                new WorkDay
                {
                    Id = 7,
                    Date = DateTime.Today.AddDays(-7),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 15, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4.5, WorkDate = DateTime.Today.AddDays(-7), Comment = "Konfigurerade Azure Front Door för global innehållsdistribution.", UserId = "demo" },
                        new TimeEntry { Id = 16, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 3.5, WorkDate = DateTime.Today.AddDays(-7), Comment = "Implementerade .NET 8:s nya funktioner för API-kontrakt med TypedResults.", UserId = "demo" }
                    }
                }
            };

            // Filtrera bort demo-data för helger
            return demoDays.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).ToList();
        }
    }
}