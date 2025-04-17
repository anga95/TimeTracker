using System.Security.Claims;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class AIService : IAIService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;

        private readonly IDbContextFactory<TimeTrackerContext> _dbContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Gränsvärden, lästa från konfiguration
        private readonly int _maxCallsPerMonth;
        private readonly int _minSecondsBetweenCalls;
        // Spåra senaste anropstid (global rate-limit)
        private static DateTime _lastCallTime = DateTime.MinValue;

        // Två systemmeddelanden, som du kan växla mellan
        private const string SummarySystemMessage =
            @"Du är en hjälpsam assistent som skapar tydliga och koncisa summeringar av tidrapportering. 
              Dela upp summeringen per projekt, skriv total arbetad tid med fetstil, lista aktiviteter som punktlista 
              och avsluta med en rad: 'Totalt arbetade timmar: XX'. Svara på svenska.";

        private const string CreativeSystemMessage =
            @"Du är en kreativ assistent som skriver lättsamma men tydliga summeringar av tidrapportering. 
              Dela upp summeringen per projekt, skriv antal timmar med fetstil och lista höjdpunkter som punktlista.
              Avsluta med en rolig kommentar eller metafor. Svara på svenska.";

        public AIService(IConfiguration configuration,
            IDbContextFactory<TimeTrackerContext> dbContextFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            string endpoint = configuration["OpenAI:Endpoint"];     // t.ex. "https://openai-anga.openai.azure.com/"
            string apiKey = configuration["OpenAI:ApiKey"];
            _deploymentName = configuration["OpenAI:DeploymentName"]; // t.ex. "gpt-4o"

            var options = new AzureOpenAIClientOptions();
            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), options);

            _dbContextFactory = dbContextFactory;
            _httpContextAccessor = httpContextAccessor;

            // Läs in gränsvärden från konfiguration, med standardvärden om inte specificerat
            _maxCallsPerMonth = int.Parse(configuration["AIUsage:MaxCallsPerMonth"] ?? "150");
            _minSecondsBetweenCalls = int.Parse(configuration["AIUsage:MinSecondsBetweenCalls"] ?? "10");
        }

        public async Task<ChatResponseResult> GetChatResponseAsync(string prompt, bool creative = true)
        {
            var result = new ChatResponseResult();

            if (IsRateLimited())
            {
                result.IsRateLimited = true;
                result.Summary = "Vänta lite, vänligen försök igen om några sekunder.";
                return result;
            }

            if (!await CanMakeMoreCallsThisMonth())
            {
                result.IsRateLimited = false;
                result.Summary = "Du har nått max antal AI-sammanfattningar för den här månaden.";
                return result;
            }

            _lastCallTime = DateTime.UtcNow;

            var systemMessage = creative ? CreativeSystemMessage : SummarySystemMessage;
            var messages = new ChatMessage[]
            {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(prompt)
            };

            ChatClient chatClient = _client.GetChatClient(_deploymentName);
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages);

            await LogAiCallAsync(prompt);

            result.IsRateLimited = false;
            result.Summary = $"{completion.Role}: {completion.Content[0].Text}";
            return result;
        }
        private bool IsRateLimited()
        {
            return (DateTime.UtcNow - _lastCallTime).TotalSeconds < _minSecondsBetweenCalls;
        }

        private async Task<bool> CanMakeMoreCallsThisMonth()
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            int monthlyCalls = await db.AiUsageLogs.CountAsync(log => log.Timestamp >= monthStart);
            Console.WriteLine($"AI calls this month: {monthlyCalls}/{_maxCallsPerMonth}");
            return monthlyCalls < _maxCallsPerMonth;
        }

        private async Task LogAiCallAsync(string prompt)
        {
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await using var db = _dbContextFactory.CreateDbContext();
            db.AiUsageLogs.Add(new AiUsageLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = string.IsNullOrWhiteSpace(userId) ? "demo" : userId,
                PromptSnippet = prompt?.Length > 200 ? prompt[..200] : prompt
            });
            await db.SaveChangesAsync();
        }

        public async Task<(int monthlyCalls, int maxCalls)> GetUsageInfoAsync()
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            int monthlyCalls = await db.AiUsageLogs.CountAsync(log => log.Timestamp >= monthStart);
            return (monthlyCalls, _maxCallsPerMonth);
        }
        
        public async Task<string?> GetCachedSummaryAsync(string userId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var entry = await db.AISummaries.FirstOrDefaultAsync(s => s.UserId == userId);
            return entry?.Summary;
        }

        public async Task SaveOrUpdateSummaryAsync(string userId, string summary)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var entry = await db.AISummaries.FirstOrDefaultAsync(s => s.UserId == userId);
            if (entry is null)
            {
                entry = new AISummary
                {
                    UserId = userId,
                    Summary = summary,
                    LastUpdated = DateTime.UtcNow
                };
                db.AISummaries.Add(entry);
            }
            else
            {
                entry.Summary = summary;
                entry.LastUpdated = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
        }

        public async Task ClearCachedSummaryAsync(string userId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            var entry = await db.AISummaries.FirstOrDefaultAsync(s => s.UserId == userId);
            if (entry is not null)
            {
                db.AISummaries.Remove(entry);
                await db.SaveChangesAsync();
            }
        }
    }
}
