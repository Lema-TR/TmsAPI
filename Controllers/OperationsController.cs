using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/operations")]
public class OperationsController(TmsDbContext context) : ControllerBase
{
    // EXERCISE 7: The N+1 Fix
    [HttpGet("n-plus-one-fix")]
    public async Task<IActionResult> FixNPlusOne(CancellationToken cancellationToken = default)
    {
        // BAD: This would cause 1 + N queries if we looped and queried inside.
        // GOOD: Single query with a SQL subquery for the count.
        var report = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(report);
    }

    // EXERCISE 9: Bulk Archive using ExecuteUpdateAsync
    [HttpPost("archive-old-enrollments")]
    public async Task<IActionResult> ArchiveOldEnrollments(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddYears(-1);
        
        // Updates the database directly without loading rows into C# memory!
        var rowsAffected = await context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsArchived, true), cancellationToken);

        return Ok(new { Message = "Bulk update complete", RowsAffected = rowsAffected });
    }

    // EXERCISE 9: Soft-Delete Admin Restore (Bypasses the Query Filter)
    [HttpGet("all-students-including-deleted")]
    public async Task<IActionResult> GetAllStudentsIncludingDeleted(CancellationToken cancellationToken = default)
    {
        // IgnoreQueryFilters() removes the HasQueryFilter(s => !s.IsDeleted) rule
        var students = await context.Students
            .IgnoreQueryFilters()
            .Select(s => new { s.Name, s.IsDeleted })
            .ToListAsync(cancellationToken);

        return Ok(students);
    }
}