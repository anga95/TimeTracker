using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Moq;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.Tests.ViewModels
{
    [TestFixture]
    public class ProjectSelectorViewModelTests
    {
        private ProjectSelectorViewModel _viewModel;
        private Mock<ITimeTrackingService> _mockTimeService;
        private Mock<IJSRuntime> _mockJsRuntime;
        private Mock<AuthenticationStateProvider> _mockAuthProvider;
        private bool _stateChangedFired;
        private int? _selectedProjectIdChangedValue;
        private bool _projectChangedFired;
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
            
            _viewModel = new ProjectSelectorViewModel(_mockTimeService.Object, _mockJsRuntime.Object, _mockAuthProvider.Object);
            
            _stateChangedFired = false;
            _selectedProjectIdChangedValue = null;
            _projectChangedFired = false;
            
            _viewModel.StateChanged += () => _stateChangedFired = true;
            _viewModel.SelectedProjectIdChanged += (id) => 
            {
                _selectedProjectIdChangedValue = id;
                return Task.CompletedTask;
            };
            _viewModel.ProjectChanged += () => 
            {
                _projectChangedFired = true;
                return Task.CompletedTask;
            };
        }

        [Test]
        public async Task InitializeAsync_SetsUserID()
        {
            // Act
            await _viewModel.InitializeAsync();
            
            // Assert
            Assert.That(_stateChangedFired, Is.True);
            
            var projectName = "Test Project";
            _mockTimeService.Setup(s => s.CreateProjectAsync(projectName, TEST_USER_ID))
                .Returns(Task.CompletedTask);
                
            _viewModel.NewProjectName = projectName;
            await _viewModel.CreateProject();
            
            _mockTimeService.Verify(s => s.CreateProjectAsync(projectName, TEST_USER_ID), Times.Once);
        }
        
        [Test]
        public void OpenModal_SetsShowModalToTrue()
        {
            // Act
            _viewModel.OpenModal();
            
            // Assert
            Assert.That(_viewModel.ShowModal, Is.True);
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public void NewProjectName_PropertyChangeNotifiesStateChanged()
        {
            // Act
            _viewModel.NewProjectName = "Test Project";
            
            // Assert
            Assert.That(_viewModel.NewProjectName, Is.EqualTo("Test Project"));
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public async Task CreateProject_WithValidName_CreatesProjectAndClosesModal()
        {
            // Arrange
            var projectName = "New Test Project";
            
            await _viewModel.InitializeAsync();
            
            _viewModel.NewProjectName = projectName;
            
            _mockTimeService.Setup(s => s.CreateProjectAsync(projectName, TEST_USER_ID))
                .Returns(Task.CompletedTask);
            
            // Act
            await _viewModel.CreateProject();
            
            // Assert
            _mockTimeService.Verify(s => s.CreateProjectAsync(projectName, TEST_USER_ID), Times.Once);
            Assert.That(_projectChangedFired, Is.True);
            Assert.That(_viewModel.ShowModal, Is.False);
            Assert.That(_viewModel.NewProjectName, Is.Empty);
        }
        
        [Test]
        public async Task CreateProject_WithEmptyName_DoesNotCreateProject()
        {
            // Arrange
            await _viewModel.InitializeAsync();
            _viewModel.NewProjectName = "";
            
            // Act
            await _viewModel.CreateProject();
            
            // Assert
            _mockTimeService.Verify(s => s.CreateProjectAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.That(_projectChangedFired, Is.False);
        }
        
        [Test]
        public void CloseModal_ResetsStateAndClosesModal()
        {
            // Arrange
            _viewModel.OpenModal();
            _viewModel.NewProjectName = "Test Project";
            _stateChangedFired = false;
            
            // Act
            _viewModel.CloseModal();
            
            // Assert
            Assert.That(_viewModel.ShowModal, Is.False);
            Assert.That(_viewModel.NewProjectName, Is.Empty);
            Assert.That(_stateChangedFired, Is.True);
        }
        
        [Test]
        public async Task DeleteProjectWithConfirmation_WithConfirmation_DeletesProject()
        {
            // Arrange
            await _viewModel.InitializeAsync();
            _viewModel.SelectedProjectId = 1;
            
            _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(true);
            
            _mockTimeService.Setup(s => s.DeleteProjectAsync(_viewModel.SelectedProjectId))
                .Returns(Task.CompletedTask);
            
            // Act
            await _viewModel.DeleteProjectWithConfirmation();
            
            // Assert
            _mockTimeService.Verify(s => s.DeleteProjectAsync(_viewModel.SelectedProjectId), Times.Once);
            Assert.That(_selectedProjectIdChangedValue, Is.EqualTo(0));
            Assert.That(_projectChangedFired, Is.True);
        }
        
        [Test]
        public async Task DeleteProjectWithConfirmation_WithoutConfirmation_DoesNotDeleteProject()
        {
            // Arrange
            await _viewModel.InitializeAsync();
            _viewModel.SelectedProjectId = 1;
            var before = _selectedProjectIdChangedValue;
            
            _mockJsRuntime
                .Setup(js => js.InvokeAsync<bool>("confirm", It.IsAny<object[]>()))
                .ReturnsAsync(false);
            
            // Act
            await _viewModel.DeleteProjectWithConfirmation();
            
            // Assert
            _mockTimeService.Verify(s => s.DeleteProjectAsync(It.IsAny<int>()), Times.Never);
            Assert.That(_selectedProjectIdChangedValue, Is.EqualTo(before));
        }
        
        [Test]
        public async Task OnSelectionChanged_WithValidValue_UpdatesSelectedProjectId()
        {
            // Arrange
            var newValue = 5;
            
            // Act
            _viewModel.SelectedProjectId = newValue;
            
            // Assert
            Assert.That(_viewModel.SelectedProjectId, Is.EqualTo(newValue));
            Assert.That(_selectedProjectIdChangedValue, Is.EqualTo(newValue));
        }
        
        [Test]
        public async Task SelectedProjectId_Setter_SameValue_DoesNotRaiseEventTwice()
        {
            // Arrange
            int raiseCount = 0;
            _viewModel.SelectedProjectIdChanged += id => { raiseCount++; return Task.CompletedTask; };

            // Act
            _viewModel.SelectedProjectId = 5;
            _viewModel.SelectedProjectId = 5; // samma värde

            // Assert
            Assert.That(raiseCount, Is.EqualTo(1));
        }
    }
}