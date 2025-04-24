using TimeTracker.Models;

namespace TimeTracker.Demo
{
    public static class DemoData
    {
        public static List<Project> GetDemoProjects() => new()
        {
            new Project { Id = 1, Name = "Demo Projekt 1" },
            new Project { Id = 2, Name = "Demo Projekt 2" }
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
                        new TimeEntry { Id = 1, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 3, WorkDate = DateTime.Today.AddDays(-1), Comment = "Designade en futuristisk användarupplevelse med neonfärger.", UserId = "demo" },
                        new TimeEntry { Id = 2, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2.5, WorkDate = DateTime.Today.AddDays(-1), Comment = "Testade en AI-assistent som pratade med enhörningar.", UserId = "demo" },
                        new TimeEntry { Id = 3, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 2.5, WorkDate = DateTime.Today.AddDays(-1), Comment = "Skrev en poetisk dokumentation.", UserId = "demo" }
                    }
                },
                // Dag 2 med 2 uppdrag
                new WorkDay
                {
                    Id = 2,
                    Date = DateTime.Today.AddDays(-2),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 4, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-2), Comment = "Experimenterade med hologram och 3D-effekter.", UserId = "demo" },
                        new TimeEntry { Id = 5, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2, WorkDate = DateTime.Today.AddDays(-2), Comment = "Införde interaktiva easter eggs.", UserId = "demo" }
                    }
                },
                // Dag 3 med 2 uppdrag
                new WorkDay
                {
                    Id = 3,
                    Date = DateTime.Today.AddDays(-3),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 6, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 5, WorkDate = DateTime.Today.AddDays(-3), Comment = "Kodade en spelifierad tidrapportering med poängsystem.", UserId = "demo" },
                        new TimeEntry { Id = 7, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 2, WorkDate = DateTime.Today.AddDays(-3), Comment = "Skapade en AI-genererad kommentar med oväntad twist.", UserId = "demo" }
                    }
                },
                // Dag 4 med 2 uppdrag
                new WorkDay
                {
                    Id = 4,
                    Date = DateTime.Today.AddDays(-4),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 8, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 3.5, WorkDate = DateTime.Today.AddDays(-4), Comment = "Optimerade prestanda med magiska kodtrick.", UserId = "demo" },
                        new TimeEntry { Id = 9, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 1, WorkDate = DateTime.Today.AddDays(-4), Comment = "Fixade små buggar med humor.", UserId = "demo" }
                    }
                },
                // Dag 5 med 3 uppdrag (Exempel: totalt 8 h)
                new WorkDay
                {
                    Id = 5,
                    Date = DateTime.Today.AddDays(-5),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 10, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-5), Comment = "Experimenterade med UI-koncept inspirerade av naturen.", UserId = "demo" },
                        new TimeEntry { Id = 11, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 3, WorkDate = DateTime.Today.AddDays(-5), Comment = "Implementerade en spännande backend-funktion.", UserId = "demo" },
                        new TimeEntry { Id = 12, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 1, WorkDate = DateTime.Today.AddDays(-5), Comment = "Skrev en episk commit-historia.", UserId = "demo" }
                    }
                },
                // Dag 6 med 2 uppdrag
                new WorkDay
                {
                    Id = 6,
                    Date = DateTime.Today.AddDays(-6),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 13, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 4, WorkDate = DateTime.Today.AddDays(-6), Comment = "Testade integration med en fiktiv rymdstation.", UserId = "demo" },
                        new TimeEntry { Id = 14, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 5, WorkDate = DateTime.Today.AddDays(-6), Comment = "Kodade om tankar till text.", UserId = "demo" }
                    }
                },
                // Dag 7 med 2 uppdrag
                new WorkDay
                {
                    Id = 7,
                    Date = DateTime.Today.AddDays(-7),
                    TimeEntries = new List<TimeEntry>
                    {
                        new TimeEntry { Id = 15, ProjectId = 1, Project = demoProjects.First(), HoursWorked = 4.5, WorkDate = DateTime.Today.AddDays(-7), Comment = "Skapade en interaktiv demo med animerade grafer.", UserId = "demo" },
                        new TimeEntry { Id = 16, ProjectId = 2, Project = demoProjects.Last(), HoursWorked = 3.5, WorkDate = DateTime.Today.AddDays(-7), Comment = "Förtrollade systemet med magiska AI-funktioner.", UserId = "demo" }
                    }
                }
            };

            // Filtrera bort demo-data för helger
            return demoDays.Where(day => day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday).ToList();
        }
    }
}
