using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for managing Windows startup behavior
/// </summary>
public class StartupService : IStartupService
{
    private readonly ILogger<StartupService> _logger;
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "PianoPracticeJournal";

    public StartupService(ILogger<StartupService> logger)
    {
        _logger = logger;
    }

    public async Task SetAutoStartAsync(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null)
            {
                _logger.LogError("Failed to open registry key for startup settings");
                return;
            }

            if (enabled)
            {
                var applicationPath = GetApplicationPath();
                key.SetValue(AppName, $"\"{applicationPath}\"");
                _logger.LogInformation("Auto-start enabled for Piano Practice Journal");
            }
            else
            {
                key.DeleteValue(AppName, false);
                _logger.LogInformation("Auto-start disabled for Piano Practice Journal");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting auto-start registry key");
            throw;
        }

        await Task.CompletedTask;
    }

    public async Task<bool> IsAutoStartEnabledAsync()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            if (key == null)
            {
                return false;
            }

            var value = key.GetValue(AppName);
            return value != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking auto-start registry key");
            return false;
        }
    }

    public string GetApplicationPath()
    {
        return System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? 
               System.Reflection.Assembly.GetExecutingAssembly().Location;
    }
}
