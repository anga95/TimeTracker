using System.Globalization;

namespace TimeTracker.ViewModels
{
    public class MonthNavigationViewModel
    {
        private int _month;
        private int _year;
        
        public int Month 
        { 
            get => _month;
            set
            {
                if (_month != value)
                {
                    _month = value;
                    NotifyStateChanged();
                }
            }
        }
        
        public int Year 
        { 
            get => _year;
            set
            {
                if (_year != value)
                {
                    _year = value;
                    NotifyStateChanged();
                }
            }
        }
        
        public string MonthName => 
            CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
            
        public event Action? StateChanged;
        public event Func<(int Year, int Month), Task>? NavigationRequested;
        
        public async Task PrevMonthClicked()
        {
            int newMonth, newYear;
            
            if (Month == 1)
            {
                newMonth = 12;
                newYear = Year - 1;
            }
            else
            {
                newMonth = Month - 1;
                newYear = Year;
            }
            
            if (NavigationRequested != null)
            {
                await NavigationRequested.Invoke((newYear, newMonth));
            }
        }
        
        public async Task NextMonthClicked()
        {
            int newMonth, newYear;
            
            if (Month == 12)
            {
                newMonth = 1;
                newYear = Year + 1;
            }
            else
            {
                newMonth = Month + 1;
                newYear = Year;
            }
            
            if (NavigationRequested != null)
            {
                await NavigationRequested.Invoke((newYear, newMonth));
            }
        }
        
        private void NotifyStateChanged() => StateChanged?.Invoke();
    }
}