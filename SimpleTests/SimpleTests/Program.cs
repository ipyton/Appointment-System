using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Simple Tests for Appointment System");
            Console.WriteLine("==========================================");
            
            var testRunner = new SimpleTestRunner();
            
            // Run tests for the basic test class
            testRunner.RunTests<BasicTests>();
            
            // Run tests for more complex test classes
            // Uncomment these when the custom attributes are properly set up
            // testRunner.RunTests<AppointmentTests>();
            // testRunner.RunTests<TemplateTests>();
            
            Console.WriteLine("\nTest Summary:");
            Console.WriteLine($"Total Tests: {testRunner.TotalTests}");
            Console.WriteLine($"Passed: {testRunner.PassedTests}");
            Console.WriteLine($"Failed: {testRunner.FailedTests}");
            
            if (testRunner.FailedTests > 0)
            {
                Console.WriteLine("\nFailed Tests:");
                foreach (var failedTest in testRunner.FailedTestNames)
                {
                    Console.WriteLine($"- {failedTest}");
                }
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
    
    public class TestRunner
    {
        public int TotalTests { get; private set; } = 0;
        public int PassedTests { get; private set; } = 0;
        public int FailedTests { get; private set; } = 0;
        public List<string> FailedTestNames { get; } = new List<string>();
        
        public void DiscoverAndRunTests()
        {
            // Get all test classes in the current assembly
            var testClasses = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttributes<TestClassAttribute>().Any())
                .ToList();
            
            foreach (var testClass in testClasses)
            {
                Console.WriteLine($"\nRunning tests in {testClass.Name}");
                
                // Create an instance of the test class
                var testInstance = Activator.CreateInstance(testClass);
                
                // Get all test methods in the class
                var testMethods = testClass.GetMethods()
                    .Where(m => m.GetCustomAttributes<TestMethodAttribute>().Any())
                    .ToList();
                
                foreach (var testMethod in testMethods)
                {
                    TotalTests++;
                    string testName = $"{testClass.Name}.{testMethod.Name}";
                    
                    try
                    {
                        Console.Write($"  {testMethod.Name}: ");
                        
                        // Run setup method if it exists
                        var setupMethod = testClass.GetMethod("Setup");
                        setupMethod?.Invoke(testInstance, null);
                        
                        // Run the test method
                        testMethod.Invoke(testInstance, null);
                        
                        // Run teardown method if it exists
                        var teardownMethod = testClass.GetMethod("Teardown");
                        teardownMethod?.Invoke(testInstance, null);
                        
                        Console.WriteLine("Passed");
                        PassedTests++;
                    }
                    catch (Exception ex)
                    {
                        // Get the actual exception (not the reflection exception)
                        var actualException = ex.InnerException ?? ex;
                        
                        Console.WriteLine($"Failed - {actualException.Message}");
                        FailedTests++;
                        FailedTestNames.Add(testName);
                    }
                }
            }
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClassAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : Attribute
    {
    }
    
    public static class Assert
    {
        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new AssertFailedException(
                    message ?? $"Expected: {expected}, Actual: {actual}");
            }
        }
        
        public static void IsTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new AssertFailedException(
                    message ?? "Expected condition to be true");
            }
        }
        
        public static void IsFalse(bool condition, string message = null)
        {
            if (condition)
            {
                throw new AssertFailedException(
                    message ?? "Expected condition to be false");
            }
        }
        
        public static void IsNull(object value, string message = null)
        {
            if (value != null)
            {
                throw new AssertFailedException(
                    message ?? $"Expected null, but was: {value}");
            }
        }
        
        public static void IsNotNull(object value, string message = null)
        {
            if (value == null)
            {
                throw new AssertFailedException(
                    message ?? "Expected non-null value");
            }
        }
    }
    
    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message)
        {
        }
    }
} 