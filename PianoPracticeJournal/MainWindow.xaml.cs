using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PianoPracticeJournal.Models;
using PianoPracticeJournal.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace PianoPracticeJournal;

/// <summary>
/// Main window for the Piano Practice Journal application
/// </summary>
public partial class MainWindow : Window
{
    private IHost? _host;
    private ILogger<MainWindow>? _logger;
    private IMidiService? _midiService;
    private ISessionManager? _sessionManager;
    private ISyncService? _syncService;
    private AppSettings? _settings;
    
    private readonly ObservableCollection<PracticeSession> _sessions = new();
    private readonly DispatcherTimer _uiUpdateTimer;
    private readonly DispatcherTimer _sessionDurationTimer;

    // Parameterless constructor for XAML
    public MainWindow()
    {
        InitializeComponent();
        System.Diagnostics.Debug.WriteLine("MainWindow constructor called and InitializeComponent completed");
        
        // Initialize with null values - will be set via SetServices method
        _host = null;
        _logger = null;
        _midiService = null;
        _sessionManager = null;
        _syncService = null;
        _settings = null;
        
        // Set up data binding
        SessionsListView.ItemsSource = _sessions;
        
        // Add converter to resources
        Resources.Add("BooleanToColorConverter", new Converters.BooleanToColorConverter());

        // Set up UI update timer (every 5 seconds)
        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _uiUpdateTimer.Tick += OnUiUpdateTimerTick;

        // Set up session duration timer (every second)
        _sessionDurationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _sessionDurationTimer.Tick += OnSessionDurationTimerTick;
    }

    // Method to set services after construction
    public void SetServices(
        IHost host,
        ILogger<MainWindow> logger,
        IMidiService midiService,
        ISessionManager sessionManager,
        ISyncService syncService,
        AppSettings settings)
    {
        _host = host;
        _logger = logger;
        _midiService = midiService;
        _sessionManager = sessionManager;
        _syncService = syncService;
        _settings = settings;

        // Subscribe to events
        _midiService.MidiSignalReceived += OnMidiSignalReceived;
        _sessionManager.SessionStarted += OnSessionStarted;
        _sessionManager.SessionEnded += OnSessionEnded;
        _syncService.SyncCompleted += OnSyncCompleted;
        _syncService.SyncFailed += OnSyncFailed;

        // Initialize UI
        InitializeUI();
        
        // Start timers
        _uiUpdateTimer.Start();
        _sessionDurationTimer.Start();

        _logger.LogInformation("Main window initialized");
    }

