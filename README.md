# Piano Practice Journal

A Windows desktop application that automatically tracks piano practice sessions by monitoring MIDI input from your keyboard. The app records session start/end times and can synchronize data with a web API.

## Table of Contents
- [Features](#features)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Mock API Server](#mock-api-server)
- [Building and Deployment](#building-and-deployment)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Development](#development)
- [API Integration](#api-integration)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Usage Guide](#usage-guide)
- [File Structure](#file-structure)
- [Contributing](#contributing)
- [License](#license)

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

### Option 1: Using the Mock API (Recommended for Testing)

1. **Start the Mock API Server**:
   ```bash
   cd MockApiServer
   dotnet run
   ```
   The mock API will start on `http://localhost:5000`

2. **Run the Piano Practice Journal**:
   ```bash
   dotnet run --project PianoPracticeJournal
   ```

3. **Connect your MIDI keyboard** and start playing - sessions will automatically begin recording!

### Option 2: Using Your Own API

1. **Configure your API endpoint** in `PianoPracticeJournal/appsettings.json`:
   ```json
   {
     "AppSettings": {
       "ApiEndpoint": "https://your-api.com/sessions",
       "ApiTimeoutSeconds": 30
     }
   }
   ```

2. **Run the application**:
   ```bash
   dotnet run --project PianoPracticeJournal
   ```

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

### Settings Explained

- **MidiInactivityTimeoutSeconds**: How long to wait before ending a session (default: 60 seconds)
- **ApiEndpoint**: URL for session data synchronization
- **ApiTimeoutSeconds**: Timeout for API requests
- **AutoStartWithWindows**: Automatically launch app when Windows starts
- **MinimizeToTray**: Minimize to system tray instead of closing
- **LogLevel**: Logging verbosity (Debug, Information, Warning, Error)

## Mock API Server

The included mock API server is perfect for testing and development:

### Features
- **Session Logging**: Displays all received session data in the console
- **CORS Enabled**: Allows requests from the WPF application
- **Health Check**: `GET /health` endpoint for monitoring
- **Error Handling**: Graceful error responses for debugging

### API Endpoints

- `POST /api/sessions` - Submit practice session data
- `GET /health` - Health check endpoint
- `GET /` - API information and usage instructions

### Expected Session Data Format

The app sends JSON data in this format:
```json
{
  "id": "session-guid",
  "startTime": "2024-01-15T10:30:00Z",
  "endTime": "2024-01-15T10:45:00Z",
  "duration": "00:15:00",
  "isCompleted": true,
  "isSynced": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "lastSyncAttempt": null,
  "syncError": null,
  "syncAttemptCount": 0
}
```

## Building and Deployment

### Development Build
```bash
dotnet build
```

### Release Build
```bash
dotnet build -c Release
```

### Self-Contained Deployment
```bash
dotnet publish PianoPracticeJournal -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "publish"
```

### Framework-Dependent Deployment
```bash
dotnet publish PianoPracticeJournal -c Release -o "publish"
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
- **Check**: Look for "Found X MIDI input devices" in the logs

### API Connection Issues
- **Mock API**: Ensure the mock API server is running on port 5000
- **Custom API**: Verify the endpoint URL and network connectivity
- **CORS**: For web APIs, ensure CORS is configured to allow the app

### UI Not Visible
- **Check System Tray**: App may be minimized to tray (look for piano icon)
- **Settings**: Set `MinimizeToTray: false` in appsettings.json
- **Restart**: Close and reopen the application

### Session Not Starting
- **API Required**: Sessions only start when an API endpoint is configured
- **MIDI Input**: Play keys on your MIDI keyboard to trigger session start
- **Timeout**: Sessions auto-end after inactivity (configurable timeout)

## Development

### Project Structure
```
PianoPracticeJournal/
├── Models/           # Data models (PracticeSession, etc.)
├── Services/         # Business logic services
├── Commands/         # WPF command implementations
├── Converters/       # WPF value converters
├── MainWindow.xaml   # Main UI
└── App.xaml.cs       # Application entry point

MockApiServer/
└── Program.cs        # Mock API server

PianoPracticeJournal.Tests/
└── Services/         # Unit tests
```

### Key Services
- **MidiService**: Handles MIDI input detection
- **SessionManager**: Manages practice sessions and local storage
- **ApiClient**: Handles API communication
- **SyncService**: Synchronizes local data with remote API
- **SystemTrayService**: Manages system tray integration

## License

This project is provided as-is for educational and development purposes.