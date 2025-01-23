using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TimeTracker.Services;

namespace TimeTracker.Converters
{
    public class DayColorConverter : IMultiValueConverter
    {
        public IDataService? DataService { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (DataService == null)
                return Brushes.Transparent;

            // values[0] = CalendarDayButton.Content (oftast int dagnummer)
            // values[1] = Calendar.DisplayDate (year + month)
            if (values[0] is string dayString
                && int.TryParse(dayString, out int dayNumber)
                && values[1] is DateTime displayDate)
            {
                // Bygg ihop ett riktigt datum
                // Obs! Om dagnumret hör till föregående/nästa månad kan du behöva extra logik.
                var date = new DateTime(displayDate.Year, displayDate.Month, dayNumber);

                var entries = DataService.LoadTimeLogEntries(date);
                double totalHours = entries.Sum(e => e.HoursWorked);

                return (totalHours >= 8) ? Brushes.LightGreen : Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
