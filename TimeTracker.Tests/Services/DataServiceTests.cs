using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Tests
{
    [TestFixture]
    public class DataServiceTests
    {
        private string testDirectory;
        private IDataService dataService;

        [SetUp]
        public void SetUp()
        {
            // Skapa en temporär katalog för testfiler
            testDirectory = Path.Combine(Path.GetTempPath(), "TimeTrackerTests");
            if (!Directory.Exists(testDirectory))
            {
                Directory.CreateDirectory(testDirectory);
            }

            // Injicera en "custom" DataService som använder testkatalogen
            dataService = new DataServiceWrapper(testDirectory);
        }

        [Test]
        public void SaveTimeLogEntries_ShouldCreateJsonFile()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            var entries = new List<TimeLogEntry>
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 4, Comments = "Worked on feature A" },
                new TimeLogEntry { ProjectName = "Project B", HoursWorked = 5, Comments = "Bug fixes" }
            };

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
            var date = new DateTime(2025, 1, 21);
            var entries = new List<TimeLogEntry>
            {
                new TimeLogEntry { ProjectName = "Project C", HoursWorked = 8, Comments = "Full day work" }
            };
            dataService.SaveTimeLogEntries(date, entries);

            // Act
            var loadedEntries = dataService.LoadTimeLogEntries(date);

            // Assert
            Assert.That(loadedEntries, Has.Count.EqualTo(1));
            Assert.That(loadedEntries[0].ProjectName, Is.EqualTo("Project C"));
            Assert.That(loadedEntries[0].HoursWorked, Is.EqualTo(8));
            Assert.That(loadedEntries[0].Comments, Is.EqualTo("Full day work"));
        }

        [Test]
        public void LoadTimeLogEntries_ShouldReturnEmptyList_WhenNoEntriesExist()
        {
            // Arrange
            var date = new DateTime(2025, 2, 1);

            // Act
            var entries = dataService.LoadTimeLogEntries(date);

            // Assert
            Assert.That(entries, Is.Empty);
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
