using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using TimeTracker.Services;

namespace TimeTracker.Converters
{
    /// <summary>
    /// 
    /// A IValueConverter that checks how many hours
    /// that are logged for a given date and returns
    /// true (or false) depending on if hours >= 8.
    /// 
    /// In XAML this return (bool) is used for a DataTrigger
    /// 
    /// </summary>
    public class DateToBackgroundConverter : IValueConverter
    {
        public IDataService? DataService { get; set; }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                if (DataService == null) return false;

                var entries = DataService.LoadTimeLogEntries(date);
                double totalHours = entries.Sum(e => e.HoursWorked);

                return (totalHours >= 8);
            }

            // If it's not a valid date - return false
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
