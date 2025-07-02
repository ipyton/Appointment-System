using System;

namespace SimpleTestsDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Simple Tests for Appointment System");
            Console.WriteLine("==========================================");
            
            var testRunner = new SimpleTestRunner();
            
            // Run tests for appointment functionality
            testRunner.RunTests<AppointmentTests>();
            
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
} 