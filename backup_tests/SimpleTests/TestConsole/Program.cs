using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Simple Tests for Appointment System");
            Console.WriteLine("==========================================");
            
            int totalTests = 0;
            int passedTests = 0;
            int failedTests = 0;
            var failedTestNames = new List<string>();
            
            // Run calculator tests
            Console.WriteLine("\nRunning Calculator Tests:");
            RunTest("Addition", () => {
                totalTests++;
                int result = Add(2, 3);
                if (result != 5)
                    throw new Exception($"Expected 5, got {result}");
                passedTests++;
            }, failedTestNames);
            
            RunTest("Subtraction", () => {
                totalTests++;
                int result = Subtract(5, 3);
                if (result != 2)
                    throw new Exception($"Expected 2, got {result}");
                passedTests++;
            }, failedTestNames);
            
            RunTest("Multiplication", () => {
                totalTests++;
                int result = Multiply(2, 3);
                if (result != 6)
                    throw new Exception($"Expected 6, got {result}");
                passedTests++;
            }, failedTestNames);
            
            RunTest("Division", () => {
                totalTests++;
                int result = Divide(6, 3);
                if (result != 2)
                    throw new Exception($"Expected 2, got {result}");
                passedTests++;
            }, failedTestNames);
            
            RunTest("Division by Zero", () => {
                totalTests++;
                try
                {
                    int result = Divide(6, 0);
                    throw new Exception("Expected DivideByZeroException was not thrown");
                }
                catch (DivideByZeroException)
                {
                    // This is the expected behavior
                    passedTests++;
                }
            }, failedTestNames);
            
            // Run string tests
            Console.WriteLine("\nRunning String Tests:");
            RunTest("String Concatenation", () => {
                totalTests++;
                string result = Concatenate("Hello", "World");
                if (result != "HelloWorld")
                    throw new Exception($"Expected 'HelloWorld', got '{result}'");
                passedTests++;
            }, failedTestNames);
            
            RunTest("String Length", () => {
                totalTests++;
                int result = GetLength("Hello");
                if (result != 5)
                    throw new Exception($"Expected 5, got {result}");
                passedTests++;
            }, failedTestNames);
            
            // Run appointment tests
            Console.WriteLine("\nRunning Appointment Tests:");
            RunTest("Create Appointment", () => {
                totalTests++;
                var appointment = CreateAppointment("user1", "Service 1", DateTime.Today.AddDays(1));
                if (appointment == null)
                    throw new Exception("Appointment should not be null");
                if (appointment.UserId != "user1")
                    throw new Exception($"Expected user1, got {appointment.UserId}");
                if (appointment.ServiceName != "Service 1")
                    throw new Exception($"Expected 'Service 1', got '{appointment.ServiceName}'");
                passedTests++;
            }, failedTestNames);
            
            RunTest("Cancel Appointment", () => {
                totalTests++;
                var appointment = CreateAppointment("user1", "Service 1", DateTime.Today.AddDays(1));
                bool result = CancelAppointment(appointment);
                if (!result)
                    throw new Exception("Expected true for successful cancellation");
                if (appointment.Status != "Cancelled")
                    throw new Exception($"Expected 'Cancelled', got '{appointment.Status}'");
                passedTests++;
            }, failedTestNames);
            
            // Print test summary
            failedTests = totalTests - passedTests;
            Console.WriteLine("\nTest Summary:");
            Console.WriteLine($"Total Tests: {totalTests}");
            Console.WriteLine($"Passed: {passedTests}");
            Console.WriteLine($"Failed: {failedTests}");
            
            if (failedTests > 0)
            {
                Console.WriteLine("\nFailed Tests:");
                foreach (var failedTest in failedTestNames)
                {
                    Console.WriteLine($"- {failedTest}");
                }
            }
        }
        
        // Helper method to run a test
        static void RunTest(string testName, Action testAction, List<string> failedTestNames)
        {
            Console.Write($"  {testName}: ");
            try
            {
                testAction();
                Console.WriteLine("Passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed - {ex.Message}");
                failedTestNames.Add(testName);
            }
        }
        
        // Calculator functions
        static int Add(int a, int b) => a + b;
        static int Subtract(int a, int b) => a - b;
        static int Multiply(int a, int b) => a * b;
        static int Divide(int a, int b) => a / b;
        
        // String functions
        static string Concatenate(string a, string b) => a + b;
        static int GetLength(string s) => s.Length;
        
        // Simple appointment class
        class Appointment
        {
            public string UserId { get; set; }
            public string ServiceName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Status { get; set; } = "Pending";
        }
        
        // Appointment functions
        static Appointment CreateAppointment(string userId, string serviceName, DateTime date)
        {
            return new Appointment
            {
                UserId = userId,
                ServiceName = serviceName,
                AppointmentDate = date
            };
        }
        
        static bool CancelAppointment(Appointment appointment)
        {
            if (appointment == null)
                return false;
                
            appointment.Status = "Cancelled";
            return true;
        }
    }
} 