    private async void InitializeUI()
    {
        try
        {
            if (_midiService == null || _logger == null || _settings == null) return;
            
            // Load MIDI devices
            var devices = _midiService.GetAvailableDevices();
            MidiDeviceComboBox.ItemsSource = devices;
            
            if (devices.Count > 0)
            {
                MidiDeviceComboBox.SelectedIndex = 0;
            }

            // Load settings
            InactivityTimeoutTextBox.Text = _settings.MidiInactivityTimeoutSeconds.ToString();
            ApiEndpointTextBox.Text = _settings.ApiEndpoint;
            ApiTimeoutTextBox.Text = _settings.ApiTimeoutSeconds.ToString();
            AutoStartCheckBox.IsChecked = _settings.AutoStartWithWindows;

            // Load sessions
            await RefreshSessionsAsync();
            await UpdateStatisticsAsync();

            _logger.LogInformation("UI initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error initializing UI");
            MessageBox.Show($"Error initializing UI: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnMidiSignalReceived(object? sender, MidiEventArgs e)
    {
        try
        {
            if (_sessionManager == null) return;
            await _sessionManager.OnMidiSignalReceivedAsync();
            
            // Update UI on UI thread
            Dispatcher.Invoke(() =>
            {
                MidiStatusText.Text = "Connected";
                MidiStatusText.Foreground = System.Windows.Media.Brushes.Green;
                StatusBarText.Text = $"MIDI signal received at {e.Timestamp:HH:mm:ss}";
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling MIDI signal");
        }
    }

    private void OnSessionStarted(object? sender, SessionEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = "Recording";
            CurrentSessionStatus.Text = "Active";
            CurrentSessionStartTime.Text = e.Session.StartTime.ToString("HH:mm:ss");
            StatusBarText.Text = "Practice session started";
        });
    }

    private void OnSessionEnded(object? sender, SessionEventArgs e)
    {
        Dispatcher.Invoke(async () =>
        {
            StatusText.Text = "Stopped";
            CurrentSessionStatus.Text = "No active session";
            CurrentSessionStartTime.Text = "--";
            CurrentSessionDuration.Text = "--";
            StatusBarText.Text = $"Practice session ended (Duration: {e.Session.Duration:hh\\:mm\\:ss})";
            
            await RefreshSessionsAsync();
            await UpdateStatisticsAsync();
        });
    }

    private void OnSyncCompleted(object? sender, SyncEventArgs e)
    {
        Dispatcher.Invoke(async () =>
        {
            SyncStatusText.Text = $"Synced {e.SyncedCount} sessions in {e.Duration.TotalSeconds:F1}s";
            StatusBarText.Text = $"Sync completed: {e.SyncedCount} synced, {e.FailedCount} failed";
            
            await RefreshSessionsAsync();
            await UpdateStatisticsAsync();
        });
    }

    private void OnSyncFailed(object? sender, SyncErrorEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            SyncStatusText.Text = $"Sync failed for session {e.Session.Id}";
            StatusBarText.Text = $"Sync failed: {e.Error.Message}";
        });
    }

    private async void OnUiUpdateTimerTick(object? sender, EventArgs e)
    {
        await RefreshSessionsAsync();
        await UpdateStatisticsAsync();
    }

    private void OnSessionDurationTimerTick(object? sender, EventArgs e)
    {
        if (_sessionManager?.CurrentSession != null)
        {
            var duration = DateTime.Now - _sessionManager.CurrentSession.StartTime;
            CurrentSessionDuration.Text = duration.ToString(@"hh\:mm\:ss");
        }
    }

    private async Task RefreshSessionsAsync()
    {
        try
        {
            if (_sessionManager == null) return;
            var sessions = await _sessionManager.GetAllSessionsAsync();
            
            Dispatcher.Invoke(() =>
            {
                _sessions.Clear();
                foreach (var session in sessions.OrderByDescending(s => s.StartTime))
                {
                    _sessions.Add(session);
                }
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error refreshing sessions");
        }
    }

    private async Task UpdateStatisticsAsync()
    {
        try
        {
            if (_syncService == null) return;
            var stats = await _syncService.GetSyncStatisticsAsync();
            
            Dispatcher.Invoke(() =>
            {
                TotalSessionsText.Text = stats.TotalSessions.ToString();
                SyncedSessionsText.Text = stats.SyncedSessions.ToString();
                UnsyncedSessionsText.Text = stats.UnsyncedSessions.ToString();
                FailedSessionsText.Text = stats.FailedSessions.ToString();
                LastSyncText.Text = stats.LastSyncTime?.ToString("MM/dd/yyyy HH:mm:ss") ?? "Never";
            });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating statistics");
        }
    }

    private async void SyncAllButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_syncService == null) return;
            SyncAllButton.IsEnabled = false;
            SyncStatusText.Text = "Syncing...";
            
            await _syncService.SyncAllUnsyncedSessionsAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during manual sync");
            MessageBox.Show($"Sync failed: {ex.Message}", "Sync Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SyncAllButton.IsEnabled = true;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshSessionsAsync();
        await UpdateStatisticsAsync();
    }

    private async void MidiDeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MidiDeviceComboBox.SelectedItem is string deviceName && !string.IsNullOrEmpty(deviceName))
        {
            try
            {
                _midiService?.SelectDevice(deviceName);
                StatusBarText.Text = $"Switched to MIDI device: {deviceName}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error switching MIDI device");
                MessageBox.Show($"Error switching MIDI device: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void InactivityTimeoutTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(InactivityTimeoutTextBox.Text, out int timeout) && timeout > 0 && _settings != null)
        {
            _settings.MidiInactivityTimeoutSeconds = timeout;
        }
    }

    private void ApiEndpointTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_settings != null)
        {
            _settings.ApiEndpoint = ApiEndpointTextBox.Text;
        }
    }

    private void ApiTimeoutTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(ApiTimeoutTextBox.Text, out int timeout) && timeout > 0 && _settings != null)
        {
            _settings.ApiTimeoutSeconds = timeout;
        }
    }

    private async void TestApiButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_host == null) return;
            TestApiButton.IsEnabled = false;
            ApiTestResult.Text = "Testing...";
            
            var apiClient = _host.Services.GetRequiredService<IApiClient>();
            var isConnected = await apiClient.TestConnectivityAsync();
            
            ApiTestResult.Text = isConnected ? "✓ Connected" : "✗ Failed";
            ApiTestResult.Foreground = isConnected ? 
                System.Windows.Media.Brushes.Green : 
                System.Windows.Media.Brushes.Red;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error testing API connection");
            ApiTestResult.Text = "✗ Error";
            ApiTestResult.Foreground = System.Windows.Media.Brushes.Red;
        }
        finally
        {
            TestApiButton.IsEnabled = true;
        }
    }

    private void AutoStartCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_settings != null)
        {
            _settings.AutoStartWithWindows = AutoStartCheckBox.IsChecked == true;
            // TODO: Implement auto-start with Windows functionality
        }
    }


    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _logger?.LogInformation("Application shutting down");
            
            // Stop timers
            _uiUpdateTimer?.Stop();
            _sessionDurationTimer?.Stop();
            
            // Stop MIDI service
            _midiService?.Dispose();
            
            // Stop host
            _host?.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during application shutdown");
        }
        
        base.OnClosing(e);
    }
}
