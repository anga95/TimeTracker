namespace TimeTracker.Services
{
    public class AISummaryStateService
    {
        private string _aiSummary = string.Empty;
        public string AISummary
        {
            get => _aiSummary;
            private set
            {
                _aiSummary = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();

        public void SetAISummary(string summary)
        {
            AISummary = summary;
        }

        public void ClearAISummary()
        {
            AISummary = string.Empty;
        }
    }
}
