using System;

namespace SimpleTests
{
    public class BasicTests
    {
        // Simple calculator functions for testing
        private int Add(int a, int b) => a + b;
        private int Subtract(int a, int b) => a - b;
        private int Multiply(int a, int b) => a * b;
        private int Divide(int a, int b) => a / b;
        
        // Test methods
        public void TestAdd()
        {
            int result = Add(2, 3);
            if (result != 5)
                throw new Exception($"Add failed: Expected 5, got {result}");
        }
        
        public void TestSubtract()
        {
            int result = Subtract(5, 3);
            if (result != 2)
                throw new Exception($"Subtract failed: Expected 2, got {result}");
        }
        
        public void TestMultiply()
        {
            int result = Multiply(2, 3);
            if (result != 6)
                throw new Exception($"Multiply failed: Expected 6, got {result}");
        }
        
        public void TestDivide()
        {
            int result = Divide(6, 3);
            if (result != 2)
                throw new Exception($"Divide failed: Expected 2, got {result}");
        }
        
        public void TestDivideByZero()
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
    }
} 