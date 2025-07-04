using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Appointment_System.GraphQL.Authorization
{
    public class AuthorizationDirectiveHandler : AuthorizationHandler<AuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AuthorizationRequirement requirement)
        {
            // Get the middleware context
            if (context.Resource is IMiddlewareContext middlewareContext)
            {
                // Check if the user is authenticated
                if (context.User.Identity?.IsAuthenticated ?? false)
                {
                    // Check if roles are required
                    if (requirement.Roles.Count > 0)
                    {
                        foreach (var role in requirement.Roles)
                        {
                            if (context.User.IsInRole(role))
                            {
                                context.Succeed(requirement);
                                return Task.CompletedTask;
                            }
                        }
                        
                        // If we get here, the user doesn't have any of the required roles
                        context.Fail();
                    }
                    else
                    {
                        // No specific roles required, just authentication
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    // User is not authenticated
                    context.Fail();
                }
            }
            
            return Task.CompletedTask;
        }
    }
} 