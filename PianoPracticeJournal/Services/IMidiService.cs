namespace PianoPracticeJournal.Services;

/// <summary>
/// Service interface for handling MIDI input detection
/// </summary>
public interface IMidiService : IDisposable
{
    /// <summary>
    /// Fired when a MIDI signal is received
    /// </summary>
    event EventHandler<MidiEventArgs>? MidiSignalReceived;
    
    /// <summary>
    /// Starts monitoring for MIDI input
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stops monitoring for MIDI input
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Gets the list of available MIDI input devices
    /// </summary>
    List<string> GetAvailableDevices();
    
    /// <summary>
    /// Selects a MIDI input device by name
    /// </summary>
    void SelectDevice(string deviceName);
}

/// <summary>
/// Event arguments for MIDI signal events
/// </summary>
public class MidiEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SignalType { get; set; } = string.Empty;
}
