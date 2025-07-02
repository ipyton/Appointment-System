# Testing the Appointment System

This document provides instructions for setting up and running tests for the Appointment System project.

## Summary

Based on our attempts to set up testing for this project, we've encountered some challenges with the project structure. Here's a summary of the key points for testing this application:

1. **Test Project Structure**: Create a separate test project that mirrors the structure of the main application.

2. **Required Packages**: 
   - xUnit for the testing framework
   - Moq for mocking dependencies
   - Microsoft.EntityFrameworkCore.InMemory for database testing

3. **Testing Strategies**:
   - **Unit Tests**: Test individual components in isolation using mocks for dependencies.
   - **Integration Tests**: Test components working together, using in-memory database.
   - **Controller Tests**: Test API endpoints with mocked services and authentication.

4. **Common Testing Patterns**:
   - Use the AAA pattern (Arrange, Act, Assert)
   - Create helper methods for common setup tasks
   - Use a separate in-memory database for each test to ensure isolation

5. **Troubleshooting**:
   - If you encounter namespace conflicts, ensure the test project has a different namespace
   - If you have issues with references, make sure all required packages are installed
   - For build errors, clean the solution and rebuild

## Test Project Structure

The test project is organized to mirror the structure of the main application:

```
Appointment.System.Tests/
├── Controllers/           # Tests for controllers
├── Services/              # Tests for services
├── Models/                # Tests for models
└── Helpers/               # Test helper classes
```

## Setup Instructions

1. **Create the test project**:
   ```bash
   dotnet new xunit -o Appointment.System.Tests
   ```

2. **Add a reference to the main project**:
   ```bash
   cd Appointment.System.Tests
   dotnet add reference "../Appointment System.csproj"
   ```

3. **Add required NuGet packages**:
   ```bash
   dotnet add package Moq
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   ```

4. **Add the test project to the solution**:
   ```bash
   cd ..
   dotnet sln "../Appointment System.sln" add "Appointment.System.Tests/Appointment.System.Tests.csproj"
   ```

## Writing Tests

### Database Helper

Create a `DatabaseHelper.cs` in the Helpers folder to set up an in-memory database for testing:

```csharp
using System;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;

namespace Appointment.System.Tests.Helpers
{
    public static class DatabaseHelper
    {
        public static ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);
            
            // Seed the database with test data
            SeedDatabase(dbContext);
            
            return dbContext;
        }

        private static void SeedDatabase(ApplicationDbContext dbContext)
        {
            // Add test data here
            // Example:
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User"
            };
            dbContext.Users.Add(user);

            // Add other test entities as needed

            dbContext.SaveChanges();
        }
    }
}
```

### Service Tests

Create test classes for services in the Services folder. For example:

```csharp
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Appointment_System.Services;
using Appointment_System.Models;
using Appointment.System.Tests.Helpers;

namespace Appointment.System.Tests.Services
{
    public class AppointmentClientServiceTests
    {
        [Fact]
        public async Task GetAvailableServicesAsync_ReturnsActiveServices()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var loggerMock = new Mock<ILogger<AppointmentClientService>>();
            var service = new AppointmentClientService(dbContext, loggerMock.Object);

            // Act
            var result = await service.GetAvailableServicesAsync();

            // Assert
            Assert.NotNull(result);
            // Additional assertions
        }

        // Additional tests
    }
}
```

### Controller Tests

Create test classes for controllers in the Controllers folder. For example:

```csharp
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Appointment_System.Controllers;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Appointment.System.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Appointment.System.Tests.Controllers
{
    public class AppointmentControllerTests
    {
        private readonly Mock<ILogger<AppointmentController>> _loggerMock;
        private readonly string _userId = "test-user-id";

        public AppointmentControllerTests()
        {
            _loggerMock = new Mock<ILogger<AppointmentController>>();
        }

        private AppointmentController SetupController(ApplicationDbContext dbContext)
        {
            var appointmentService = new AppointmentClientService(dbContext, Mock.Of<ILogger<AppointmentClientService>>());
            var controller = new AppointmentController(appointmentService, dbContext, _loggerMock.Object);

            // Setup ClaimsPrincipal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId),
                new Claim(ClaimTypes.Name, "testuser@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Setup controller context
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task GetAvailableServices_ReturnsOkResult_WithServices()
        {
            // Arrange
            var dbContext = DatabaseHelper.GetDatabaseContext();
            var controller = SetupController(dbContext);

            // Act
            var result = await controller.GetAvailableServices();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Additional assertions
        }

        // Additional tests
    }
}
```

## Running Tests

Run all tests:

```bash
dotnet test
```

Run specific tests:

```bash
dotnet test --filter "FullyQualifiedName~AppointmentClientServiceTests"
```

## Troubleshooting

If you encounter issues with the test project:

1. **Namespace conflicts**: Ensure that the test project namespace doesn't conflict with the main project.

2. **Missing references**: Make sure all required packages are installed.

3. **Build errors**: Clean the solution and rebuild:
   ```bash
   dotnet clean
   dotnet build
   ```

4. **In-memory database issues**: If tests that use the in-memory database fail, ensure that each test uses a unique database name.

5. **Authentication issues**: For controller tests that require authentication, ensure that the `ClaimsPrincipal` is properly set up. 