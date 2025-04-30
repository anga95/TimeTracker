using System.Data.Common;

namespace TimeTracker.Services;

public interface IErrorHandlingService
{
    Task HandleExceptionAsync(Exception ex, string source = null);
    Task HandleValidationErrorAsync(string message, string source = null, IDictionary<string, string[]> errors = null);
    Task HandleApiErrorAsync(HttpResponseMessage response, string source = null);
    Task HandleDatabaseErrorAsync(DbException ex, string source = null);
    Task<bool> TryRecoverAsync();
    event Action<string, ErrorSeverity> OnError;
}


public enum ErrorSeverity
{
    Information,
    Warning,
    Error,
    Critical
}
