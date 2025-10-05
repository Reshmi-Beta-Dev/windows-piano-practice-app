using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add CORS to allow the WPF app to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors();

// Mock API endpoint to receive session data
app.MapPost("/api/sessions", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var json = await reader.ReadToEndAsync();
        
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Received session data:");
        Console.WriteLine(json);
        Console.WriteLine();
        
        // Parse the session data to validate it
        var sessionData = JsonSerializer.Deserialize<object>(json);
        
        // Simulate some processing time
        await Task.Delay(100);
        
        // Return success response
        return Results.Ok(new
        {
            success = true,
            message = "Session recorded successfully",
            timestamp = DateTime.UtcNow,
            sessionId = Guid.NewGuid().ToString()
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error processing session: {ex.Message}");
        
        return Results.BadRequest(new
        {
            success = false,
            message = "Error processing session data",
            error = ex.Message
        });
    }
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Root endpoint with instructions
app.MapGet("/", () => Results.Ok(new
{
    message = "Mock API Server for Piano Practice Journal",
    endpoints = new
    {
        sessions = "POST /api/sessions - Submit session data",
        health = "GET /health - Health check"
    },
    instructions = "Update your app settings to use: http://localhost:5000/api/sessions"
}));

Console.WriteLine("Mock API Server starting...");
Console.WriteLine("Endpoints:");
Console.WriteLine("  POST /api/sessions - Submit session data");
Console.WriteLine("  GET /health - Health check");
Console.WriteLine("  GET / - API information");
Console.WriteLine();
Console.WriteLine("Update your app settings to use: http://localhost:5000/api/sessions");
Console.WriteLine();

app.Run("http://localhost:5000");
