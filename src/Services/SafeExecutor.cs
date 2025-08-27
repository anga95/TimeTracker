using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics;

namespace TimeTracker.Services;

public class SafeExecutor
{
    private readonly IErrorHandlingService _errorHandler;

    public SafeExecutor(IErrorHandlingService errorHandler)
    {
        _errorHandler = errorHandler;
    }
    
    public async Task ExecuteAsync(Func<Task> action)
    {
        string source = GetCallerInfo();
        
        try
        {
            await action();
        }
        catch (DbException ex)
        {
            await _errorHandler.HandleDatabaseErrorAsync(ex, source);
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            var response = new HttpResponseMessage(ex.StatusCode.Value)
            {
                ReasonPhrase = ex.Message
            };
            
            await _errorHandler.HandleApiErrorAsync(response, source);
        }
        catch (ValidationException ex)
        {
            await _errorHandler.HandleValidationErrorAsync(ex.Message, source);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleExceptionAsync(ex, source);
        }
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<T> fallback) where T : notnull
    {
        string source = GetCallerInfo();
        
        try
        {
            return await action();
        }
        catch (DbException ex)
        {
            await _errorHandler.HandleDatabaseErrorAsync(ex, source);
            return fallback();
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            var response = new HttpResponseMessage(ex.StatusCode.Value)
            {
                ReasonPhrase = ex.Message
            };
            
            await _errorHandler.HandleApiErrorAsync(response, source);
            return fallback();
        }
        catch (ValidationException ex)
        {
            await _errorHandler.HandleValidationErrorAsync(ex.Message, source);
            return fallback();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleExceptionAsync(ex, source);
            return fallback();
        }
    }
    
    public Task<List<TItem>> ExecuteListAsync<TItem>(Func<Task<List<TItem>>> action)
        => ExecuteAsync(action, static () => new List<TItem>());
    
    private string GetCallerInfo()
    {
        // Frame 0 is this method (GetCallerInfo)
        // Frame 1 is ExecuteAsync (the calling method in SafeExecutor)
        // Frame 2 is the caller from the user's code
        var frame = new StackTrace().GetFrame(2)?.GetMethod();
        if (frame == null)
        {
            return "UnknownSource";
        }
        
        string className = frame.DeclaringType?.Name ?? "UnknownClass";
        string methodName = frame.Name;
        return $"{className}.{methodName}";
    }
}