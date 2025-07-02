using System;
using System.Collections.Generic;

namespace TestConsole
{
    public class SimpleTests
    {
        private int totalTests = 0;
        private int passedTests = 0;
        private List<string> failedTestNames = new List<string>();
        
        public void RunAllTests()
        {
            Console.WriteLine("Running Simple Tests for Appointment System");
            Console.WriteLine("==========================================");
            
            // Run calculator tests
            Console.WriteLine("\nRunning Calculator Tests:");
            RunTest("Addition", TestAddition);
            RunTest("Subtraction", TestSubtraction);
            RunTest("Multiplication", TestMultiplication);
            RunTest("Division", TestDivision);
            RunTest("Division by Zero", TestDivisionByZero);
            
            // Run string tests
            Console.WriteLine("\nRunning String Tests:");
            RunTest("String Concatenation", TestStringConcatenation);
            RunTest("String Length", TestStringLength);
            
            // Run appointment tests
            Console.WriteLine("\nRunning Appointment Tests:");
            RunTest("Create Appointment", TestCreateAppointment);
            RunTest("Cancel Appointment", TestCancelAppointment);
            
            // Print test summary
            int failedTests = totalTests - passedTests;
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
        private void RunTest(string testName, Action testAction)
        {
            totalTests++;
            Console.Write($"  {testName}: ");
            try
            {
                testAction();
                Console.WriteLine("Passed");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed - {ex.Message}");
                failedTestNames.Add(testName);
            }
        }
        
        // Calculator tests
        private void TestAddition()
        {
            int result = Add(2, 3);
            if (result != 5)
                throw new Exception($"Expected 5, got {result}");
        }
        
        private void TestSubtraction()
        {
            int result = Subtract(5, 3);
            if (result != 2)
                throw new Exception($"Expected 2, got {result}");
        }
        
        private void TestMultiplication()
        {
            int result = Multiply(2, 3);
            if (result != 6)
                throw new Exception($"Expected 6, got {result}");
        }
        
        private void TestDivision()
        {
            int result = Divide(6, 3);
            if (result != 2)
                throw new Exception($"Expected 2, got {result}");
        }
        
        private void TestDivisionByZero()
        {
            try
            {
                int result = Divide(6, 0);
                throw new Exception("Expected DivideByZeroException was not thrown");
            }
            catch (DivideByZeroException)
            {
                // This is the expected behavior
            }
        }
        
        // String tests
        private void TestStringConcatenation()
        {
            string result = Concatenate("Hello", "World");
            if (result != "HelloWorld")
                throw new Exception($"Expected 'HelloWorld', got '{result}'");
        }
        
        private void TestStringLength()
        {
            int result = GetLength("Hello");
            if (result != 5)
                throw new Exception($"Expected 5, got {result}");
        }
        
        // Appointment tests
        private void TestCreateAppointment()
        {
            var appointment = CreateAppointment("user1", "Service 1", DateTime.Today.AddDays(1));
            if (appointment == null)
                throw new Exception("Appointment should not be null");
            if (appointment.UserId != "user1")
                throw new Exception($"Expected user1, got {appointment.UserId}");
            if (appointment.ServiceName != "Service 1")
                throw new Exception($"Expected 'Service 1', got '{appointment.ServiceName}'");
        }
        
        private void TestCancelAppointment()
        {
            var appointment = CreateAppointment("user1", "Service 1", DateTime.Today.AddDays(1));
            bool result = CancelAppointment(appointment);
            if (!result)
                throw new Exception("Expected true for successful cancellation");
            if (appointment.Status != "Cancelled")
                throw new Exception($"Expected 'Cancelled', got '{appointment.Status}'");
        }
        
        // Calculator functions
        private int Add(int a, int b) => a + b;
        private int Subtract(int a, int b) => a - b;
        private int Multiply(int a, int b) => a * b;
        private int Divide(int a, int b) => a / b;
        
        // String functions
        private string Concatenate(string a, string b) => a + b;
        private int GetLength(string s) => s.Length;
        
        // Simple appointment class
        class Appointment
        {
            public string UserId { get; set; }
            public string ServiceName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string Status { get; set; } = "Pending";
        }
        
        // Appointment functions
        private Appointment CreateAppointment(string userId, string serviceName, DateTime date)
        {
            return new Appointment
            {
                UserId = userId,
                ServiceName = serviceName,
                AppointmentDate = date
            };
        }
        
        private bool CancelAppointment(Appointment appointment)
        {
            if (appointment == null)
                return false;
                
            appointment.Status = "Cancelled";
            return true;
        }
    }
} 