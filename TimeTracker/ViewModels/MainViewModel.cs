using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TimeTracker.Models;
using TimeTracker.Services;

namespace TimeTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DataService dataService;

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    LoadTimeLogsForSelectedDate();
                }
            }
        }

        private ObservableCollection<TimeLogEntry> _timeLogEntries;
        public ObservableCollection<TimeLogEntry> TimeLogEntries
        {
            get => _timeLogEntries;
            set
            {
                _timeLogEntries = value;
                OnPropertyChanged(nameof(TimeLogEntries));
            }
        }

        private Dictionary<DateTime, bool> _dayCompletionStatus;
        public Dictionary<DateTime, bool> DayCompletionStatus
        {
            get => _dayCompletionStatus;
            set
            {
                _dayCompletionStatus = value;
                OnPropertyChanged(nameof(DayCompletionStatus));
            }
        }

        private string _saveStatus;
        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                _saveStatus = value;
                OnPropertyChanged(nameof(SaveStatus));
            }
        }

        private HashSet<DateTime> _completedDates;
        public HashSet<DateTime> CompletedDates
        {
            get => _completedDates;
            set
            {
                _completedDates = value;
                OnPropertyChanged(nameof(CompletedDates));
            }
        }

        private HashSet<DateTime> _incompleteDates;
        public HashSet<DateTime> IncompleteDates
        {
            get => _incompleteDates;
            set
            {
                _incompleteDates = value;
                OnPropertyChanged(nameof(IncompleteDates));
            }
        }

        public MainViewModel()
        {
            dataService = new DataService();
            SelectedDate = DateTime.Today;
            LoadTimeLogsForSelectedDate();
            LoadDayCompletionStatus();

            SaveCommand = new RelayCommand(SaveTimeLogEntries);

            LoadMarkedDates();
        }

        private void LoadMarkedDates()
        {
            CompletedDates = new HashSet<DateTime>();
            IncompleteDates = new HashSet<DateTime>();
            var startDate = DateTime.Today.AddMonths(-1);
            var endDate = DateTime.Today.AddMonths(1);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Hoppa över lördagar och söndagar
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                var entries = dataService.LoadTimeLogEntries(date.Date);
                if (entries.Count > 0)
                {
                    double totalHours = entries.Sum(e => e.HoursWorked);
                    if (totalHours >= 8)
                    {
                        CompletedDates.Add(date.Date);
                    }
                    else
                    {
                        IncompleteDates.Add(date.Date);
                    }
                }
                else
                {
                    IncompleteDates.Add(date.Date); // Dag utan inmatningar
                }
            }

            // Uppdatera bindningarna
            OnPropertyChanged(nameof(CompletedDates));
            OnPropertyChanged(nameof(IncompleteDates));
        }

        private void LoadTimeLogsForSelectedDate()
        {
            var entries = dataService.LoadTimeLogEntries(SelectedDate);
            TimeLogEntries = new ObservableCollection<TimeLogEntry>(entries);
        }

        private void LoadDayCompletionStatus()
        {
            DayCompletionStatus = new Dictionary<DateTime, bool>();
            var startDate = DateTime.Today.AddMonths(-1); // Load status for the past month
            for (var date = startDate; date <= DateTime.Today.AddMonths(1); date = date.AddDays(1))
            {
                var entries = dataService.LoadTimeLogEntries(date.Date);
                if (entries.Count == 0)
                {
                    DayCompletionStatus[date.Date] = false; // Not touched
                }
                else
                {
                    DayCompletionStatus[date.Date] = entries.TrueForAll(e => e.IsComplete);
                }
            }
            OnPropertyChanged(nameof(DayCompletionStatus));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand SaveCommand { get; }
        private void SaveTimeLogEntries()
        {
            // Validera indata
            double totalHours = TimeLogEntries.Sum(e => e.HoursWorked);

            if (totalHours > 24)
            {
                MessageBox.Show("Du kan inte logga mer än 24 timmar på en dag.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (totalHours < 8)
            {
                MessageBox.Show("Du har loggat färre än 8 timmar för denna dag.", "Varning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            dataService.SaveTimeLogEntries(SelectedDate, TimeLogEntries);
            LoadMarkedDates(); // Uppdatera markerade datum efter sparande
            SaveStatus = "Data sparad!";
        }
    }
}