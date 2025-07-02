# Testing the Appointment System

This document provides instructions for setting up and running tests for the Appointment System project.

## Test Project Structure

The test project is organized to mirror the structure of the main application:

```
AppointmentSystem.Tests/
├── Controllers/           # Tests for controllers
├── Services/              # Tests for services
├── Models/                # Tests for models
├── Integration/           # Integration tests
└── Helpers/               # Test helper classes
```

## Running Tests

To run all tests, use the following command from the solution root:

```bash
dotnet test AppointmentSystem.Tests
```

To run specific tests:

```bash
dotnet test AppointmentSystem.Tests --filter "FullyQualifiedName~AppointmentControllerTests"
```

## Testing Approaches

### Unit Tests

Unit tests focus on testing individual components in isolation. For the Appointment System, this includes:

1. **Service Tests**: Test the business logic in service classes.
2. **Controller Tests**: Test the API endpoints and their responses.
3. **Model Tests**: Test model validations and behavior.

### Integration Tests

Integration tests verify that different components work together correctly. For the Appointment System, this includes:

1. **Database Integration**: Test interactions with the database using an in-memory provider.
2. **Service-Controller Integration**: Test the interaction between controllers and services.

## Testing with In-Memory Database

The tests use Entity Framework Core's in-memory database provider to simulate database operations without requiring a real database. This approach allows for fast, isolated tests that don't depend on external resources.

Example:

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

## Testing Controllers with Authentication

For endpoints that require authentication, the tests mock the user identity:

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
6. **Avoid Test Duplication**: Don't create multiple tests that verify the same functionality.
7. **Test One Thing at a Time**: Each test should verify a single aspect of behavior.

## Troubleshooting

### Common Issues

1. **Entity Framework Errors**: If you encounter EF Core errors, make sure you're using the in-memory provider correctly and that your models are properly configured.

2. **Authentication Issues**: For authentication-related tests, ensure you're properly mocking the user identity and claims.

3. **Dependency Injection**: If your tests fail due to missing dependencies, make sure you're providing all required services to your controllers or services.

4. **Namespace Conflicts**: If you have namespace conflicts between test and application code, consider using different namespaces for your test classes.

## Adding New Tests

When adding new tests:

1. Follow the existing structure and naming conventions.
2. Create a new test class for each controller or service.
3. Use the `DatabaseHelper` class to get an in-memory database context.
4. Follow the Arrange-Act-Assert pattern for test structure.

## Consolidated Testing Approach

This test project consolidates all testing approaches for the Appointment System:

1. **Standard xUnit Tests**: Most tests use the standard xUnit testing framework.
2. **In-Memory Database Tests**: Database tests use EF Core's in-memory provider.
3. **Mocked Dependencies**: Services and external dependencies are mocked when appropriate.

By keeping all tests in a single project with a consistent structure, we make it easier to:
- Maintain the test suite
- Find and run specific tests
- Ensure consistent test coverage
- Avoid duplication of test code and setup logic 