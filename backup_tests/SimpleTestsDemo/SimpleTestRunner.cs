using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleTestsDemo
{
    public class SimpleTestRunner
    {
        public int TotalTests { get; private set; } = 0;
        public int PassedTests { get; private set; } = 0;
        public int FailedTests { get; private set; } = 0;
        public List<string> FailedTestNames { get; } = new List<string>();
        
        public void RunTests<T>() where T : new()
        {
            Console.WriteLine($"\nRunning tests in {typeof(T).Name}");
            
            // Create an instance of the test class
            var testInstance = new T();
            
            // Get setup method if it exists
            var setupMethod = typeof(T).GetMethod("Setup");
            
            // Get teardown method if it exists
            var teardownMethod = typeof(T).GetMethod("Teardown");
            
            // Get all test methods in the class
            var testMethods = typeof(T).GetMethods()
                .Where(m => m.Name.StartsWith("Test"))
                .ToList();
            
            foreach (var testMethod in testMethods)
            {
                TotalTests++;
                string testName = $"{typeof(T).Name}.{testMethod.Name}";
                
                try
                {
                    Console.Write($"  {testMethod.Name}: ");
                    
                    // Run setup method if it exists
                    setupMethod?.Invoke(testInstance, null);
                    
                    // Run the test method
                    testMethod.Invoke(testInstance, null);
                    
                    // Run teardown method if it exists
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