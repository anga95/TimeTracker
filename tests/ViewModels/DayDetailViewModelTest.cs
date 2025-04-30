using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.Tests.ViewModels
{
    [TestFixture]
    public class DayDetailViewModelTests
    {
        private DayDetailViewModel _viewModel;
        private Mock<ITimeTrackingService> _mockTimeService;
        private Mock<IJSRuntime> _mockJsRuntime;
        private bool _stateChangedFired;
        private TimeEntry? _timeEntryChangedValue;
        private List<TimeEntry>? _itemsChangedValue;
        private int? _refreshRequestedValue;

        [SetUp]
        public void Setup()
        {
            _mockTimeService = new Mock<ITimeTrackingService>();
            _mockJsRuntime = new Mock<IJSRuntime>();
            
            _viewModel = new DayDetailViewModel(_mockTimeService.Object, _mockJsRuntime.Object);
            
            _stateChangedFired = false;
            _timeEntryChangedValue = null;
            _itemsChangedValue = null;
            _refreshRequestedValue = null;
            
            _viewModel.StateChanged += () => _stateChangedFired = true;
            _viewModel.TimeEntryChanged += (entry) => _timeEntryChangedValue = entry;
            _viewModel.ItemsChanged += (items) => _itemsChangedValue = items;
            _viewModel.RefreshRequested += (value) => _refreshRequestedValue = value;
            
            // Standardvärden
            _viewModel.Day = new DateTime(2024, 6, 15);
            _viewModel.CurrentUserId = "test-user-id";
            _viewModel.IsAuthenticated = true;
        }
        
        [Test]
        public void Initialize_CreatesNewEditContext()
        {
            // Act
            _viewModel.Initialize();
            
            // Assert
            Assert.That(_viewModel.EditContext, Is.Not.Null);
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void SetParameters_UpdatesWorkDateAndEditContext()
        {
            // Arrange
            var testDate = new DateTime(2024, 7, 10);
            _viewModel.Day = testDate;
            _stateChangedFired = false;
            
            // Act
            _viewModel.SetParameters();
            
            // Assert
            Assert.That(_viewModel.TimeEntry.WorkDate, Is.EqualTo(testDate));
            Assert.That(_viewModel.EditContext, Is.Not.Null);
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public async Task HandleValidSubmit_WithValidData_AddsTimeEntry()
        {
            // Arrange
            _viewModel.TimeEntry = new TimeEntry
            {
                WorkDate = new DateTime(2024, 6, 15),
                ProjectId = 1,
                HoursWorked = 8,
                Comment = "Test comment"
            };
            
            _mockTimeService.Setup(s => s.AddTimeEntryAsync(It.IsAny<TimeEntry>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            var context = new EditContext(_viewModel.TimeEntry);
            
            // Act
            await _viewModel.HandleValidSubmit(context);
            
            // Assert
            _mockTimeService.Verify(s => s.AddTimeEntryAsync(
                It.Is<TimeEntry>(e => 
                    e.WorkDate == _viewModel.Day && 
                    e.ProjectId == 1 &&
                    e.HoursWorked == 8 &&
                    e.Comment == "Test comment"), 
                _viewModel.CurrentUserId), 
                Times.Once);
            
            Assert.That(_refreshRequestedValue, Is.EqualTo(0));
            // Form reset verifiering
            Assert.That(_viewModel.TimeEntry.ProjectId, Is.EqualTo(0));
            Assert.That(_viewModel.TimeEntry.HoursWorked, Is.EqualTo(0));
            Assert.That(_viewModel.TimeEntry.Comment, Is.Null);
        }
        
        [Test]
        public async Task DeleteTimeEntry_WithConfirmation_DeletesEntry()
        {
            // Arrange
            var entryId = 123;
            
            _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(true);
            
            _mockTimeService.Setup(s => s.DeleteTimeEntryAsync(entryId))
                .Returns(Task.CompletedTask);
            
            // Act
            await _viewModel.DeleteTimeEntry(entryId);
            
            // Assert
            _mockTimeService.Verify(s => s.DeleteTimeEntryAsync(entryId), Times.Once);
            Assert.That(_refreshRequestedValue, Is.EqualTo(0));
        }
        
        [Test]
        public async Task DeleteTimeEntry_WithoutConfirmation_DoesNotDelete()
        {
            // Arrange
            var entryId = 123;
            
            _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(false);
            
            // Act
            await _viewModel.DeleteTimeEntry(entryId);
            
            // Assert
            _mockTimeService.Verify(s => s.DeleteTimeEntryAsync(It.IsAny<int>()), Times.Never);
            Assert.That(_refreshRequestedValue, Is.Null);
        }
        
        [Test]
        public async Task HandleProjectsChanged_LoadsProjects()
        {
            // Arrange
            var projects = new List<Project> { new Project { Id = 1, Name = "Test Project" } };
            
            _mockTimeService.Setup(s => s.GetProjectsAsync(_viewModel.CurrentUserId))
                .ReturnsAsync(projects);
            
            // Act
            await _viewModel.HandleProjectsChanged();
            
            // Assert
            Assert.That(_viewModel.Projects, Is.EqualTo(projects));
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void NotifyTimeEntryChanged_FiresEvent()
        {
            // Arrange
            var entry = new TimeEntry { Id = 1, HoursWorked = 8 };
            
            // Act
            _viewModel.NotifyTimeEntryChanged(entry);
            
            // Assert
            Assert.That(_timeEntryChangedValue, Is.EqualTo(entry));
            Assert.That(_stateChangedFired, Is.True);
        }
    }
}