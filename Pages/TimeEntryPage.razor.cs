using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TimeTracker.Models;
using TimeTracker.Pages.Components;
using TimeTracker.Services;

namespace TimeTracker.Pages
{
    public partial class TimeEntryPage
    {
        [Inject] protected ITimeTrackingService TimeService { get; set; }
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; }

        // Component reference
        private CalendarGrid _calendarGrid;

        // ---------- State ----------------------------------------------------
        private string _currentUserId = "";
        private List<Project>? _projects = new List<Project>();
        private List<WorkDay> _monthWorkDays = new();
        private DateTime _selectedDay = DateTime.MinValue;
        private List<TimeEntry>? _dayTimeEntry;
        private TimeEntry _newTimeEntry = new() { HoursWorked = 0 };

        private int _currentYear;
        private int _currentMonth;
        
        protected override async Task OnInitializedAsync()
        {
            // Get the current user ID
            var auth = await AuthProvider.GetAuthenticationStateAsync();
            _currentUserId = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            var today = DateTime.Today;
            _currentYear = today.Year;
            _currentMonth = today.Month;
            _selectedDay = today;
            
            _projects = await TimeService.GetProjectsAsync(_currentUserId);
            
            _newTimeEntry.WorkDate = today;
        }

        // ---------- Eventhandler for CalendarGrid ----------------------------
        
        private void HandleMonthDataLoaded(List<WorkDay> workDays)
        {
            _monthWorkDays = workDays;
            
            if (_selectedDay != DateTime.MinValue)
            {
                UpdateDayTimeEntries();
            }
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
            UpdateDayTimeEntries();
            StateHasChanged();
        }

        // ---------- Eventhandler for DayDetail ------------------------------

        private async Task RefreshCalendarData(int _)
        {
            if (_calendarGrid != null)
            {
                await _calendarGrid.RefreshDataAsync();
            }
        }

        private void HandleTimeEntriesChanged(List<TimeEntry> items)
        {
            _dayTimeEntry = items;
            StateHasChanged();
        }

        // ---------- Helpers ----------------------------------------------

        private void UpdateDayTimeEntries()
        {
            _dayTimeEntry = _monthWorkDays
                .FirstOrDefault(w => w.Date.Date == _selectedDay.Date)?
                .TimeEntries.ToList()
                ?? new List<TimeEntry>();
        }
    }
}