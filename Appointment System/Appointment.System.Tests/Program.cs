using System;

namespace Appointment.System.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Running standalone tests...");
            
            var tests = new StandaloneTest();
            tests.RunAllTests();
            
            Console.WriteLine("Test run complete.");
        }
    }
} 