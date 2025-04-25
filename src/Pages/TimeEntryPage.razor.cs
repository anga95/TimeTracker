using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TimeTracker.Components;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;

namespace TimeTracker.Pages
{
    public partial class TimeEntryPage : IDisposable
    {
        [Inject] private ITimeTrackingService TimeService { get; set; } = null!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = null!;

        private TimeEntryViewModel _viewModel = null!;
        private CalendarGrid _calendarGrid = null!;


        protected override async Task OnInitializedAsync()
        {
            _viewModel = new TimeEntryViewModel(TimeService, AuthProvider);
            _viewModel.StateChanged += OnViewModelStateChanged;
            await _viewModel.InitializeAsync();
        }

        private void OnViewModelStateChanged() => StateHasChanged();

        private void HandleMonthDataLoaded(List<WorkDay> workDays)
        {
            _viewModel.HandleMonthDataLoaded(workDays);
        }

        private void HandleMonthChanged((int Year, int Month) newDate)
        {
            _viewModel.HandleMonthChanged(newDate);
        }

        private void HandleDaySelected(DateTime day)
        {
            _viewModel.HandleDaySelected(day);
        }

        private async Task RefreshCalendarData(int _)
        {
            if (_calendarGrid != null)
            {
                await _calendarGrid.RefreshDataAsync();
            }
        }

        private void HandleTimeEntriesChanged(List<TimeEntry> items)
        {
            _viewModel.HandleTimeEntriesChanged(items);
        }

        public void Dispose()
        {
            if (_viewModel != null)
            {
                _viewModel.StateChanged -= OnViewModelStateChanged;
            }
        }
    }
}