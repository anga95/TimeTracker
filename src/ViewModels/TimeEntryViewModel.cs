using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels;

public class TimeEntryViewModel
{
    private readonly ITimeTrackingService _timeService;
    private readonly AuthenticationStateProvider _authProvider;

    // Tillstånd
    private string _currentUserId = "";
    private List<Project> _projects = new();
    private List<WorkDay> _monthWorkDays = new();
    private DateTime _selectedDay = DateTime.MinValue;
    private List<TimeEntry> _dayTimeEntry = new();
    private TimeEntry _newTimeEntry = new() { HoursWorked = 0 };
    private int _currentYear;
    private int _currentMonth;

    // Properties
    public string CurrentUserId => _currentUserId;
    public List<Project> Projects => _projects;
    public List<WorkDay> MonthWorkDays => _monthWorkDays;
    public DateTime SelectedDay => _selectedDay;
    public List<TimeEntry> DayTimeEntry => _dayTimeEntry;
    public int CurrentYear => _currentYear;
    public int CurrentMonth => _currentMonth;
    
    public TimeEntry NewTimeEntry 
    { 
        get => _newTimeEntry; 
        set 
        {
            _newTimeEntry = value;
            NotifyStateChanged();
        }
    }

    
    // Event för UI-uppdatering
    public event Action StateChanged;

    public TimeEntryViewModel(ITimeTrackingService timeService, AuthenticationStateProvider authProvider)
    {
        _timeService = timeService;
        _authProvider = authProvider;
    }

    public async Task InitializeAsync()
    {
        var auth = await _authProvider.GetAuthenticationStateAsync();
        _currentUserId = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        var today = DateTime.Today;
        _currentYear = today.Year;
        _currentMonth = today.Month;
        _selectedDay = today;
        
        _projects = await _timeService.GetProjectsAsync(_currentUserId);
        
        _newTimeEntry.WorkDate = today;
        NotifyStateChanged();
    }

    public void HandleMonthDataLoaded(List<WorkDay> workDays)
    {
        _monthWorkDays = workDays;
        
        if (_selectedDay != DateTime.MinValue)
        {
            UpdateDayTimeEntries();
        }
    }
    
    public void HandleMonthChanged((int Year, int Month) newDate)
    {
        _currentYear = newDate.Year;
        _currentMonth = newDate.Month;
        _selectedDay = DateTime.MinValue;
        NotifyStateChanged();
    }

    public void HandleDaySelected(DateTime day)
    {
        _selectedDay = day;
        UpdateDayTimeEntries();
        NotifyStateChanged();
    }

    public void HandleTimeEntriesChanged(List<TimeEntry> items)
    {
        _dayTimeEntry = items;
        NotifyStateChanged();
    }

    private void UpdateDayTimeEntries()
    {
        _dayTimeEntry = _monthWorkDays
            .FirstOrDefault(w => w.Date.Date == _selectedDay.Date)?
            .TimeEntries.ToList()
            ?? new List<TimeEntry>();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}