using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging;
using PianoPracticeJournal.Commands;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for managing system tray functionality
/// </summary>
public class SystemTrayService : ISystemTrayService
{
    private readonly ILogger<SystemTrayService> _logger;
    private TaskbarIcon? _taskbarIcon;
    private Window? _mainWindow;
    private bool _disposed = false;

    public SystemTrayService(ILogger<SystemTrayService> logger)
    {
        _logger = logger;
    }

    public void Initialize(Window mainWindow)
    {
        try
        {
            _mainWindow = mainWindow;
            
            // Create system tray icon
            _taskbarIcon = new TaskbarIcon();
            
            // Set custom icon
            _taskbarIcon.IconSource = CreateTrayIcon();
            
            // Set up context menu
            var contextMenu = CreateContextMenu();
            _taskbarIcon.ContextMenu = contextMenu;
            
            // Set up double-click to show window
            _taskbarIcon.DoubleClickCommand = new PianoPracticeJournal.Commands.RelayCommand(() => ShowMainWindow());
            
            // Set initial tooltip
            UpdateTooltip("Piano Practice Journal - Stopped");
            
            // Handle main window state changes
            _mainWindow.StateChanged += OnMainWindowStateChanged;
            _mainWindow.Closing += OnMainWindowClosing;
            
            _logger.LogInformation("System tray initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing system tray");
        }
    }

    public void ShowMainWindow()
    {
        if (_mainWindow == null) return;

        try
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
            _mainWindow.Topmost = true;
            _mainWindow.Topmost = false;
            _mainWindow.Focus();
            
            _logger.LogDebug("Main window shown from system tray");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing main window");
        }
    }

    public void HideToTray()
    {
        if (_mainWindow == null) return;

        try
        {
            _mainWindow.Hide();
            ShowNotification("Piano Practice Journal", "Application minimized to system tray");
            
            _logger.LogDebug("Main window hidden to system tray");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hiding main window to tray");
        }
    }

    public void ShowNotification(string title, string message)
    {
        try
        {
            _taskbarIcon?.ShowBalloonTip(title, message, BalloonIcon.Info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing notification");
        }
    }

    public void UpdateTooltip(string tooltip)
    {
        try
        {
            if (_taskbarIcon != null)
            {
                _taskbarIcon.ToolTipText = tooltip;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tooltip");
        }
    }

    private System.Windows.Controls.ContextMenu CreateContextMenu()
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem
        {
            Header = "Show Piano Practice Journal",
            Command = new PianoPracticeJournal.Commands.RelayCommand(() => ShowMainWindow())
        };
        menu.Items.Add(showItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem
        {
            Header = "Exit",
            Command = new PianoPracticeJournal.Commands.RelayCommand(() => ExitApplication())
        };
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnMainWindowStateChanged(object? sender, EventArgs e)
    {
        // Only hide to tray if minimize to tray is enabled
        if (_mainWindow?.WindowState == WindowState.Minimized)
        {
            // Don't automatically hide to tray on minimize - let user control this
            _logger.LogDebug("Main window minimized");
        }
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Only hide to tray on close if minimize to tray is enabled in settings
        // For now, allow normal close behavior
        _logger.LogDebug("Main window closing");
    }

    private void ExitApplication()
    {
        try
        {
            _logger.LogInformation("Application exit requested from system tray");
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application exit");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            try
            {
                if (_mainWindow != null)
                {
                    _mainWindow.StateChanged -= OnMainWindowStateChanged;
                    _mainWindow.Closing -= OnMainWindowClosing;
                }

                _taskbarIcon?.Dispose();
                _taskbarIcon = null;
                
                _disposed = true;
                _logger.LogInformation("System tray disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing system tray");
            }
        }
    }

    private BitmapSource CreateTrayIcon()
    {
        // Create a 16x16 icon with a white piano symbol on transparent background
        // This will be visible in both light and dark modes
        var drawingVisual = new DrawingVisual();
        
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            // Set background as transparent
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 16, 16));
            
            // Draw a simple white piano icon (keyboard shape)
            var whitePen = new Pen(Brushes.White, 1.5);
            
            // Piano keys outline
            drawingContext.DrawRectangle(null, whitePen, new Rect(2, 4, 12, 8));
            
            // Black keys
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(4, 4, 1.5, 4));
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(7, 4, 1.5, 4));
            drawingContext.DrawRectangle(Brushes.White, null, new Rect(10.5, 4, 1.5, 4));
        }
        
        var renderTargetBitmap = new RenderTargetBitmap(16, 16, 96, 96, PixelFormats.Pbgra32);
        renderTargetBitmap.Render(drawingVisual);
        
        return renderTargetBitmap;
    }
}
