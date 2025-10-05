using NAudio.Midi;
using Microsoft.Extensions.Logging;

namespace PianoPracticeJournal.Services;

/// <summary>
/// Service for detecting MIDI input from digital piano
/// </summary>
public class MidiService : IMidiService
{
    private readonly ILogger<MidiService> _logger;
    private MidiIn? _midiIn;
    private readonly List<string> _availableDevices = new();
    private int _selectedDeviceNumber = 0;
    private bool _disposed = false;

    public event EventHandler<MidiEventArgs>? MidiSignalReceived;

    public MidiService(ILogger<MidiService> logger)
    {
        _logger = logger;
        InitializeDevices();
    }

    public async Task StartAsync()
    {
        if (_midiIn != null)
        {
            _logger.LogWarning("MIDI service is already running");
            return;
        }

        try
        {
            if (_availableDevices.Count == 0)
            {
                _logger.LogWarning("No MIDI input devices found");
                return;
            }

            // Use the first available device by default
            _selectedDeviceNumber = 0;
            _midiIn = new MidiIn(_selectedDeviceNumber);
            _midiIn.MessageReceived += OnMidiMessageReceived;
            _midiIn.ErrorReceived += OnMidiErrorReceived;
            _midiIn.Start();

            _logger.LogInformation("MIDI service started successfully on device: {DeviceName}", 
                _availableDevices[_selectedDeviceNumber]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MIDI service");
            throw;
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        try
        {
            if (_midiIn != null)
            {
                _midiIn.MessageReceived -= OnMidiMessageReceived;
                _midiIn.ErrorReceived -= OnMidiErrorReceived;
                _midiIn.Stop();
                _midiIn.Dispose();
                _midiIn = null;
                
                _logger.LogInformation("MIDI service stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MIDI service");
        }

        await Task.CompletedTask;
    }

    public List<string> GetAvailableDevices()
    {
        return new List<string>(_availableDevices);
    }

    public void SelectDevice(string deviceName)
    {
        var deviceIndex = _availableDevices.IndexOf(deviceName);
        if (deviceIndex == -1)
        {
            throw new ArgumentException($"Device '{deviceName}' not found", nameof(deviceName));
        }

        // Stop current device if running
        if (_midiIn != null)
        {
            StopAsync().Wait();
        }

        // Start new device
        _selectedDeviceNumber = deviceIndex;
        _midiIn = new MidiIn(deviceIndex);
        _midiIn.MessageReceived += OnMidiMessageReceived;
        _midiIn.ErrorReceived += OnMidiErrorReceived;
        _midiIn.Start();

        _logger.LogInformation("Switched to MIDI device: {DeviceName}", deviceName);
    }

    private void InitializeDevices()
    {
        try
        {
            var deviceCount = MidiIn.NumberOfDevices;
            for (int i = 0; i < deviceCount; i++)
            {
                var deviceInfo = MidiIn.DeviceInfo(i);
                _availableDevices.Add(deviceInfo.ProductName);
                _logger.LogDebug("Found MIDI device: {DeviceName}", deviceInfo.ProductName);
            }

            _logger.LogInformation("Found {DeviceCount} MIDI input devices", deviceCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MIDI devices");
        }
    }

    private void OnMidiMessageReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            var timestamp = DateTime.Now;
            var deviceName = _availableDevices.Count > _selectedDeviceNumber ? _availableDevices[_selectedDeviceNumber] : "Unknown";
            
            // Log the MIDI message for debugging
            _logger.LogDebug("MIDI message received: {Message} from {Device}", 
                e.MidiEvent.ToString(), deviceName);

            // Fire event for any MIDI signal (note on, note off, etc.)
            MidiSignalReceived?.Invoke(this, new MidiEventArgs
            {
                Timestamp = timestamp,
                DeviceName = deviceName,
                SignalType = e.MidiEvent.GetType().Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI message");
        }
    }

    private void OnMidiErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        _logger.LogWarning("MIDI error received: {Message}", e.MidiEvent.ToString());
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().Wait();
            _disposed = true;
        }
    }
}
