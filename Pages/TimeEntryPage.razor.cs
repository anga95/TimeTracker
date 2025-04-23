using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.Pages
{
    public partial class TimeEntryPage
    {
        [Inject] protected ITimeTrackingService TimeService { get; set; }
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; }
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        // ---------- state ----------------------------------------------------
        private string _currentUserId = "";
        private List<Project>? _projects = new List<Project>();
        private List<WorkDay> _monthWorkDays = new();
        private DateTime _selectedDay = DateTime.MinValue;
        private List<WorkItem>? _dayWorkItems;
        private WorkItem _newWorkItem = new() { HoursWorked = 0 };


        private int _currentYear;
        private int _currentMonth;

        private string currentMonthName =>
            System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_currentMonth);

        private DateTime _today = DateTime.Today;
        private DateTime _gridStart;
        private int _totalRows;

        private readonly string[] weekdayHeaders =
            { "Mån", "Tis", "Ons", "Tors", "Fre", "Lör", "Sön" };

        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthProvider.GetAuthenticationStateAsync();
            _currentUserId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var today = DateTime.Today;
            _currentYear = today.Year;
            _currentMonth = today.Month;
            _projects = await TimeService.GetProjectsAsync(_currentUserId);

            await LoadMonth();

            SelectDay(today);
        }

        // ---------- kalender -------------------------------------------------
        private async Task LoadMonth()
        {
            var all = await TimeService.GetWorkDaysAsync(_currentUserId);
            _monthWorkDays = all.Where(d => d.Date.Year == _currentYear && d.Date.Month == _currentMonth).ToList();

            var first = new DateTime(_currentYear, _currentMonth, 1);
            int dow = (int)first.DayOfWeek;
            if (dow == 0) dow = 7;
            var offset = dow - 1;
            _gridStart = first.AddDays(-offset);

            int cells = offset + DateTime.DaysInMonth(_currentYear, _currentMonth);
            _totalRows = (int)Math.Ceiling(cells / 7.0);
        }

        private double GetTotalHours(DateTime d) =>
            _monthWorkDays.FirstOrDefault(w => w.Date.Date == d.Date)?.WorkItems.Sum(i => i.HoursWorked) ?? 0;

        private double GetRoundedTotal(List<WorkItem>? items) =>
            items != null
                ? Math.Ceiling(items.Sum(i => i.HoursWorked) * 2) / 2
                : 0;

        private void SelectDay(DateTime d)
        {
            _selectedDay = d;
            _newWorkItem = new WorkItem { WorkDate = d };
            _dayWorkItems = _monthWorkDays
                               .FirstOrDefault(w => w.Date.Date == d.Date)?
                               .WorkItems.ToList()
                           ?? new List<WorkItem>();
        }

        private async Task PrevMonth()
        {
            if (_currentMonth == 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }
            else _currentMonth--;

            await LoadMonth();
            _selectedDay = DateTime.MinValue;
        }

        private async Task NextMonth()
        {
            if (_currentMonth == 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }
            else _currentMonth++;

            await LoadMonth();
            _selectedDay = DateTime.MinValue;
        }

        // ---------- inmatning -------------------------------------------------
        private async Task HandleSubmit()
        {
            if (_newWorkItem.ProjectId == 0) return;
            await TimeService.AddWorkItemAsync(_newWorkItem, _currentUserId);
            await LoadMonth();
            SelectDay(_selectedDay); // reload items
        }

        private async Task Delete(int id)
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", "Är du säker?"))
            {
                await TimeService.DeleteWorkItemAsync(id);
                await LoadMonth();
                SelectDay(_selectedDay);
            }
        }

        private IEnumerable<(string projectName, double hours)> GetProjectSummary(DateTime day)
        {
            var wd = _monthWorkDays.FirstOrDefault(w => w.Date.Date == day.Date);
            if (wd == null)
                return Enumerable.Empty<(string, double)>();

            return wd.WorkItems
                .GroupBy(wi => wi.Project?.Name ?? "Okänt projekt")
                .Select(g => (projectName: g.Key, hours: g.Sum(wi => wi.HoursWorked)));
        }

        // När ett nytt projekt skapas i ProjectsManager
        private async Task HandleCreateProject(string projectName)
        {
            await TimeService.CreateProjectAsync(projectName, _currentUserId);
            _projects = await TimeService.GetProjectsAsync(_currentUserId);
            StateHasChanged();
        }

        // När användaren vill ta bort ett projekt
        private async Task HandleDeleteProject(int projectId)
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", "Är du säker att du vill radera projektet?"))
            {
                await TimeService.DeleteProjectAsync(projectId);
                _projects = await TimeService.GetProjectsAsync(_currentUserId);
                StateHasChanged();
            }
        }
    }
}