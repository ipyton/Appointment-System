using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Appointment_System.Data;
using Appointment_System.Models;
using Appointment_System.Services;
using Appointment_System.Middleware;
using Microsoft.Extensions.Logging;
using TokenService = Appointment_System.Services.TokenService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register the Services
builder.Services.AddScoped<DatabaseLoggerService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AzureSearchService>();
builder.Services.AddScoped<SearchIndexingEventHandler>();

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
});

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

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

// Add token validation middleware to filter all requests
app.UseMiddleware<TokenValidationMiddleware>();

// 添加认证和授权中间件
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
            initLogger.LogInformation("Creating role: {RoleName}", roleName);
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // 创建管理员用户（如果不存在）
    var adminEmail = "czh1278341834@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        initLogger.LogInformation("Creating admin user: {AdminEmail}", adminEmail);
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrator",
            EmailConfirmed = true,
            Address = "unknown",
            BusinessName = "Admin Business",
            BusinessDescription="This is an Admin Account",
            ProfilePictureUrl = "default.png",
            IsServiceProvider = false,
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
        
        if (result.Succeeded)
        {
            initLogger.LogInformation("Admin user created successfully");
            await userManager.AddToRoleAsync(adminUser, "Admin");
            initLogger.LogInformation("Admin role assigned to user");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                initLogger.LogError("Error creating admin user: {ErrorCode} - {ErrorDescription}", 
                    error.Code, error.Description);
            }
        }
    }
}

