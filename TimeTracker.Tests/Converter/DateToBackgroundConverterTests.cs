using Moq;
using System.Globalization;
using TimeTracker.Converters;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Tests.Converters
{
    [TestFixture]
    public class DateToBackgroundConverterTests
    {
        private DateToBackgroundConverter converter;
        private Mock<IDataService> mockDataService;

        [SetUp]
        public void SetUp()
        {
            // Skapa en mock för IDataService
            mockDataService = new Mock<IDataService>();

            // Skapa konverteraren och tilldela mockad tjänst
            converter = new DateToBackgroundConverter
            {
                DataService = mockDataService.Object
            };
        }

        [Test]
        public void Convert_ShouldReturnTrue_WhenHoursAreGreaterThanOrEqualTo8()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            var entries = new[]
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 4, Comments = "Worked on feature A" },
                new TimeLogEntry { ProjectName = "Project B", HoursWorked = 5, Comments = "Bug fixes" }
            }.ToList();

            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(entries);

            // Act
            var result = converter.Convert(date, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.True, "Konverteraren borde returnera true för >= 8 timmar.");
        }

        [Test]
        public void Convert_ShouldReturnFalse_WhenHoursAreLessThan8()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            var entries = new[]
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 4, Comments = "Worked on feature A" }
            }.ToList();

            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(entries);

            // Act
            var result = converter.Convert(date, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.False, "Konverteraren borde returnera false för < 8 timmar.");
        }

        [Test]
        public void Convert_ShouldReturnFalse_WhenNoEntriesExist()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(new List<TimeLogEntry>());

            // Act
            var result = converter.Convert(date, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.False, "Konverteraren borde returnera false för tomma loggar.");
        }

        [Test]
        public void Convert_ShouldReturnFalse_WhenDateIsInvalid()
        {
            // Act
            var result = converter.Convert(null, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.False, "Konverteraren borde returnera false för null-värde.");
        }

        [Test]
        public void Convert_ShouldReturnFalse_WhenDataServiceIsNull()
        {
            // Arrange
            converter.DataService = null;
            var date = new DateTime(2025, 1, 21);

            // Act
            var result = converter.Convert(date, null, null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.False, "Konverteraren borde returnera false om DataService är null.");
        }
    }
}
