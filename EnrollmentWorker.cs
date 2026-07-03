public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnrollmentWorker> _logger;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        
        _logger.LogInformation("Processing batch in worker...");
    }
}