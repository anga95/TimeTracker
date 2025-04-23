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
        private string currentUserId = "";
        private List<Project>? projects;
        private List<WorkDay> monthWorkDays = new();
        private DateTime selectedDay = DateTime.MinValue;
        private List<WorkItem>? dayWorkItems;
        private WorkItem newWorkItem = new() { HoursWorked = 0 };


        private int currentYear;
        private int currentMonth;

        private string currentMonthName =>
            System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentMonth);

        private DateTime today = DateTime.Today;
        private DateTime gridStart;
        private int totalRows;

        private readonly string[] weekdayHeaders =
            { "Mån", "Tis", "Ons", "Tors", "Fre", "Lör", "Sön" };

        protected override async Task OnInitializedAsync()
        {
            var auth = await AuthProvider.GetAuthenticationStateAsync();
            currentUserId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var today = DateTime.Today;
            currentYear = today.Year;
            currentMonth = today.Month;
            projects = await TimeService.GetProjectsAsync(currentUserId);

            await LoadMonth();

            SelectDay(today);
        }

        // ---------- kalender -------------------------------------------------
        private async Task LoadMonth()
        {
            var all = await TimeService.GetWorkDaysAsync(currentUserId);
            monthWorkDays = all.Where(d => d.Date.Year == currentYear && d.Date.Month == currentMonth).ToList();

            var first = new DateTime(currentYear, currentMonth, 1);
            int dow = (int)first.DayOfWeek;
            if (dow == 0) dow = 7;
            var offset = dow - 1;
            gridStart = first.AddDays(-offset);

            int cells = offset + DateTime.DaysInMonth(currentYear, currentMonth);
            totalRows = (int)Math.Ceiling(cells / 7.0);
        }

        private double GetTotalHours(DateTime d) =>
            monthWorkDays.FirstOrDefault(w => w.Date.Date == d.Date)?.WorkItems.Sum(i => i.HoursWorked) ?? 0;

        private double GetRoundedTotal(List<WorkItem>? items) =>
            items != null
                ? Math.Ceiling(items.Sum(i => i.HoursWorked) * 2) / 2
                : 0;

        private void SelectDay(DateTime d)
        {
            selectedDay = d;
            newWorkItem = new WorkItem { WorkDate = d };
            dayWorkItems = monthWorkDays
                               .FirstOrDefault(w => w.Date.Date == d.Date)?
                               .WorkItems.ToList()
                           ?? new List<WorkItem>();
        }

        private async Task PrevMonth()
        {
            if (currentMonth == 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            else currentMonth--;

            await LoadMonth();
            selectedDay = DateTime.MinValue;
        }

        private async Task NextMonth()
        {
            if (currentMonth == 12)
            {
                currentMonth = 1;
                currentYear++;
            }
            else currentMonth++;

            await LoadMonth();
            selectedDay = DateTime.MinValue;
        }

        // ---------- inmatning -------------------------------------------------
        private async Task HandleSubmit()
        {
            if (newWorkItem.ProjectId == 0) return;
            await TimeService.AddWorkItemAsync(newWorkItem, currentUserId);
            await LoadMonth();
            SelectDay(selectedDay); // reload items
        }

        private async Task Delete(int id)
        {
            if (await JSRuntime.InvokeAsync<bool>("confirm", "Är du säker?"))
            {
                await TimeService.DeleteWorkItemAsync(id);
                await LoadMonth();
                SelectDay(selectedDay);
            }
        }

        private IEnumerable<(string projectName, double hours)> GetProjectSummary(DateTime day)
        {
            var wd = monthWorkDays.FirstOrDefault(w => w.Date.Date == day.Date);
            if (wd == null)
                return Enumerable.Empty<(string, double)>();

            return wd.WorkItems
                .GroupBy(wi => wi.Project?.Name ?? "Okänt projekt")
                .Select(g => (projectName: g.Key, hours: g.Sum(wi => wi.HoursWorked)));
        }
    }
}