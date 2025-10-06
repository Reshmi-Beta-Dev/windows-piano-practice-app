using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace PianoPracticeJournal;

/// <summary>
/// Application entry point and configuration
/// </summary>
public partial class App : Application
{
    private IHost? _host;
    private ISystemTrayService? _systemTrayService;

    // Windows API methods to force window to foreground
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Build host with dependency injection
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configure logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });

                    // Add configuration
                    var appSettings = new AppSettings();
                    context.Configuration.GetSection("AppSettings").Bind(appSettings);
                    services.AddSingleton(appSettings);

                    // Add services
                    services.AddSingleton<IMidiService, MidiService>();
                    services.AddSingleton<ISessionManager, SessionManager>();
                    services.AddSingleton<IApiClient, ApiClient>();
                    services.AddSingleton<ISyncService, SyncService>();
                    services.AddSingleton<IStartupService, StartupService>();
                    services.AddSingleton<ISystemTrayService, SystemTrayService>();
                    services.AddSingleton<HttpClient>();
                })
                .Build();

            // Start host
            await _host.StartAsync();

            // Get services
            var logger = _host.Services.GetRequiredService<ILogger<App>>();
            _systemTrayService = _host.Services.GetRequiredService<ISystemTrayService>();

            logger.LogInformation("Application starting up");

            // Create and show main window
            var mainWindow = new MainWindow();
            logger.LogInformation("MainWindow created successfully");
            
            // Set as main window - this is crucial for WPF
            Current.MainWindow = mainWindow;
            
            // Temporarily comment out SetServices to test window visibility
            // mainWindow.SetServices(
            //     _host,
            //     _host.Services.GetRequiredService<ILogger<MainWindow>>(),
            //     _host.Services.GetRequiredService<IMidiService>(),
            //     _host.Services.GetRequiredService<ISessionManager>(),
            //     _host.Services.GetRequiredService<ISyncService>(),
            //     _host.Services.GetRequiredService<AppSettings>());
            // Temporarily disable system tray to test window visibility
            // _systemTrayService.Initialize(mainWindow);
            
            // Force window to be visible and shown using multiple methods
            mainWindow.Left = 100;
            mainWindow.Top = 100;
            mainWindow.Width = 800;
            mainWindow.Height = 600;
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Visibility = Visibility.Visible;
            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.Focus();
            mainWindow.BringIntoView();
            
            // Force the window to be on top temporarily
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            
            // Use Windows API to force window to foreground
            var windowHandle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
            ShowWindow(windowHandle, SW_RESTORE);
            ShowWindow(windowHandle, SW_SHOW);
            SetForegroundWindow(windowHandle);
            
            // Additional forced visibility
            mainWindow.UpdateLayout();
            mainWindow.InvalidateVisual();
            
            logger.LogInformation("MainWindow shown and activated");

            // Start MIDI service
            var midiService = _host.Services.GetRequiredService<IMidiService>();
            await midiService.StartAsync();

            logger.LogInformation("Application started successfully");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start application: {ex.Message}", "Startup Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogInformation("Application shutting down");

                // Stop MIDI service
                var midiService = _host.Services.GetRequiredService<IMidiService>();
                await midiService.StopAsync();

                // Dispose system tray
                _systemTrayService?.Dispose();

                // Stop host
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't prevent shutdown
            System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
        }

        base.OnExit(e);
    }
}
