﻿using System.Security.Claims;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using TimeTracker.Data;
using TimeTracker.Models;

namespace TimeTracker.Services
{
    public class AiService : IAiService
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;

        private readonly IDbContextFactory<TimeTrackerContext> _dbContextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SafeExecutor _safeExecutor;

        // Gränsvärden, lästa från konfiguration
        private readonly int _maxCallsPerMonth;
        private readonly int _minSecondsBetweenCalls;
        // Spåra senaste anropstid (global rate-limit)
        private static DateTime _lastCallTime = DateTime.MinValue;

        // Två systemmeddelanden, som du kan växla mellan
        private const string SummarySystemMessage = @"
            Du är en professionell assistent som hjälper användaren att svara sin chef på frågan: 'Vad har du gjort under veckan?'.

            För varje dag:
            - Skriv ett stycke i första person (jag-form), där användaren berättar vad hen har gjort.
            - Inkludera projektnamn, antal timmar, och relevanta kommentarer.
            - Summera gärna dagens totala tid sist i varje stycke.
            - Använd en vänlig och tydlig ton, på svenska.

            Använd gärna punktlista om det blir tydligare, men skriv som till en kollega/chef.
            ";

        public AiService(
            IConfiguration configuration,
            IDbContextFactory<TimeTrackerContext> dbContextFactory,
            IHttpContextAccessor httpContextAccessor,
            SafeExecutor safeExecutor)
        {
            string endpoint = configuration["OpenAI:Endpoint"]
                              ?? throw new InvalidOperationException("OpenAI:Endpoint saknas i konfigurationen.");

            string apiKey = configuration["OpenAI:ApiKey"]
                            ?? throw new InvalidOperationException("OpenAI:ApiKey saknas i konfigurationen.");

            _deploymentName = configuration["OpenAI:DeploymentName"]
                              ?? throw new InvalidOperationException("OpenAI:DeploymentName saknas i konfigurationen."); // t.ex. "gpt-4o"
            

            var options = new AzureOpenAIClientOptions();
            _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), options);

            _dbContextFactory = dbContextFactory;
            _httpContextAccessor = httpContextAccessor;
            _safeExecutor = safeExecutor;

            // Läs in gränsvärden från konfiguration, med standardvärden om inte specificerat
            _maxCallsPerMonth = int.Parse(configuration["AIUsage:MaxCallsPerMonth"] ?? "150");
            _minSecondsBetweenCalls = int.Parse(configuration["AIUsage:MinSecondsBetweenCalls"] ?? "10");
        }

        public async Task<ChatResponseResult> GetChatResponseAsync(string prompt)
        {
            return await _safeExecutor.ExecuteAsync(async () =>
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

                var messages = new ChatMessage[]
                {
                    new SystemChatMessage(SummarySystemMessage),
                    new UserChatMessage(prompt)
                };

                ChatClient chatClient = _client.GetChatClient(_deploymentName);
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages);

                await LogAiCallAsync(prompt);

                result.IsRateLimited = false;
                var raw = completion.Content[0].Text?.Trim() ?? "";

                if (raw.StartsWith("Assistant:", StringComparison.OrdinalIgnoreCase))
                {
                    raw = raw.Substring("Assistant:".Length).Trim();
                }

                result.Summary = raw;
                return result;
            }, new ChatResponseResult 
            { 
                IsRateLimited = false,
                Summary = "Ett fel uppstod när AI-sammanfattningen skulle genereras. Försök igen senare."
            });
        }
        
        private bool IsRateLimited()
        {
            return (DateTime.UtcNow - _lastCallTime).TotalSeconds < _minSecondsBetweenCalls;
        }

        private async Task<bool> CanMakeMoreCallsThisMonth()
        {
            return await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var db = _dbContextFactory.CreateDbContext();
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                int monthlyCalls = await db.AiUsageLogs.CountAsync(log => log.Timestamp >= monthStart);
                Console.WriteLine($"AI calls this month: {monthlyCalls}/{_maxCallsPerMonth}");
                return monthlyCalls < _maxCallsPerMonth;
            }, true);
        }

        private async Task LogAiCallAsync(string prompt)
        {
            await _safeExecutor.ExecuteAsync(async () =>
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
            });
        }

        public async Task<(int monthlyCalls, int maxCalls)> GetUsageInfoAsync()
        {
            return await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var db = _dbContextFactory.CreateDbContext();
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                int monthlyCalls = await db.AiUsageLogs.CountAsync(log => log.Timestamp >= monthStart);
                return (monthlyCalls, _maxCallsPerMonth);
            });
        }
        
        public async Task<string?> GetCachedSummaryAsync(string userId)
        {
            return await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var db = _dbContextFactory.CreateDbContext();
                var entry = await db.AiSummaries.FirstOrDefaultAsync(s => s.UserId == userId);
                return entry?.Summary;
            });
        }

        public async Task SaveOrUpdateSummaryAsync(string userId, string summary)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var db = _dbContextFactory.CreateDbContext();
                var entry = await db.AiSummaries.FirstOrDefaultAsync(s => s.UserId == userId);
                if (entry is null)
                {
                    entry = new AiSummary
                    {
                        UserId = userId,
                        Summary = summary,
                        LastUpdated = DateTime.UtcNow
                    };
                    db.AiSummaries.Add(entry);
                }
                else
                {
                    entry.Summary = summary;
                    entry.LastUpdated = DateTime.UtcNow;
                }

                await db.SaveChangesAsync();
            });
        }

        public async Task ClearCachedSummaryAsync(string userId)
        {
            await _safeExecutor.ExecuteAsync(async () =>
            {
                await using var db = _dbContextFactory.CreateDbContext();
                var entry = await db.AiSummaries.FirstOrDefaultAsync(s => s.UserId == userId);
                if (entry is not null)
                {
                    db.AiSummaries.Remove(entry);
                    await db.SaveChangesAsync();
                }
            });
        }
    }
}