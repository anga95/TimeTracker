using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.Tests.ViewModels
{
    [TestFixture]
    public class CalendarGridViewModelTests
    {
        private CalendarGridViewModel _viewModel;
        private Mock<ITimeTrackingService> _mockTimeService;
        private Mock<IJSRuntime> _mockJsRuntime;
        private Mock<AuthenticationStateProvider> _mockAuthProvider;
        private bool _stateChangedFired;
        private DateTime? _selectedDayChangedValue;
        private List<WorkDay>? _monthDataLoadedValue;
        private (int Year, int Month)? _monthChangedValue;
        private DateTime? _daySelectedValue;
        private const string TEST_USER_ID = "test-user-id";

        [SetUp]
        public void Setup()
        {
            _mockTimeService = new Mock<ITimeTrackingService>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            
            var identity = new ClaimsIdentity(new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, TEST_USER_ID)
            }, "test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(claimsPrincipal);
            
            _mockAuthProvider = new Mock<AuthenticationStateProvider>();
            _mockAuthProvider.Setup(p => p.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);
            
            _viewModel = new CalendarGridViewModel(_mockTimeService.Object, _mockJsRuntime.Object, _mockAuthProvider.Object);
            
            _stateChangedFired = false;
            _selectedDayChangedValue = null;
            _monthDataLoadedValue = null;
            _monthChangedValue = null;
            _daySelectedValue = null;
            
            _viewModel.StateChanged += () => _stateChangedFired = true;
            _viewModel.SelectedDayChanged += (day) => _selectedDayChangedValue = day;
            _viewModel.MonthDataLoaded += (days) => _monthDataLoadedValue = days;
            _viewModel.MonthChanged += (monthInfo) => _monthChangedValue = monthInfo;
            _viewModel.DaySelected += (day) => _daySelectedValue = day;
        }

        [Test]
        public async Task InitializeAsync_LoadsUserDataAndMonthData()
        {
            // Arrange
            var today = DateTime.Today;
            var workDays = new List<WorkDay> { new WorkDay { Date = today } };
            
            _mockTimeService.Setup(s => s.GetWorkDaysForMonthAsync(TEST_USER_ID, today.Year, today.Month))
                .ReturnsAsync(workDays);
            
            // Act
            await _viewModel.InitializeAsync();
            
            // Assert
            Assert.That(_viewModel.Year, Is.EqualTo(today.Year));
            Assert.That(_viewModel.Month, Is.EqualTo(today.Month));
            Assert.That(_viewModel.MonthWorkDays, Is.EqualTo(workDays));
            Assert.That(_monthDataLoadedValue, Is.EqualTo(workDays));
        }
        
        [Test]
        public async Task NavigateMonthAsync_UpdatesMonthAndNotifies()
        {
            // Arrange
            var newYear = 2024;
            var newMonth = 6;
            var workDays = new List<WorkDay>();
            
            _mockTimeService.Setup(s => s.GetWorkDaysForMonthAsync(TEST_USER_ID, newYear, newMonth))
                .ReturnsAsync(workDays);
                
            await _viewModel.InitializeAsync();
            _stateChangedFired = false;
            _monthDataLoadedValue = null;
            
            // Act
            await _viewModel.NavigateMonthAsync((newYear, newMonth));
            
            // Assert
            Assert.That(_viewModel.Year, Is.EqualTo(newYear));
            Assert.That(_viewModel.Month, Is.EqualTo(newMonth));
            Assert.That(_monthChangedValue, Is.Not.Null);
            Assert.That(_monthChangedValue?.Year, Is.EqualTo(newYear));
            Assert.That(_monthChangedValue?.Month, Is.EqualTo(newMonth));
            Assert.That(_monthDataLoadedValue, Is.EqualTo(workDays));
        }
        
        [Test]
        public async Task HandleDayClickAsync_UpdatesSelectedDayAndNotifies()
        {
            // Arrange
            var selectedDay = new DateTime(2024, 6, 15);
            
            // Act
            await _viewModel.HandleDayClickAsync(selectedDay);
            
            // Assert
            Assert.That(_viewModel.SelectedDay, Is.EqualTo(selectedDay));
            Assert.That(_selectedDayChangedValue, Is.EqualTo(selectedDay));
            Assert.That(_daySelectedValue, Is.EqualTo(selectedDay));
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void CalculateTotalHours_ReturnsCorrectSum()
        {
            // Arrange
            var day = new DateTime(2024, 6, 15);
            var entries = new List<TimeEntry>
            {
                new TimeEntry { WorkDate = day, HoursWorked = 3.5 },
                new TimeEntry { WorkDate = day, HoursWorked = 4.5 }
            };
            var workDays = new List<WorkDay>
            {
                new WorkDay { Date = day, TimeEntries = entries }
            };
            
            SetMonthDataForTest(workDays);
            
            // Act
            var result = _viewModel.CalculateTotalHours(day);
            
            // Assert
            Assert.That(result, Is.EqualTo(8.0));
        }
        
        [Test]
        public void CalculateProjectSummary_ReturnsCorrectProjectHours()
        {
            // Arrange
            var day = new DateTime(2024, 6, 15);
            var project1 = new Project { Id = 1, Name = "Project 1" };
            var project2 = new Project { Id = 2, Name = "Project 2" };
            
            var entries = new List<TimeEntry>
            {
                new TimeEntry { WorkDate = day, HoursWorked = 3.5, Project = project1 },
                new TimeEntry { WorkDate = day, HoursWorked = 2.5, Project = project1 },
                new TimeEntry { WorkDate = day, HoursWorked = 4.5, Project = project2 }
            };
            
            var workDays = new List<WorkDay>
            {
                new WorkDay { Date = day, TimeEntries = entries }
            };
            
            SetMonthDataForTest(workDays);
            
            // Act
            var result = _viewModel.CalculateProjectSummary(day).ToList();
            
            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Item1, Is.EqualTo("Project 1"));
            Assert.That(result[0].Item2, Is.EqualTo(6.0));
            Assert.That(result[1].Item1, Is.EqualTo("Project 2"));
            Assert.That(result[1].Item2, Is.EqualTo(4.5));
        }
        
        // Hjälpmetod för att sätta månadsdata i ViewModel för tester
        private void SetMonthDataForTest(List<WorkDay> workDays)
        {
            // Använd reflektion för att sätta det privata fältet _monthWorkDays
            var fieldInfo = typeof(CalendarGridViewModel).GetField("_monthWorkDays", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(_viewModel, workDays);
            }
        }
    }
}