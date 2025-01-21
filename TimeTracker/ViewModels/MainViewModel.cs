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

        // Egenskaper för inmatningsfälten
        private string _newProjectName;
        public string NewProjectName
        {
            get => _newProjectName;
            set
            {
                _newProjectName = value;
                OnPropertyChanged(nameof(NewProjectName));
            }
        }

        private string _newHoursWorked;
        public string NewHoursWorked
        {
            get => _newHoursWorked;
            set
            {
                _newHoursWorked = value;
                OnPropertyChanged(nameof(NewHoursWorked));
            }
        }

        private string _newComments;
        public string NewComments
        {
            get => _newComments;
            set
            {
                _newComments = value;
                OnPropertyChanged(nameof(NewComments));
            }
        }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value.Date;
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

        public ICommand SaveCommand { get; }
        public ICommand AddTimeLogEntryCommand { get; }
        public ICommand DeleteTimeLogEntryCommand { get; }

        public MainViewModel()
        {
            dataService = new DataService();
            SelectedDate = DateTime.Today;
            LoadTimeLogsForSelectedDate();

            SaveCommand = new RelayCommand(SaveTimeLogEntries);
            AddTimeLogEntryCommand = new RelayCommand(AddTimeLogEntry);
            DeleteTimeLogEntryCommand = new RelayCommand<TimeLogEntry>(DeleteTimeLogEntry);
        }

        private void LoadTimeLogsForSelectedDate()
        {
            var entries = dataService.LoadTimeLogEntries(SelectedDate);
            TimeLogEntries = new ObservableCollection<TimeLogEntry>(entries);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SaveTimeLogEntries()
        {
            // Validera indata
            double totalHours = TimeLogEntries.Sum(e => e.HoursWorked);

            if (totalHours > 24)
            {
                MessageBox.Show("Du kan inte logga mer än 24 timmar på en dag.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Spara data
            dataService.SaveTimeLogEntries(SelectedDate, TimeLogEntries);

            SaveStatus = "Data sparad!";
        }

        private void AddTimeLogEntry()
        {
            if (double.TryParse(NewHoursWorked, out double hoursWorked))
            {
                var newEntry = new TimeLogEntry
                {
                    ProjectName = NewProjectName,
                    HoursWorked = hoursWorked,
                    Comments = NewComments
                };

                TimeLogEntries.Add(newEntry);

                // Spara omedelbart
                dataService.SaveTimeLogEntries(SelectedDate, TimeLogEntries);

                // Töm inmatningsfälten
                NewProjectName = string.Empty;
                NewHoursWorked = string.Empty;
                NewComments = string.Empty;

                SaveStatus = "Data sparad!";
            }
            else
            {
                MessageBox.Show("Ogiltigt antal timmar. Ange ett numeriskt värde.", "Fel", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTimeLogEntry(TimeLogEntry entry)
        {
            if (entry != null)
            {
                TimeLogEntries.Remove(entry);

                // Spara omedelbart
                dataService.SaveTimeLogEntries(SelectedDate, TimeLogEntries);

                SaveStatus = "Data sparad!";
            }
        }
    }
}