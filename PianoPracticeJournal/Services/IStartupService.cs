namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for managing Windows startup behavior
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Enables or disables auto-start with Windows
    /// </summary>
    Task SetAutoStartAsync(bool enabled);
    
    /// <summary>
    /// Checks if auto-start is currently enabled
    /// </summary>
    Task<bool> IsAutoStartEnabledAsync();
    
    /// <summary>
    /// Gets the application executable path
    /// </summary>
    string GetApplicationPath();
}
