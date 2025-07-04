using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Appointment_System.Services
{
    public class FileLoggerConfiguration
    {
        public string Path { get; set; } = "logs/app.log";
        public long FileSizeLimitBytes { get; set; } = 10 * 1024 * 1024; // 10MB
        public int MaxRollingFiles { get; set; } = 10;
    }

    // Logger provider implementation
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly FileLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
        private readonly string _path;
        private readonly long _fileSizeLimit;
        private readonly int _maxRollingFiles;
        private readonly object _lockObj = new();

        public FileLoggerProvider(IOptions<FileLoggerConfiguration> config)
        {
            _config = config.Value;
            _path = _config.Path;
            _fileSizeLimit = _config.FileSizeLimitBytes;
            _maxRollingFiles = _config.MaxRollingFiles;

            // Ensure log directory exists
            var logDirectory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _path, _fileSizeLimit, _maxRollingFiles, _lockObj));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _path;
        private readonly long _fileSizeLimit;
        private readonly int _maxRollingFiles;
        private readonly object _lockObj;

        public FileLogger(string categoryName, string path, long fileSizeLimit, int maxRollingFiles, object lockObj)
        {
            _categoryName = categoryName;
            _path = path;
            _fileSizeLimit = fileSizeLimit;
            _maxRollingFiles = maxRollingFiles;
            _lockObj = lockObj;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            
            if (string.IsNullOrEmpty(message) && exception == null)
                return;

            var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_categoryName}: {message}";
            
            if (exception != null)
                logEntry += $"{Environment.NewLine}{exception}{Environment.NewLine}";

            lock (_lockObj)
            {
                try
                {
                    // Check if file needs to be rolled
                    CheckFileSize();
                    
                    // Write to file
                    File.AppendAllText(_path, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Silently fail if logging fails
                }
            }
        }

        private void CheckFileSize()
        {
            if (!File.Exists(_path))
                return;

            var fileInfo = new FileInfo(_path);
            if (fileInfo.Length < _fileSizeLimit)
                return;

            // Roll files
            for (int i = _maxRollingFiles - 1; i >= 0; i--)
            {
                var sourceFile = i == 0 ? _path : $"{_path}.{i}";
                var destFile = $"{_path}.{i + 1}";

                if (File.Exists(sourceFile))
                {
                    if (File.Exists(destFile))
                        File.Delete(destFile);

                    if (i == 0)
                        File.Move(_path, destFile);
                    else
                        File.Move(sourceFile, destFile);
                }
            }
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerConfiguration> configure = null)
        {
            builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
            
            if (configure != null)
            {
                builder.Services.Configure(configure);
            }
            
            return builder;
        }
    }
} 