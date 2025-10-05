# Piano Practice Journal

A Windows application for tracking piano practice sessions by detecting MIDI input from a digital piano. The app automatically records start and stop times and can sync session data to a web API.

## Features

- **MIDI Input Detection**: Automatically detects MIDI signals from connected digital pianos
- **Session Management**: Records practice sessions with start/stop times based on MIDI activity
- **Configurable Timeout**: Automatic session end after configurable period of inactivity
- **Local Storage**: Sessions are stored locally in JSON format for offline access
- **API Synchronization**: Sync sessions to a web API with retry logic for failed attempts
- **System Tray Integration**: Minimize to system tray with notification support
- **Auto-Start**: Option to start with Windows
- **Modern UI**: WPF-based interface with real-time session monitoring

## Requirements

- Windows 10/11
- .NET 9 Runtime
- MIDI-compatible digital piano or MIDI interface
- Internet connection for API synchronization (optional)

## Installation

1. Download the latest release from the releases page
2. Extract the files to a folder of your choice
3. Run `PianoPracticeJournal.exe`

## Configuration

### MIDI Settings

1. Connect your digital piano via USB or MIDI interface
2. Open the application
3. Go to the Settings tab
4. Select your MIDI device from the dropdown
5. Configure the inactivity timeout (default: 60 seconds)

### API Settings

1. In the Settings tab, enter your API endpoint URL
2. Set the API timeout (default: 30 seconds)
3. Click "Test API Connection" to verify connectivity
4. The app will automatically sync sessions when online

### Application Settings

- **Auto-start with Windows**: Adds the app to Windows startup
- **Minimize to system tray**: Hides the app to the system tray instead of closing

## Usage

### Starting a Practice Session

1. Ensure your digital piano is connected and configured
2. Start playing - the app will automatically detect MIDI input
3. A practice session will begin automatically
4. The session will end after the configured period of inactivity

### Monitoring Sessions

- View current session status in the main window
- See all recorded sessions in the Sessions tab
- Monitor sync status and errors
- View session statistics

### Manual Synchronization

- Click "Sync All Unsynced" to manually sync pending sessions
- Use "Refresh" to update the session list
- Failed syncs are automatically retried

## API Integration

The app expects a REST API endpoint that accepts practice session data. The request format is:

```json
{
  "sessionId": "guid",
  "startTime": "2024-01-01T10:00:00Z",
  "endTime": "2024-01-01T10:30:00Z",
  "duration": 1800.0,
  "submittedAt": "2024-01-01T10:30:00Z"
}
```

### Expected Response

```json
{
  "success": true,
  "message": "Session recorded successfully",
  "sessionId": "guid"
}
```

## File Structure

```
PianoPracticeJournal/
├── appsettings.json          # Application configuration
├── PianoPracticeJournal.exe  # Main executable
└── sessions.json             # Local session storage (created automatically)
```

## Data Storage

Sessions are stored locally in `%AppData%\PianoPracticeJournal\sessions.json`. This file contains:

- Session start/end times
- Sync status and error information
- Session metadata

## Troubleshooting

### MIDI Not Detected

1. Ensure your digital piano is connected via USB or MIDI interface
2. Check that the correct MIDI device is selected in settings
3. Verify that other MIDI applications can detect your device
4. Try restarting the application

### API Sync Issues

1. Verify your API endpoint URL is correct
2. Check your internet connection
3. Use "Test API Connection" to diagnose issues
4. Check the application logs for detailed error messages

### Application Won't Start

1. Ensure .NET 9 Runtime is installed
2. Check Windows Event Viewer for error details
3. Run the application as administrator if needed
4. Verify all required files are present

## Development

### Building from Source

1. Install .NET 9 SDK
2. Clone the repository
3. Run `dotnet restore` to restore packages
4. Run `dotnet build` to build the application
5. Run `dotnet run` to start the application

### Project Structure

```
PianoPracticeJournal/
├── Models/                   # Data models
├── Services/                 # Business logic services
├── Commands/                 # WPF command implementations
├── Converters/               # WPF value converters
├── MainWindow.xaml           # Main UI
├── MainWindow.xaml.cs        # Main window code-behind
├── App.xaml                  # Application configuration
├── App.xaml.cs               # Application entry point
└── appsettings.json          # Configuration file
```

### Key Services

- **MidiService**: Handles MIDI input detection
- **SessionManager**: Manages practice sessions and timing
- **ApiClient**: Handles API communication
- **SyncService**: Manages session synchronization
- **StartupService**: Manages Windows startup integration
- **SystemTrayService**: Handles system tray functionality

## License

This project is licensed under the MIT License.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## Support

For issues and questions:

1. Check the troubleshooting section
2. Search existing issues
3. Create a new issue with detailed information
4. Include system information and error logs

## Changelog

### Version 1.0.0
- Initial release
- MIDI input detection
- Session management
- API synchronization
- System tray integration
- Auto-start functionality
- Modern WPF UI
