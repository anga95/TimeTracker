using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Tests
{
    [TestFixture]
    public class DataServiceTests
    {
        private string testDirectory;
        private DataServiceWrapper dataService;
        private DateTime date = new DateTime(2025, 1, 21);
        private List<TimeLogEntry> entries;

        [SetUp]
        public void SetUp()
        {
            // Skapa en temporär katalog för testfiler
            testDirectory = Path.Combine(Path.GetTempPath(), "TimeTrackerTests");
            if (!Directory.Exists(testDirectory))
            {
                Directory.CreateDirectory(testDirectory);
            }

            // Skapa några testdata
            entries = new List<TimeLogEntry>
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 4, Comments = "Worked on feature A" },
                new TimeLogEntry { ProjectName = "Project B", HoursWorked = 5, Comments = "Bug fixes" }
            };

            // Injicera en "custom" DataService som använder testkatalogen
            dataService = new DataServiceWrapper(testDirectory);
        }

        [Test]
        public void SaveTimeLogEntries_ShouldCreateJsonFile()
        {
            // needed Arrange is done in setup

            // Act
            dataService.SaveTimeLogEntries(date, entries);

            // Assert
            var filePath = Path.Combine(testDirectory, "2025-01.json");
            Assert.That(File.Exists(filePath), Is.True, "JSON-filen skapades inte.");

            var jsonContent = File.ReadAllText(filePath);
            Assert.That(jsonContent, Does.Contain("Project A"));
            Assert.That(jsonContent, Does.Contain("Project B"));
        }

        [Test]
        public void LoadTimeLogEntries_ShouldReturnCorrectData()
        {
            // Arrange
            dataService.SaveTimeLogEntries(date, entries);

            // Act
            var loadedEntries = dataService.LoadTimeLogEntries(date);

            // Assert
            Assert.That(loadedEntries, Has.Count.EqualTo(entries.Count()));

            Assert.That(loadedEntries[0].ProjectName, Is.EqualTo(entries[0].ProjectName));
            Assert.That(loadedEntries[0].HoursWorked, Is.EqualTo(entries[0].HoursWorked));
            Assert.That(loadedEntries[0].Comments, Is.EqualTo(entries[0].Comments));

            Assert.That(loadedEntries[1].ProjectName, Is.EqualTo(entries[1].ProjectName));
            Assert.That(loadedEntries[1].HoursWorked, Is.EqualTo(entries[1].HoursWorked));
            Assert.That(loadedEntries[1].Comments, Is.EqualTo(entries[1].Comments));
        }

        [Test]
        public void LoadTimeLogEntries_ShouldReturnEmptyList_WhenNoEntriesExist()
        {
            // Arrange

            // Act
            var emptyEntries = dataService.LoadTimeLogEntries(date);

            // Assert
            Assert.That(emptyEntries, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            // Radera temporära filer efter testerna
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        /// <summary>
        /// Wrapper för att injicera en custom dataDirectory i DataService
        /// </summary>
        private class DataServiceWrapper : DataService
        {
            private readonly string customDirectory;

            public DataServiceWrapper(string directory)
            {
                customDirectory = directory;
            }

            protected override string GetFilePath(DateTime date)
            {
                return Path.Combine(customDirectory, $"{date:yyyy-MM}.json");
            }
        }
    }
}
