var builder = WebApplication.CreateBuilder(args);

// Session 2: Validate DI scopes to catch captive dependencies at startup
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Services
builder.Services.AddAuthorization();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Session 1 & 2: Middleware Pipeline (Order matters!)
app.UseMiddleware<RequestLoggingMiddleware>(); // Outer wrapper
app.UseExceptionHandler();                     // Early error handling
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapGet("/api/assessments/results", () => Results.Ok(new 
{ 
    courseCode = "CS-101", 
    studentId = "S-001", 
    letterGrade = "A" 
})).RequireAuthorization(); // Returns 401 for anonymous users

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.Run();