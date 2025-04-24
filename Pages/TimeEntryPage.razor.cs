using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TimeTracker.Models;
using TimeTracker.Pages.Components;
using TimeTracker.Services;

namespace TimeTracker.Pages
{
    public partial class TimeEntryPage
    {
        [Inject] protected ITimeTrackingService TimeService { get; set; }
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; }
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        // Component reference to access public methods
        private CalendarGrid _calendarGrid;

        // ---------- state ----------------------------------------------------
        private string _currentUserId = "";
        private List<Project>? _projects = new List<Project>();
        private List<WorkDay> _monthWorkDays = new();
        private DateTime _selectedDay = DateTime.MinValue;
        private List<WorkItem>? _dayWorkItems;
        private WorkItem _newWorkItem = new() { HoursWorked = 0 };

        private int _currentYear;
        private int _currentMonth;
        
        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthProvider.GetAuthenticationStateAsync();
            _currentUserId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var today = DateTime.Today;
            _currentYear = today.Year;
            _currentMonth = today.Month;
            _projects = await TimeService.GetProjectsAsync(_currentUserId);
            
            _selectedDay = today;
            _projects = await TimeService.GetProjectsAsync(_currentUserId);
            if (_newWorkItem != null)
            {
                _newWorkItem.WorkDate = today;
            }
        }

        private void HandleMonthDataLoaded(List<WorkDay> workDays)
        {
            _monthWorkDays = workDays;
            
            // If we have a selected day, update the day items
            if (_selectedDay != DateTime.MinValue)
            {
                UpdateDayWorkItems();
            }
        }

        private void UpdateDayWorkItems()
        {
            _dayWorkItems = _monthWorkDays
                .FirstOrDefault(w => w.Date.Date == _selectedDay.Date)?
                .WorkItems.ToList()
                ?? new List<WorkItem>();
        }

        private async Task RefreshCalendarData(int _)
        {
            if (_calendarGrid != null)
            {
                await _calendarGrid.RefreshDataAsync();
            }
        }

        private void HandleWorkItemsChanged(List<WorkItem> items)
        {
            _dayWorkItems = items;
            StateHasChanged();
        }

        private void HandleMonthChanged((int Year, int Month) newDate)
        {
            _currentYear = newDate.Year;
            _currentMonth = newDate.Month;
            _selectedDay = DateTime.MinValue;
            StateHasChanged();
        }

        private void HandleDaySelected(DateTime day)
        {
            _selectedDay = day;
            UpdateDayWorkItems();
            StateHasChanged();
        }
    }
}