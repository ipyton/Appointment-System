# Testing Guide for Appointment System

This document provides guidelines and instructions for testing the Appointment System application.

## Test Project Structure

The recommended structure for testing the Appointment System is as follows:

```
Appointment.System.Tests/
├── Controllers/              # Tests for controllers
├── Services/                 # Tests for services
├── Models/                   # Tests for models
├── Integration/              # Integration tests
└── Helpers/                  # Test helpers and utilities
```

## Required Packages

To set up testing for the Appointment System, you need to install the following NuGet packages:

```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

## Types of Tests

### Unit Tests

Unit tests focus on testing individual components in isolation. For the Appointment System, this includes:

1. **Service Tests**: Test the business logic in service classes.
2. **Controller Tests**: Test the API endpoints and their responses.
3. **Model Tests**: Test model validations and behavior.

### Integration Tests

Integration tests verify that different components work together correctly. For the Appointment System, this includes:

1. **Database Integration**: Test interactions with the database using an in-memory provider.
2. **Service-Controller Integration**: Test the interaction between controllers and services.

## Testing Services

When testing services, use the in-memory database provider to simulate database operations:

```csharp
// Set up in-memory database
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;

// Create context and service
using var context = new ApplicationDbContext(options);
var service = new YourService(context);

// Test the service
var result = await service.YourMethod();
```

## Testing Controllers

When testing controllers, mock the required services and dependencies:

```csharp
// Set up mocks
var serviceMock = new Mock<IYourService>();
serviceMock.Setup(s => s.YourMethod()).ReturnsAsync(expectedResult);

// Create controller with mocked service
var controller = new YourController(serviceMock.Object);

// Test the controller
var result = await controller.YourAction();
```

## Testing with Authentication

For endpoints that require authentication, you need to mock the user identity:

```csharp
// Create claims for a test user
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
    new Claim(ClaimTypes.Name, "test@example.com"),
    new Claim(ClaimTypes.Role, "User")
};

var identity = new ClaimsIdentity(claims, "TestAuth");
var user = new ClaimsPrincipal(identity);

// Set the user on the controller
controller.ControllerContext = new ControllerContext
{
    HttpContext = new DefaultHttpContext { User = user }
};
```

## Best Practices

1. **Use Descriptive Test Names**: Follow the pattern `MethodName_Scenario_ExpectedBehavior`.
2. **Arrange-Act-Assert**: Structure tests with clear arrangement, action, and assertion phases.
3. **Test Edge Cases**: Include tests for error conditions and edge cases.
4. **Keep Tests Independent**: Each test should be able to run independently of others.
5. **Use Fresh Data**: Use a new database instance for each test to avoid interference.

## Troubleshooting

### Common Issues

1. **Entity Framework Errors**: If you encounter EF Core errors, make sure you're using the in-memory provider correctly and that your models are properly configured.

2. **Authentication Issues**: For authentication-related tests, ensure you're properly mocking the user identity and claims.

3. **Dependency Injection**: If your tests fail due to missing dependencies, make sure you're providing all required services to your controllers or services.

4. **Namespace Conflicts**: If you have namespace conflicts between test and application code, consider using different namespaces for your test classes.

### Running Tests

To run tests, use the following command:

```bash
dotnet test
```

To run specific tests:

```bash
dotnet test --filter "FullyQualifiedName~YourTestClassName"
```

## Simple Test Example

If you're having trouble with the full test setup, you can use a simpler approach with a console application that implements basic testing functionality:

```csharp
public class SimpleTestRunner
{
    public void RunTests()
    {
        // Find and run test methods
        var testMethods = GetType().GetMethods()
            .Where(m => m.Name.StartsWith("Test"))
            .ToList();
            
        foreach (var method in testMethods)
        {
            try
            {
                Console.Write($"Running {method.Name}... ");
                method.Invoke(this, null);
                Console.WriteLine("Passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
    
    // Test methods go here
    public void TestExample()
    {
        // Your test code
        if (1 + 1 != 2)
            throw new Exception("Math is broken!");
    }
}
```

This approach allows you to write and run tests without the complexity of a full test framework.

## Conclusion

Testing is an essential part of maintaining the quality and reliability of the Appointment System. By following these guidelines, you can create effective tests that help ensure the application works correctly and continues to do so as it evolves. 