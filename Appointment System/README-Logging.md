# Appointment System - Logging Documentation

This document provides an overview of the logging system implemented in the Appointment System application.

## Logging Configuration

The application uses a comprehensive logging system that includes:

1. **Console Logging**: Outputs logs to the console during development
2. **File Logging**: Writes logs to files with automatic rotation
3. **Request Logging**: Logs HTTP requests and responses
4. **Exception Logging**: Captures and logs all unhandled exceptions
5. **Database Operation Logging**: Tracks database operations

## Log Levels

The following log levels are used:

- **Debug**: Detailed information for debugging purposes
- **Information**: General information about application flow
- **Warning**: Potential issues that don't cause application failure
- **Error**: Errors that prevent a function from working
- **Critical**: Critical errors that cause application failure

## Log File Location

Log files are stored in the `logs` directory with the following naming convention:
- Main log file: `logs/app.log`
- Rotated log files: `logs/app.log.1`, `logs/app.log.2`, etc.

## Log File Rotation

Log files are automatically rotated when they reach 10MB in size. The system keeps up to 10 rotated log files.

## Structured Logging

The logging system uses structured logging with semantic logging format:

```
TIMESTAMP [LEVEL] CATEGORY: MESSAGE
```

For example:
```
2023-07-01 12:34:56.789 [Information] Appointment_System.Program: Application starting up
```

## Middleware Components

1. **RequestLoggingMiddleware**: Logs all HTTP requests and responses with timing information
2. **GlobalExceptionHandlingMiddleware**: Captures unhandled exceptions and logs them

## Database Logging

The `DatabaseLoggerService` provides methods for logging database operations:

- `LogDatabaseOperation`: Logs create, update, delete operations
- `LogDatabaseError`: Logs database errors with exception details
- `LogDatabaseQuery`: Logs database queries with performance metrics

## How to Use Logging in New Code

### Dependency Injection

```csharp
private readonly ILogger<YourClass> _logger;

public YourClass(ILogger<YourClass> logger)
{
    _logger = logger;
}
```

### Logging Examples

```csharp
// Information logging
_logger.LogInformation("Operation completed successfully");

// With structured data
_logger.LogInformation("User {UserId} performed {Action}", userId, action);

// Warning logging
_logger.LogWarning("Resource {ResourceId} is running low", resourceId);

// Error logging
try
{
    // Some operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred while processing {Operation}", operationName);
}
```

### Using DatabaseLoggerService

```csharp
private readonly DatabaseLoggerService _dbLogger;

public YourClass(DatabaseLoggerService dbLogger)
{
    _dbLogger = dbLogger;
}

public void SomeMethod()
{
    try
    {
        // Database operation
        _dbLogger.LogDatabaseOperation("Create", "Appointment", appointmentId, userId);
    }
    catch (Exception ex)
    {
        _dbLogger.LogDatabaseError("Create", "Appointment", ex, appointmentId, userId);
    }
}
```

## Configuration

Logging configuration is stored in `appsettings.json` and can be modified to adjust log levels for different components. 