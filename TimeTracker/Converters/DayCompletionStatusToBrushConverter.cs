using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeTracker.Converters
{
    public class DayCompletionStatusToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dayCompletionStatus = values[0] as Dictionary<DateTime, bool>;
            var date = values[1] as DateTime?;

            if (dayCompletionStatus != null && date.HasValue)
            {
                if (dayCompletionStatus.TryGetValue(date.Value.Date, out bool isComplete))
                {
                    return isComplete ? Brushes.Green : Brushes.Red;
                }
                else
                {
                    return Brushes.Yellow;
                }
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
