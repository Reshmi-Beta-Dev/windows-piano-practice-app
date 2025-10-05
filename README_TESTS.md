# Piano Practice Journal - Unit Tests

This directory contains comprehensive unit tests for the Piano Practice Journal application.

## Test Structure

```
PianoPracticeJournal.Tests/
├── Models/                           # Tests for data models
│   └── PracticeSessionTests.cs      # Tests for PracticeSession model
├── Services/                         # Tests for business logic services
│   ├── SessionManagerTests.cs       # Tests for session management
│   ├── ApiClientTests.cs            # Tests for API client
│   ├── SyncServiceTests.cs          # Tests for sync service
│   └── StartupServiceTests.cs       # Tests for startup service
├── Integration/                      # Integration tests
│   └── SessionManagementIntegrationTests.cs
├── TestUtilities/                    # Test helper utilities
│   ├── TestDataBuilder.cs           # Data builder for test objects
│   └── MockHelpers.cs               # Mock helper utilities
└── appsettings.json                 # Test configuration
```

## Running Tests

### Prerequisites
- .NET 9 SDK
- The main PianoPracticeJournal project built successfully

### Commands

```bash
# Restore packages
dotnet restore

# Build the test project
dotnet build PianoPracticeJournal.Tests

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with coverage (requires coverlet.collector)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=PracticeSessionTests"

# Run tests by category
dotnet test --filter "Category=Unit"
```

## Test Categories

### Unit Tests
- **PracticeSessionTests**: Tests for the PracticeSession model
- **SessionManagerTests**: Tests for session management logic
- **ApiClientTests**: Tests for API communication
- **SyncServiceTests**: Tests for synchronization logic
- **StartupServiceTests**: Tests for Windows startup integration

### Integration Tests
- **SessionManagementIntegrationTests**: End-to-end tests for session workflow

## Test Coverage

The tests cover the following areas:

### Models
- ✅ PracticeSession creation and initialization
- ✅ Duration calculations
- ✅ Completion status
- ✅ Sync status tracking

### Services
- ✅ SessionManager: Start/stop sessions, MIDI signal handling
- ✅ ApiClient: HTTP requests, error handling, connectivity testing
- ✅ SyncService: Session synchronization, retry logic
- ✅ StartupService: Windows registry operations

### Integration
- ✅ Complete session workflow (start → play → end → sync)
- ✅ Multiple session management
- ✅ Error handling and recovery
- ✅ Statistics and reporting

## Test Utilities

### TestDataBuilder
Provides factory methods for creating test objects:
```csharp
// Create a completed session
var session = TestDataBuilder.CreateCompletedSession();

// Create multiple sessions
var sessions = TestDataBuilder.CreateMultipleSessions(5);

// Create test app settings
var settings = TestDataBuilder.CreateTestAppSettings();
```

### MockHelpers
Provides utilities for creating and verifying mocks:
```csharp
// Create a mock logger
var mockLogger = MockHelpers.CreateMockLogger<MyClass>();

// Verify log was called
MockHelpers.VerifyLogCalled(mockLogger, LogLevel.Information, Times.Once);
```

## Test Configuration

The `appsettings.json` file contains test-specific configuration:
- API endpoint for testing
- Reduced timeouts for faster tests
- Disabled auto-start and system tray features

## Mocking Strategy

The tests use **Moq** for creating mocks and **FluentAssertions** for assertions:

```csharp
// Mock external dependencies
var mockApiClient = new Mock<IApiClient>();
mockApiClient.Setup(x => x.SubmitSessionAsync(It.IsAny<PracticeSession>()))
    .ReturnsAsync(new SessionSubmissionResponse { Success = true });

// Verify interactions
mockApiClient.Verify(x => x.SubmitSessionAsync(session), Times.Once);

// Fluent assertions
result.Should().NotBeNull();
result.Success.Should().BeTrue();
```

## Continuous Integration

The tests are designed to run in CI environments:
- No external dependencies (MIDI hardware, network)
- Deterministic test data
- Isolated test execution
- Fast execution time

## Troubleshooting

### Common Issues

1. **Build failures**: Ensure the main project builds successfully
2. **Test timeouts**: Some integration tests may take longer
3. **Registry access**: StartupService tests may fail in restricted environments

### Debugging

```bash
# Run tests with debug output
dotnet test --logger "console;verbosity=detailed"

# Run specific test method
dotnet test --filter "MethodName=TestMethod"

# Run tests without parallel execution
dotnet test --maxcpucount:1
```

## Contributing

When adding new tests:
1. Follow the existing naming conventions
2. Use descriptive test names
3. Include both positive and negative test cases
4. Use the TestDataBuilder for consistent test data
5. Add integration tests for new workflows
6. Update this README if adding new test categories

## Performance

- Unit tests: < 1 second total execution time
- Integration tests: < 5 seconds total execution time
- Memory usage: Minimal, no memory leaks in test execution
