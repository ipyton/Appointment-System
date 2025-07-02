using System;

namespace Appointment.System.Tests
{
    /// <summary>
    /// This is a standalone test class that doesn't depend on any testing framework.
    /// It can be used as a reference for writing tests once the testing framework is properly set up.
    /// </summary>
    public class StandaloneTest
    {
        public void TestAddition()
        {
            // Arrange
            int a = 2;
            int b = 3;
            
            // Act
            int result = a + b;
            
            // Assert
            if (result != 5)
            {
                throw new Exception($"Addition test failed: Expected 5, got {result}");
            }
            
            Console.WriteLine("Addition test passed!");
        }

        public void TestStringConcatenation()
        {
            // Arrange
            string str1 = "Hello";
            string str2 = "World";
            
            // Act
            string result = $"{str1} {str2}";
            
            // Assert
            if (result != "Hello World")
            {
                throw new Exception($"String concatenation test failed: Expected 'Hello World', got '{result}'");
            }
            
            Console.WriteLine("String concatenation test passed!");
        }

        public void TestBooleanLogic()
        {
            // Arrange
            bool condition1 = true;
            bool condition2 = false;
            
            // Act
            bool andResult = condition1 && condition2;
            bool orResult = condition1 || condition2;
            
            // Assert
            if (andResult != false)
            {
                throw new Exception($"Boolean AND test failed: Expected false, got {andResult}");
            }
            
            if (orResult != true)
            {
                throw new Exception($"Boolean OR test failed: Expected true, got {orResult}");
            }
            
            Console.WriteLine("Boolean logic tests passed!");
        }

        public void RunAllTests()
        {
            try
            {
                TestAddition();
                TestStringConcatenation();
                TestBooleanLogic();
                
                Console.WriteLine("All tests passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex.Message}");
            }
        }
    }
} 