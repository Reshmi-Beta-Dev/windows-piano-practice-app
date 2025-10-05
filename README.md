# Piano Practice Journal

A Windows desktop application that automatically tracks piano practice sessions by monitoring MIDI input from your keyboard.

## Features

- **Automatic MIDI Detection**: Monitors MIDI input to automatically start/stop practice sessions
- **Session Management**: Tracks practice duration, timestamps, and completion status
- **Local Storage**: Stores sessions locally for offline use
- **API Synchronization**: Syncs session data with web APIs
- **System Tray Integration**: Minimizes to system tray with notifications
- **Auto-Start**: Option to launch automatically with Windows
- **Dark Mode Compatible**: Tray icon visible in both light and dark themes

## Requirements

- Windows 10/11
- .NET 9 Runtime (included in self-contained builds)
- MIDI keyboard or controller (optional - app works without MIDI devices)

## Quick Start

### Using the Mock API (Recommended for Testing)

1. **Start the Mock API Server**:
   ```bash
   cd MockApiServer
   dotnet run
   ```

2. **Run the Piano Practice Journal**:
   ```bash
   dotnet run --project PianoPracticeJournal
   ```

3. **Connect your MIDI keyboard** and start playing - sessions will automatically begin recording!

## Configuration

Edit `PianoPracticeJournal/appsettings.json` to customize:

```json
{
  "AppSettings": {
    "MidiInactivityTimeoutSeconds": 60,
    "ApiEndpoint": "http://localhost:5000/api/sessions",
    "ApiTimeoutSeconds": 30,
    "AutoStartWithWindows": false,
    "MinimizeToTray": true,
    "LogLevel": "Information"
  }
}
```

## Mock API Server

The included mock API server is perfect for testing:

- **Session Logging**: Displays all received session data in the console
- **CORS Enabled**: Allows requests from the WPF application
- **Health Check**: `GET /health` endpoint for monitoring

### API Endpoints

- `POST /api/sessions` - Submit practice session data
- `GET /health` - Health check endpoint
- `GET /` - API information

## Building and Deployment

### Development Build
```bash
dotnet build
```

### Self-Contained Deployment
```bash
dotnet publish PianoPracticeJournal -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "publish"
```

## Testing

Run the unit tests:
```bash
dotnet test
```

## Troubleshooting

### MIDI Device Not Found
- **Normal**: App works without MIDI devices connected
- **Solution**: Connect your MIDI keyboard via USB and restart the app

### API Connection Issues
- **Mock API**: Ensure the mock API server is running on port 5000
- **Custom API**: Verify the endpoint URL and network connectivity

### UI Not Visible
- **Check System Tray**: App may be minimized to tray (look for piano icon)
- **Settings**: Set `MinimizeToTray: false` in appsettings.json

### Session Not Starting
- **API Required**: Sessions only start when an API endpoint is configured
- **MIDI Input**: Play keys on your MIDI keyboard to trigger session start

## Project Structure

```
PianoPracticeJournal/
├── PianoPracticeJournal/          # Main WPF application
├── MockApiServer/                 # Mock API for testing
└── PianoPracticeJournal.Tests/    # Unit tests
```

## License

This project is provided as-is for educational and development purposes.