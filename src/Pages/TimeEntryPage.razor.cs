using Microsoft.AspNetCore.Components;
using TimeTracker.Components;
using TimeTracker.ViewModels;

namespace TimeTracker.Pages
{
    public partial class TimeEntryPage : IDisposable
    {
        [Inject] private TimeEntryViewModel ViewModel { get; set; } = null!;

        private CalendarGrid _calendarGrid = null!;
        private bool initialized = false;

        protected override async Task OnInitializedAsync()
        {
            ViewModel.StateChanged += OnViewModelStateChanged;
            await ViewModel.InitializeAsync();
            initialized = true;
        }

        private void OnViewModelStateChanged() => StateHasChanged();

        private async Task RefreshCalendarData(int _)
        {
            if (_calendarGrid != null)
            {
                await _calendarGrid.RefreshDataAsync();
            }
        }

        public void Dispose()
        {
            ViewModel.StateChanged -= OnViewModelStateChanged;
        }
    }
}