namespace TimeTracker.Services
{
    public class AiSummaryStateService
    {
        private string _aiSummary = string.Empty;
        public string AiSummary
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

        public void SetAiSummary(string summary)
        {
            AiSummary = summary;
        }

        public void ClearAiSummary()
        {
            AiSummary = string.Empty;
        }
    }
}
