using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class ProjectSelectorViewModel
    {
        private readonly ITimeTrackingService _timeService;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authProvider;
        
        private bool _showModal;
        private string _newProjectName = string.Empty;
        private string _currentUserId = string.Empty;
        
        public bool ShowModal 
        { 
            get => _showModal; 
            private set 
            { 
                _showModal = value; 
                NotifyStateChanged(); 
            } 
        }
        
        public string NewProjectName 
        { 
            get => _newProjectName; 
            set 
            { 
                _newProjectName = value; 
                NotifyStateChanged(); 
            } 
        }
        
        public List<Project> Projects { get; set; } = new();
        private int _selectedProjectId;

        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set
            {
                if (_selectedProjectId == value) return;
                
                _selectedProjectId = value;
                _ = SelectedProjectIdChanged?.Invoke(value);
                NotifyStateChanged();
            }
        }
        
        public event Action? StateChanged;
        public event Func<int, Task>? SelectedProjectIdChanged;
        public event Func<Task>? ProjectChanged;
        
        public ProjectSelectorViewModel(
            ITimeTrackingService timeService, 
            IJSRuntime jsRuntime, 
            AuthenticationStateProvider authProvider)
        {
            _timeService = timeService;
            _jsRuntime = jsRuntime;
            _authProvider = authProvider;
        }
        
        public async Task InitializeAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            _currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            NotifyStateChanged();
        }
        
        public async Task CreateProject()
        {
            if (string.IsNullOrWhiteSpace(_newProjectName))
                return;

            try
            {
                await _timeService.CreateProjectAsync(_newProjectName, _currentUserId);
                
                if (ProjectChanged != null)
                    await ProjectChanged.Invoke();

                CloseModal();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", $"Fel vid skapande av projekt: {ex.Message}");
            }
        }
        
        public void CloseModal()
        {
            ShowModal = false;
            NewProjectName = string.Empty;
        }
        
        public void OpenModal()
        {
            ShowModal = true;
        }
        
        public async Task DeleteProjectWithConfirmation()
        {
            if (SelectedProjectId == 0)
                return;

            if (await _jsRuntime.InvokeAsync<bool>("confirm", "Radera projektet?"))
            {
                try
                {
                    await _timeService.DeleteProjectAsync(SelectedProjectId);
                    
                    if (SelectedProjectIdChanged != null)
                        await SelectedProjectIdChanged.Invoke(0);
                    
                    if (ProjectChanged != null)
                        await ProjectChanged.Invoke();
                }
                catch (Exception ex)
                {
                    await _jsRuntime.InvokeVoidAsync("alert", $"Fel vid radering av projekt: {ex.Message}");
                }
            }
        }

        public async Task ArchiveProjectWithConfirmation()
        {
            if (SelectedProjectId == 0) return;

            try
            {
                var archiveProject = await _jsRuntime.InvokeAsync<bool>(
                    "confirm",
                    "Arkivera projektet?"
                    );
                
                if (!archiveProject) return;
                
                await _timeService.ArchiveProjectAsync(SelectedProjectId);
                
                SelectedProjectId = 0;
                
                if (SelectedProjectIdChanged != null)
                    await SelectedProjectIdChanged.Invoke(SelectedProjectId);

                if (ProjectChanged != null)
                    await ProjectChanged.Invoke();
                
            }
            catch (ValidationException ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", ex.Message);
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", $"Fel vid arkivering av projekt: {ex.Message}");
            }
        }
        
        private void NotifyStateChanged() => StateChanged?.Invoke();
    }
}