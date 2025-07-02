using System;
using Xunit;

namespace Appointment.System.Tests
{
    public class ExampleTest
    {
        [Fact]
        public void BasicTest_ShouldPass()
        {
            // Arrange
            int a = 2;
            int b = 3;
            
            // Act
            int result = a + b;
            
            // Assert
            Assert.Equal(5, result);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(5, 5, 10)]
        [InlineData(0, 0, 0)]
        public void AdditionTheory_ShouldPass(int a, int b, int expected)
        {
            // Act
            int result = a + b;
            
            // Assert
            Assert.Equal(expected, result);
        }
    }
} 