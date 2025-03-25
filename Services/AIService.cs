namespace TimeTracker.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _endpoint = configuration["OpenAI:Endpoint"];
        }

        public async Task<string> GetSummaryAsync(string prompt)
        {
            var requestData = new
            {
                prompt = prompt,
                max_tokens = 150,
                temperature = 0.7
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_endpoint, requestData);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
                return result?.Choices?.FirstOrDefault()?.Text?.Trim()
                       ?? "Ingen sammanfattning kunde genereras.";
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("429"))
            {
                return "För många anrop – försök igen senare.";
            }
        }

    }

    public class OpenAiResponse
    {
        public List<Choice> Choices { get; set; } = new();
    }

    public class Choice
    {
        public string Text { get; set; } = string.Empty;
    }
}
