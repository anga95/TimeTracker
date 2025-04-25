using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.Tests.ViewModels
{
    [TestFixture]
    public class TimeEntryViewModelTests
    {
        private TimeEntryViewModel _viewModel;
        private Mock<ITimeTrackingService> _mockTimeService;
        private Mock<AuthenticationStateProvider> _mockAuthProvider;
        private bool _stateChangedFired;
        private const string TEST_USER_ID = "test-user-id";

        [SetUp]
        public void Setup()
        {
            _mockTimeService = new Mock<ITimeTrackingService>();
            
            // Skapa en mock för AuthProvider
            var identity = new ClaimsIdentity(new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, TEST_USER_ID)
            }, "test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var authState = new AuthenticationState(claimsPrincipal);
            
            _mockAuthProvider = new Mock<AuthenticationStateProvider>();
            _mockAuthProvider.Setup(p => p.GetAuthenticationStateAsync())
                .ReturnsAsync(authState);
            
            _viewModel = new TimeEntryViewModel(_mockTimeService.Object, _mockAuthProvider.Object);
            _stateChangedFired = false;
            _viewModel.StateChanged += () => _stateChangedFired = true;
        }

        [Test]
        public async Task InitializeAsync_SetsUserIdAndInitialProperties()
        {
            // Arrange
            var projects = new List<Project> { new Project { Id = 1, Name = "Test Project" } };
            _mockTimeService.Setup(s => s.GetProjectsAsync(TEST_USER_ID))
                .ReturnsAsync(projects);
            
            // Act
            await _viewModel.InitializeAsync();
            
            // Assert
            Assert.That(_viewModel.CurrentUserId, Is.EqualTo(TEST_USER_ID));
            Assert.That(_viewModel.Projects, Is.EqualTo(projects));
            Assert.That(_stateChangedFired, Is.True);
        }

        [Test]
        public void HandleMonthChanged_UpdatesYearAndMonth()
        {
            // Arrange
            var newYear = 2025;
            var newMonth = 6;
            
            // Act
            _viewModel.HandleMonthChanged((newYear, newMonth));
            
            // Assert
            Assert.That(_viewModel.CurrentYear, Is.EqualTo(newYear));
            Assert.That(_viewModel.CurrentMonth, Is.EqualTo(newMonth));
            Assert.That(_viewModel.SelectedDay, Is.EqualTo(DateTime.MinValue));
            Assert.That(_stateChangedFired, Is.True);
        }

        // Fler tester för övriga metoder
    }
}