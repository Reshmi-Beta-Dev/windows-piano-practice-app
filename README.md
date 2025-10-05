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

## API Integration

### Session Data Format

The application sends session data to your API endpoint in the following JSON format:

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "startTime": "2024-01-15T10:30:00.000Z",
  "endTime": "2024-01-15T10:45:00.000Z",
  "duration": "00:15:00",
  "isCompleted": true,
  "isSynced": false,
  "createdAt": "2024-01-15T10:30:00.000Z",
  "lastSyncAttempt": null,
  "syncError": null,
  "syncAttemptCount": 0
}
```

### Expected API Response

Your API should respond with:

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Session recorded successfully",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2024-01-15T10:45:00.000Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Invalid session data",
  "error": "Missing required field: startTime"
}
```

### CORS Configuration

If your API is web-based, ensure CORS is configured to allow requests from the desktop application:

```csharp
// ASP.NET Core example
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## System Requirements

### Minimum Requirements
- **Operating System**: Windows 10 version 1903 (Build 18362) or later
- **Architecture**: x64 or x86
- **RAM**: 4 GB minimum, 8 GB recommended
- **Storage**: 100 MB free space
- **Network**: Internet connection for API synchronization (optional)

### Recommended Requirements
- **Operating System**: Windows 11
- **RAM**: 8 GB or more
- **Storage**: 1 GB free space
- **MIDI Device**: USB MIDI keyboard or controller
- **Network**: Stable internet connection for real-time sync

### Supported MIDI Devices
- USB MIDI keyboards and controllers
- MIDI interfaces with multiple inputs
- Digital pianos with USB MIDI output
- MIDI-to-USB adapters

## Installation

### Option 1: Self-Contained Executable (Recommended)
1. Download the latest release from the releases page
2. Extract the ZIP file to your desired location
3. Run `PianoPracticeJournal.exe`
4. No additional installation required

### Option 2: Framework-Dependent
1. Ensure .NET 9 Runtime is installed on your system
2. Download the framework-dependent build
3. Run `dotnet PianoPracticeJournal.dll`

### Option 3: Development Build
1. Clone the repository
2. Install .NET 9 SDK
3. Run `dotnet restore`
4. Run `dotnet build`

## Usage Guide

### First Launch
1. **Start the application** - The main window will appear
2. **Check MIDI status** - Look for "Found X MIDI input devices" in the status bar
3. **Configure API** (optional) - Set your API endpoint in Settings
4. **Connect MIDI device** - Plug in your MIDI keyboard if not already connected

### Starting a Practice Session
1. **Ensure MIDI device is connected** and detected by the app
2. **Configure API endpoint** in Settings (required for session recording)
3. **Start playing** your MIDI keyboard - session will begin automatically
4. **Monitor progress** in the main window

### Session Management
- **Automatic Start**: Sessions begin when MIDI input is detected
- **Automatic End**: Sessions end after configured inactivity timeout
- **Manual Control**: Use the UI to start/stop sessions manually
- **Local Storage**: All sessions are saved locally for offline access

### Synchronization
- **Automatic Sync**: Sessions sync to API in the background
- **Manual Sync**: Use the "Sync Now" button for immediate synchronization
- **Sync Status**: Monitor sync status in the main window
- **Retry Logic**: Failed syncs are automatically retried

### System Tray
- **Minimize to Tray**: App can minimize to system tray
- **Tray Icon**: Piano icon visible in both light and dark themes
- **Context Menu**: Right-click tray icon for quick access
- **Notifications**: Get notified of session events

## File Structure

```
PianoPracticeJournal/
├── PianoPracticeJournal.sln              # Solution file
├── README.md                             # This documentation
├── .gitignore                           # Git ignore rules
│
├── PianoPracticeJournal/                 # Main application project
│   ├── PianoPracticeJournal.csproj      # Project file
│   ├── appsettings.json                 # Configuration
│   ├── App.xaml                         # Application definition
│   ├── App.xaml.cs                      # Application entry point
│   ├── MainWindow.xaml                  # Main UI window
│   ├── MainWindow.xaml.cs               # Main window logic
│   │
│   ├── Models/                          # Data models
│   │   └── PracticeSession.cs           # Session data model
│   │
│   ├── Services/                        # Business logic services
│   │   ├── IMidiService.cs              # MIDI service interface
│   │   ├── MidiService.cs               # MIDI input handling
│   │   ├── ISessionManager.cs           # Session management interface
│   │   ├── SessionManager.cs            # Session management logic
│   │   ├── IApiClient.cs                # API client interface
│   │   ├── ApiClient.cs                 # HTTP API communication
│   │   ├── SyncService.cs               # Data synchronization
│   │   ├── IStartupService.cs           # Windows startup interface
│   │   ├── StartupService.cs            # Auto-start functionality
│   │   ├── ISystemTrayService.cs        # System tray interface
│   │   └── SystemTrayService.cs         # System tray management
│   │
│   ├── Commands/                        # WPF command implementations
│   │   └── RelayCommand.cs              # Generic command implementation
│   │
│   └── Converters/                      # WPF value converters
│       └── BooleanToColorConverter.cs   # Boolean to color conversion
│
├── MockApiServer/                       # Mock API for testing
│   ├── MockApiServer.csproj             # Mock API project file
│   └── Program.cs                       # Mock API server implementation
│
└── PianoPracticeJournal.Tests/          # Unit tests
    ├── PianoPracticeJournal.Tests.csproj # Test project file
    ├── TestUtilities/                   # Test helper utilities
    │   └── TestDataBuilder.cs           # Test data creation
    ├── Models/                          # Model tests
    │   └── PracticeSessionTests.cs      # Session model tests
    ├── Services/                        # Service tests
    │   ├── SessionManagerTests.cs       # Session manager tests
    │   ├── ApiClientTests.cs            # API client tests
    │   └── SyncServiceTests.cs          # Sync service tests
    └── Integration/                     # Integration tests
        └── SessionManagementIntegrationTests.cs # End-to-end tests
```

## Contributing

### Development Setup
1. **Clone the repository**
2. **Install prerequisites**:
   - .NET 9 SDK
   - Visual Studio 2022 or VS Code
   - Git
3. **Restore dependencies**: `dotnet restore`
4. **Build the solution**: `dotnet build`
5. **Run tests**: `dotnet test`

### Code Style
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Include unit tests for new features
- Update documentation for API changes

### Pull Request Process
1. **Fork the repository**
2. **Create a feature branch**
3. **Make your changes**
4. **Add/update tests**
5. **Update documentation**
6. **Submit a pull request**

### Reporting Issues
When reporting issues, please include:
- **Operating System** and version
- **Application version**
- **Steps to reproduce**
- **Expected vs actual behavior**
- **Log files** (if available)
- **MIDI device information** (if relevant)

## License

This project is provided as-is for educational and development purposes.

### Third-Party Libraries
- **NAudio**: MIDI input handling (MIT License)
- **Hardcodet.NotifyIcon.Wpf**: System tray integration (MIT License)
- **Microsoft.Extensions.Hosting**: Dependency injection (MIT License)
- **Newtonsoft.Json**: JSON serialization (MIT License)
- **xUnit**: Unit testing framework (Apache 2.0 License)
- **Moq**: Mocking framework (MIT License)
- **FluentAssertions**: Test assertions (Apache 2.0 License)

### Acknowledgments
- Built with .NET 9 and WPF
- Uses modern C# features and async/await patterns
- Implements clean architecture principles
- Follows Microsoft coding standards and best practices