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

        public MainViewModel()
        {
            dataService = new DataService();
            SelectedDate = DateTime.Today;
            LoadTimeLogsForSelectedDate();

            SaveCommand = new RelayCommand(SaveTimeLogEntries);

            CheckForMissingEntries();
        }

        private void LoadTimeLogsForSelectedDate()
        {
            TimeLogEntries = new ObservableCollection<TimeLogEntry>(dataService.LoadTimeLogEntries(SelectedDate));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand SaveCommand { get; }
        private void SaveTimeLogEntries()
        {
            // Validera indata
            double totalHours = 0;
            foreach (var entry in TimeLogEntries)
            {
                totalHours += entry.HoursWorked;
            }

            if (totalHours < 8)
            {
                MessageBox.Show("Du har loggat färre än 8 timmar för denna dag.", "Varning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            foreach (var entry in TimeLogEntries)
            {
                entry.IsComplete = true;
            }

            dataService.SaveTimeLogEntries(SelectedDate, TimeLogEntries);
        }

        private void CheckForMissingEntries()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var entries = dataService.LoadTimeLogEntries(yesterday);
            if (entries == null || entries.Count == 0 || !entries.TrueForAll(e => e.IsComplete))
            {
                MessageBox.Show("Du har inte loggat arbetstid för gårdagen.", "Påminnelse", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
