using System.Data.Common;
using System.Net;
using Microsoft.AspNetCore.Components;

namespace TimeTracker.Services;
public class ErrorHandlingService : IErrorHandlingService
{
    private readonly ILogger<ErrorHandlingService> _logger;
    private readonly NavigationManager _navigationManager;
    
    public event Action<string, ErrorSeverity> OnError;
    
    public ErrorHandlingService(ILogger<ErrorHandlingService> logger, NavigationManager navigationManager)
    {
        _logger = logger;
        _navigationManager = navigationManager;
    }
    
    public Task HandleExceptionAsync(Exception ex, string? source = null)
    {
        _logger.LogError(ex, "Ett fel inträffade i {Source}: {Message}", source ?? "okänd källa", ex.Message);
        
        string userMessage = "Ett oväntat fel inträffade. Vänligen försök igen senare.";
        OnError?.Invoke(userMessage, ErrorSeverity.Error);
        
        return Task.CompletedTask;
    }
    
    public Task HandleValidationErrorAsync(string message, string? source = null, IDictionary<string, string[]>? errors = null)
    {
        _logger.LogWarning("Valideringsfel i {Source}: {Message}, Detaljer: {@Errors}", 
            source ?? "okänd källa", message, errors);
    
        OnError?.Invoke(message, ErrorSeverity.Warning);
        
        return Task.CompletedTask;
    }

    public async Task HandleApiErrorAsync(HttpResponseMessage response, string? source = null)
    {
        string content = await response.Content.ReadAsStringAsync();
        
        _logger.LogError("API-fel i {Source}: {StatusCode}, Detaljer: {Content}", 
            source ?? "okänd källa", (int)response.StatusCode, content);
        
        string userMessage = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Du är inte behörig. Vänligen logga in igen.",
            HttpStatusCode.NotFound => "Den begärda resursen kunde inte hittas.",
            HttpStatusCode.BadRequest => "Felaktig förfrågan. Kontrollera din inmatning.",
            _ => "Ett fel uppstod vid kommunikation med servern."
        };
        
        OnError?.Invoke(userMessage, ErrorSeverity.Error);
    }
    
    public Task HandleDatabaseErrorAsync(DbException ex, string? source = null)
    {
        bool isWakeUpTimeout = ex.Message.Contains("Connection Timeout Expired") &&
                               ex.Message.Contains("post-login phase");

        if (isWakeUpTimeout)
        {
            _logger.LogInformation(ex, "Databasaktivering i {Source}:  Azure SQL Database väcks",
                source ?? "okänd källa");
        }
        else
        {
            _logger.LogError(ex, "Databasfel i {Source}: {Message}, ErrorCode: {ErrorCode}", 
                source ?? "okänd källa", ex.Message, ex.ErrorCode);
            
            OnError?.Invoke("Ett fel uppstod vid databasåtkomst. Vänligen försök igen senare.", 
                ErrorSeverity.Error);
        }
    
        return Task.CompletedTask;
    }
    
    public Task<bool> TryRecoverAsync()
    {

        _logger.LogInformation("Försöker återställa applikationsläget efter ett fel");
        
        return Task.FromResult(true);
    }
}