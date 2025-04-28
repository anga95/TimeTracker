using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class DayDetailViewModel
    {
        private readonly ITimeTrackingService _timeService;
        private readonly IJSRuntime _jsRuntime;
        private EditContext? _editContext;
        private TimeEntry _timeEntry = new();
        
        // Properties
        public DateTime Day { get; set; }
        public List<TimeEntry> Items { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public string CurrentUserId { get; set; } = "";
        
        public EditContext? EditContext => _editContext;
        
        public TimeEntry TimeEntry 
        { 
            get => _timeEntry;
            set
            {
                _timeEntry = value;
                _editContext = new EditContext(_timeEntry);
                NotifyStateChanged();
            }
        }
        
        // Events
        public event Action? StateChanged;
        public event Action<TimeEntry>? TimeEntryChanged;
        public event Action<List<TimeEntry>>? ItemsChanged;
        public event Action<int>? RefreshRequested;
        
        public DayDetailViewModel(ITimeTrackingService timeService, IJSRuntime jsRuntime)
        {
            _timeService = timeService;
            _jsRuntime = jsRuntime;
            _editContext = new EditContext(TimeEntry);
        }
        
        public void Initialize()
        {
            _editContext = new EditContext(TimeEntry);
            NotifyStateChanged();
        }
        
        public void SetParameters()
        {
            TimeEntry.WorkDate = Day;
            
            if (_editContext == null || _editContext.Model != TimeEntry)
            {
                _editContext = new EditContext(TimeEntry);
            }
            
            NotifyStateChanged();
        }
        
        public async Task HandleValidSubmit(EditContext context)
        {
            if (TimeEntry.ProjectId == 0) return;

            var timeEntryToSubmit = new TimeEntry
            {
                WorkDate = TimeEntry.WorkDate,
                ProjectId = TimeEntry.ProjectId,
                HoursWorked = TimeEntry.HoursWorked,
                Comment = TimeEntry.Comment
            };

            await AddTimeEntry(timeEntryToSubmit);
        }
        
        private void ResetForm()
        {
            TimeEntry.ProjectId = 0;
            TimeEntry.HoursWorked = 0;
            TimeEntry.Comment = null;
            TimeEntryChanged?.Invoke(TimeEntry);
            NotifyStateChanged();
        }
        
        private async Task AddTimeEntry(TimeEntry timeEntry)
        {
            if (timeEntry.ProjectId == 0) return;

            try
            {
                await _timeService.AddTimeEntryAsync(timeEntry, CurrentUserId);
                RefreshRequested?.Invoke(0);
                ResetForm();
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("alert", $"Fel vid registrering av tid: {ex.Message}");
            }
        }
        
        public async Task DeleteTimeEntry(int id)
        {
            try
            {
                if (await _jsRuntime.InvokeAsync<bool>("confirm", "Är du säker?"))
                {
                    try
                    {
                        await _timeService.DeleteTimeEntryAsync(id);
                        RefreshRequested?.Invoke(0);
                    }
                    catch (Exception ex)
                    {
                        await _jsRuntime.InvokeVoidAsync("alert", $"Fel vid radering: {ex.Message}");
                        Console.Error.WriteLine($"Fel vid radering av tidpost {id}: {ex}");
                    }
                }
            }
            catch (JSException ex)
            {
                Console.Error.WriteLine($"JavaScript-fel vid bekräftelsedialog: {ex}");
            }
        }
        
        public async Task HandleProjectsChanged()
        {
            Projects = await _timeService.GetProjectsAsync(CurrentUserId);
            NotifyStateChanged();
        }
        
        public void NotifyTimeEntryChanged(TimeEntry entry)
        {
            TimeEntryChanged?.Invoke(entry);
            NotifyStateChanged();
        }
        
        private void NotifyStateChanged() => StateChanged?.Invoke();
    }
}