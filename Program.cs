using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Session 2: Validate DI scopes to catch captive dependencies at startup
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Services
builder.Services.AddAuthorization();
builder.Services.AddControllers();       // Session 3: Enable MVC Controllers
builder.Services.AddProblemDetails();    // Session 3: Enable RFC 9457 ProblemDetails
builder.Services.AddOpenApi();           // Session 3: Required for Scalar

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Middleware Pipeline (Order matters!)
app.UseMiddleware<RequestLoggingMiddleware>(); // Outer wrapper
app.UseExceptionHandler();                     // Session 3: Early error handling for ProblemDetails
app.UseStatusCodePages();                      // Session 3: Consistent JSON for empty status codes
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Session 3: Map Controllers
app.MapControllers();

// Session 1: Protected Route
app.MapGet("/api/assessments/results", () => Results.Ok(new 
{ 
    courseCode = "CS-101", 
    studentId = "S-001", 
    letterGrade = "A" 
})).RequireAuthorization();

// Session 2: Worker Smoke Test
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// Session 3: Error Test Route
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

// Session 3: Scalar API Explorer (Only in Development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Run();