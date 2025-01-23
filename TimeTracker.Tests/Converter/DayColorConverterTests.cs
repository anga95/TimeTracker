using Moq;
using System.Globalization;
using System.Windows.Media;
using TimeTracker.Converters;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Tests.Converters
{
    [TestFixture]
    public class DayColorConverterTests
    {
        private DayColorConverter converter;
        private Mock<IDataService> mockDataService;

        [SetUp]
        public void SetUp()
        {
            // Skapa en mock för IDataService
            mockDataService = new Mock<IDataService>();
            // Skapa konverteraren och tilldela mockad tjänst
            converter = new DayColorConverter
            {
                DataService = mockDataService.Object
            };
        }

        [Test]
        public void Convert_ShouldReturnLightGreen_WhenHoursAreGreaterThanOrEqualTo8()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            var entries = new[]
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 5, Comments = "Feature A" },
                new TimeLogEntry { ProjectName = "Project B", HoursWorked = 3, Comments = "Bug fixes" }
            }.ToList();
            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(entries);

            var values = new object[] { "21", new DateTime(2025, 1, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.LightGreen), "Konverteraren borde returnera LightGreen för >= 8 timmar.");
        }

        [Test]
        public void Convert_ShouldReturnTransparent_WhenHoursAreLessThan8()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            var entries = new[]
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 4, Comments = "Feature A" }
            }.ToList();
            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(entries);

            var values = new object[] { "21", new DateTime(2025, 1, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.Transparent), "Konverteraren borde returnera Transparent för < 8 timmar.");
        }

        [Test]
        public void Convert_ShouldReturnTransparent_WhenNoEntriesExist()
        {
            // Arrange
            var date = new DateTime(2025, 1, 21);
            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(new List<TimeLogEntry>());

            var values = new object[] { "21", new DateTime(2025, 1, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.Transparent), "Konverteraren borde returnera Transparent för tomma loggar.");
        }

        [Test]
        public void Convert_ShouldReturnTransparent_WhenDataServiceIsNull()
        {
            // Arrange
            converter.DataService = null;
            var values = new object[] { "21", new DateTime(2025, 1, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.Transparent), "Konverteraren borde returnera Transparent om DataService är null.");
        }

        [Test]
        public void Convert_ShouldReturnTransparent_WhenDayIsInvalid()
        {
            // Arrange
            var values = new object[] { "invalid", new DateTime(2025, 1, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.Transparent), "Konverteraren borde returnera Transparent för ogiltig dag.");
        }

        [Test]
        public void Convert_ShouldReturnLightGreen_WhenDayIsValidAndInAnotherMonth()
        {
            // Arrange
            var date = new DateTime(2025, 2, 1); // 1 februari
            var entries = new[]
            {
                new TimeLogEntry { ProjectName = "Project A", HoursWorked = 8, Comments = "Feature A" }
            }.ToList();
            mockDataService.Setup(ds => ds.LoadTimeLogEntries(date)).Returns(entries);

            var values = new object[] { "1", new DateTime(2025, 2, 1) };

            // Act
            var result = converter.Convert(values, typeof(Brush), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.That(result, Is.EqualTo(Brushes.LightGreen), "Konverteraren borde returnera LightGreen för >= 8 timmar på en giltig dag i annan månad.");
        }
    }
}
