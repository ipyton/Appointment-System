using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Appointment.System.Tests
{
    // Example interface to mock
    public interface IExampleService
    {
        Task<List<string>> GetItemsAsync();
        bool IsValidItem(string item);
    }

    // Example class that uses the service
    public class ExampleProcessor
    {
        private readonly IExampleService _service;
        private readonly ILogger<ExampleProcessor> _logger;

        public ExampleProcessor(IExampleService service, ILogger<ExampleProcessor> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<List<string>> GetValidItemsAsync()
        {
            try
            {
                var allItems = await _service.GetItemsAsync();
                var validItems = new List<string>();

                foreach (var item in allItems)
                {
                    if (_service.IsValidItem(item))
                    {
                        validItems.Add(item);
                    }
                }

                return validItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting valid items");
                throw;
            }
        }
    }

    // Test class
    public class MockingExampleTest
    {
        [Fact]
        public async Task GetValidItemsAsync_ReturnsOnlyValidItems()
        {
            // Arrange
            var mockService = new Mock<IExampleService>();
            var mockLogger = new Mock<ILogger<ExampleProcessor>>();
            
            // Setup mock behavior
            mockService.Setup(s => s.GetItemsAsync())
                .ReturnsAsync(new List<string> { "valid1", "invalid1", "valid2", "invalid2" });
            
            mockService.Setup(s => s.IsValidItem(It.Is<string>(s => s.StartsWith("valid"))))
                .Returns(true);
            
            mockService.Setup(s => s.IsValidItem(It.Is<string>(s => s.StartsWith("invalid"))))
                .Returns(false);
            
            var processor = new ExampleProcessor(mockService.Object, mockLogger.Object);

            // Act
            var result = await processor.GetValidItemsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("valid1", result);
            Assert.Contains("valid2", result);
            Assert.DoesNotContain("invalid1", result);
            Assert.DoesNotContain("invalid2", result);
            
            // Verify the service methods were called
            mockService.Verify(s => s.GetItemsAsync(), Times.Once);
            mockService.Verify(s => s.IsValidItem(It.IsAny<string>()), Times.Exactly(4));
        }

        [Fact]
        public async Task GetValidItemsAsync_LogsAndRethrowsException()
        {
            // Arrange
            var mockService = new Mock<IExampleService>();
            var mockLogger = new Mock<ILogger<ExampleProcessor>>();
            
            // Setup mock to throw an exception
            mockService.Setup(s => s.GetItemsAsync())
                .ThrowsAsync(new Exception("Test exception"));
            
            var processor = new ExampleProcessor(mockService.Object, mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => processor.GetValidItemsAsync());
            
            // Verify logger was called with error level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
} 