using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Appointment_System.Middleware;
using Appointment_System.Hubs;
using Microsoft.Extensions.Logging;
using TokenService = Appointment_System.Services.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Appointment_System.GraphQL.Types;
using Appointment_System.GraphQL.Queries;
using Appointment_System.GraphQL.Mutations;
using Appointment_System.GraphQL.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from configuration in development
if (builder.Environment.IsDevelopment())
{
    // Load sensitive configuration into environment variables in development
    var azureSearchSection = builder.Configuration.GetSection("AzureSearch");
    if (!string.IsNullOrEmpty(azureSearchSection["AdminApiKey"]))
    {
        Environment.SetEnvironmentVariable("AZURE_SEARCH_ADMIN_API_KEY", azureSearchSection["AdminApiKey"]);
    }
    
    if (!string.IsNullOrEmpty(azureSearchSection["QueryApiKey"]))
    {
        Environment.SetEnvironmentVariable("AZURE_SEARCH_QUERY_API_KEY", azureSearchSection["QueryApiKey"]);
    }
}

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFile(options =>
{
    builder.Configuration.GetSection("Logging:File").Bind(options);
});

// 配置数据库
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Only use AddDbContext, remove the AddPooledDbContextFactory to avoid conflicts
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register the Services
builder.Services.AddScoped<DatabaseLoggerService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AzureSearchService>();
builder.Services.AddScoped<SearchIndexingEventHandler>();
builder.Services.AddScoped<AppointmentClientService>();
builder.Services.AddScoped<AppointmentProviderService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddHttpContextAccessor();

// Register background services
builder.Services.AddHostedService<SearchIndexingService>();

// 配置Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    // 密码设置
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    // 锁定设置
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    
    // 用户设置
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
    
    // Configure the JWT Bearer authentication to send the token in SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            
            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Add GraphQL services
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddType<AppointmentType>()
    .AddType<ServiceType>();
    // .AddAuthorizationHandler<AuthorizationDirectiveHandler>(); - Commented out due to interface incompatibility

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Add SignalR services
builder.Services.AddSignalR();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Get logger factory
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Appointment_System.Program");

// Log application startup
logger.LogInformation("Application starting up");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Running in Development environment");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    logger.LogInformation("Running in Production environment");
}

// Add global exception handling middleware (should be first in the pipeline)
app.UseGlobalExceptionHandling();

// Add request logging middleware
app.UseRequestLogging();

// Use CORS middleware (before UseHttpsRedirection and other routing middleware)
app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();

// Add static files middleware
app.UseStaticFiles();

// Add token validation middleware to filter all requests
app.UseMiddleware<TokenValidationMiddleware>();

// 添加认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();

// Map the GraphQL endpoint
app.MapGraphQL();

app.MapControllers();
// Map the SignalR hub
app.MapHub<ChatHub>("/chatHub");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


// 确保数据库创建
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        logger.LogInformation("Ensuring database is created");
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        // 可选：创建默认角色和管理员用户
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // 初始化默认角色和用户
        logger.LogInformation("Initializing default roles and users");
        InitializeAsync(userManager, roleManager).Wait();
        logger.LogInformation("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database initialization");
    }
}

logger.LogInformation("Application started successfully");
app.Run();

// 初始化默认角色和管理员用户的方法
async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    var initLogger = loggerFactory.CreateLogger("Appointment_System.Initialization");
    
    // 创建角色（如果不存在）
    string[] roleNames = { "Admin", "User", "ServiceProvider" };
    
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
            initLogger.LogInformation("Role {RoleName} created", roleName);
        }
    }
    
    // 检查管理员用户是否存在，如果不存在则创建
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true,
            Address = "Admin Office",
            PhoneNumber = "555-ADMIN",
            ProfilePictureUrl = "/images/default-profile.png",
            // Since BusinessDescription is now nullable, it can be left as null
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123$");
        
        if (result.Succeeded)
        {
            initLogger.LogInformation("Admin user created");
            await userManager.AddToRoleAsync(adminUser, "Admin");
            initLogger.LogInformation("Admin user added to Admin role");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                initLogger.LogError("Error creating admin user: {ErrorDescription}", error.Description);
            }
        }
    }
    
    // 检查示例服务提供者用户是否存在，如果不存在则创建
    var providerEmail = "provider@example.com";
    var providerUser = await userManager.FindByEmailAsync(providerEmail);
    
    if (providerUser == null)
    {
        providerUser = new ApplicationUser
        {
            UserName = providerEmail,
            Email = providerEmail,
            FullName = "Example Provider",
            EmailConfirmed = true,
            Address = "Provider Office",
            PhoneNumber = "555-PROV",
            ProfilePictureUrl = "/images/default-profile.png",
            BusinessName = "Example Service Provider",
            BusinessDescription = "Providing excellent services since 2023.",
        };
        
        var result = await userManager.CreateAsync(providerUser, "Provider123$");
        
        if (result.Succeeded)
        {
            initLogger.LogInformation("Provider user created");
            await userManager.AddToRoleAsync(providerUser, "ServiceProvider");
            initLogger.LogInformation("Provider user added to ServiceProvider role");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                initLogger.LogError("Error creating provider user: {ErrorDescription}", error.Description);
            }
        }
    }
}

