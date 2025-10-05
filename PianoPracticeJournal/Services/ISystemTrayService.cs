using System.Windows;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for managing system tray functionality
/// </summary>
public interface ISystemTrayService
{
    /// <summary>
    /// Shows the main window
    /// </summary>
    void ShowMainWindow();
    
    /// <summary>
    /// Hides the main window to tray
    /// </summary>
    void HideToTray();
    
    /// <summary>
    /// Shows a notification balloon
    /// </summary>
    void ShowNotification(string title, string message);
    
    /// <summary>
    /// Updates the tray icon tooltip
    /// </summary>
    void UpdateTooltip(string tooltip);
    
    /// <summary>
    /// Initializes the system tray
    /// </summary>
    void Initialize(Window mainWindow);
    
    /// <summary>
    /// Disposes the system tray
    /// </summary>
    void Dispose();
}
