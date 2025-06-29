using Microsoft.Extensions.Logging;

namespace Appointment_System.Services
{
    public class DatabaseLoggerService
    {
        private readonly ILogger<DatabaseLoggerService> _logger;

        public DatabaseLoggerService(ILogger<DatabaseLoggerService> logger)
        {
            _logger = logger;
        }

        public void LogDatabaseOperation(string operation, string entityType, string entityId, string? userId = null, string? details = null)
        {
            _logger.LogInformation(
                "Database Operation: {Operation} | Entity: {EntityType} | ID: {EntityId} | User: {UserId} | Details: {Details}",
                operation,
                entityType,
                entityId,
                userId ?? "system",
                details ?? "none"
            );
        }

        public void LogDatabaseError(string operation, string entityType, Exception exception, string? entityId = null, string? userId = null)
        {
            _logger.LogError(
                exception,
                "Database Error: {Operation} | Entity: {EntityType} | ID: {EntityId} | User: {UserId}",
                operation,
                entityType,
                entityId ?? "unknown",
                userId ?? "system"
            );
        }

        public void LogDatabaseQuery(string queryDescription, long elapsedMilliseconds, int resultCount, string? userId = null)
        {
            _logger.LogDebug(
                "Database Query: {QueryDescription} | Duration: {ElapsedMs}ms | Results: {ResultCount} | User: {UserId}",
                queryDescription,
                elapsedMilliseconds,
                resultCount,
                userId ?? "system"
            );
        }
    }
} 