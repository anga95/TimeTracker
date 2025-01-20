using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.Converters
{
    public class DateToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DateTime date &&
                values[1] is HashSet<DateTime> completedDates &&
                values[2] is HashSet<DateTime> incompleteDates)
            {
                // Kontrollera om datumet är en lördag eller söndag
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return DependencyProperty.UnsetValue; // Behåll standardstilen
                }

                if (completedDates.Contains(date.Date))
                {
                    return Brushes.Green;
                }
                else if (incompleteDates.Contains(date.Date))
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Yellow; // Datum utan inmatningar
                }
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}