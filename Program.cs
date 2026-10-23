using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "User Management API",
        Version = "v1",
        Description = "ASP.NET Core Web API for User Management with CRUD operations, validation, and custom logging middleware."
    });
});

var app = builder.Build();

// Custom Request Logging Middleware - positioned early to measure total processing time
app.UseRequestLogging();

// Enable Swagger UI for interactive API exploration and peer review
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
    // Serve Swagger UI at root (http://localhost:5000/) for easy testing
    options.RoutePrefix = string.Empty;
});

// Map controller routes
app.MapControllers();

app.Run();
