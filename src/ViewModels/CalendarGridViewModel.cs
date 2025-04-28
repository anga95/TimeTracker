using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class CalendarGridViewModel
    {
        private readonly ITimeTrackingService _timeService;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authProvider;
        
        private bool _isLoading = false;
        private DateTime _selectedDay = DateTime.Today;
        private DateTime _gridStart;
        private int _totalRows;
        private List<WorkDay> _monthWorkDays = new();
        private string _currentUserId = "";
        
        public int Year { get; private set; }
        public int Month { get; private set; }
        public bool IsLoading => _isLoading;
        public DateTime SelectedDay => _selectedDay;
        public DateTime GridStart => _gridStart;
        public int TotalRows => _totalRows;
        public List<WorkDay> MonthWorkDays => _monthWorkDays;
        public string[] WeekdayHeaders => new[] { "Mån", "Tis", "Ons", "Tors", "Fre", "Lör", "Sön" };
        
        public event Action? StateChanged;
        public event Action<DateTime>? SelectedDayChanged;
        public event Action<List<WorkDay>>? MonthDataLoaded;
        public event Action<(int Year, int Month)>? MonthChanged;
        public event Action<DateTime>? DaySelected;
        
        public CalendarGridViewModel(
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
            // Hämta användar-ID
            var authState = await _authProvider.GetAuthenticationStateAsync();
            _currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            
            var now = DateTime.Now;
            
            // Ange värdena utan att köra SetMonthAsync, som skulle trigga datainläsning igen
            Year = now.Year;
            Month = now.Month;
            
            // Beräkna kalender-layout
            _gridStart = CalculateGridStart();
            _totalRows = CalculateTotalRows();
            
            _isLoading = true;
            NotifyStateChanged();
            
            try
            {
                _monthWorkDays = await _timeService.GetWorkDaysForMonthAsync(_currentUserId, Year, Month);
                MonthDataLoaded?.Invoke(_monthWorkDays);
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Fel vid hämtning av månadsdata: {ex.Message}");
                _monthWorkDays = new List<WorkDay>();
            }
            
            _isLoading = false;
            NotifyStateChanged();
        }
        
        public Task NavigateMonthAsync((int Year, int Month) newDate)
        {
            var task = SetMonthAsync(newDate.Year, newDate.Month);
            MonthChanged?.Invoke((Year, Month));
            return task;
        }
        
        public Task HandleDayClickAsync(DateTime day)
        {
            _selectedDay = day;
            SelectedDayChanged?.Invoke(day);
            DaySelected?.Invoke(day);
            NotifyStateChanged();
            return Task.CompletedTask;
        }
        
        public Task RefreshDataAsync()
        {
            return LoadMonthDataAsync();
        }
        
        public double CalculateTotalHours(DateTime day)
        {
            var dayEntries = _monthWorkDays
                .FirstOrDefault(wd => wd.Date.Date == day.Date)?.TimeEntries;
                
            if (dayEntries == null || !dayEntries.Any())
                return 0;
                
            return dayEntries.Sum(e => e.HoursWorked);
        }
        
        public IEnumerable<(string, double)> CalculateProjectSummary(DateTime day)
        {
            var dayEntries = _monthWorkDays
                .FirstOrDefault(wd => wd.Date.Date == day.Date)?.TimeEntries;
                
            if (dayEntries == null || !dayEntries.Any())
                return Enumerable.Empty<(string, double)>();
                
            return dayEntries.GroupBy(e => e.Project?.Name ?? "Okänt projekt")
                .Select(g => (g.Key, g.Sum(e => e.HoursWorked)));
        }
        
        private DateTime CalculateGridStart()
        {
            var firstDayOfMonth = new DateTime(Year, Month, 1);
            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            
            // Adjust to start from Monday (Monday is 1, Sunday is 0)
            int daysToSubtract = (dayOfWeek == 0 ? 6 : dayOfWeek - 1);
            
            return firstDayOfMonth.AddDays(-daysToSubtract);
        }
        
        private int CalculateTotalRows()
        {
            int daysInMonth = DateTime.DaysInMonth(Year, Month);
            DateTime monthStart = CalculateGridStart();
            
            // Calculate how many days are in the grid including the previous and next month days
            int totalDaysInView = daysInMonth + (int)(new DateTime(Year, Month, daysInMonth).DayOfWeek) + (monthStart.Day - 1);
            
            return (int)Math.Ceiling(totalDaysInView / 7.0);
        }
        
        private void NotifyStateChanged() => StateChanged?.Invoke();
        
        private async Task LoadMonthDataAsync()
        {
            _isLoading = true;
            NotifyStateChanged();
            
            try
            {
                _monthWorkDays = await _timeService.GetWorkDaysForMonthAsync(_currentUserId, Year, Month);
                MonthDataLoaded?.Invoke(_monthWorkDays);
            }
            catch (Exception ex)
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"Fel vid hämtning av månadsdata: {ex.Message}");
                _monthWorkDays = new List<WorkDay>();
            }
            
            _isLoading = false;
            NotifyStateChanged();
        }
        
        public async Task SetMonthAsync(int year, int month)
        {
            if (year == Year && month == Month)
                return;
                
            Year = year;
            Month = month;
            
            _gridStart = CalculateGridStart();
            _totalRows = CalculateTotalRows();
            
            await LoadMonthDataAsync();
        }
    }
}