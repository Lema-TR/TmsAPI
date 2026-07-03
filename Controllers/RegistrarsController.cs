using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/registrar")]
public class RegistrarsController(TmsDbContext context) : ControllerBase
{
    // TODO1: Pagination (OrderBy before Skip/Take is mandatory for stable sorting)
    [HttpGet("students/paged")]
    public async Task<IActionResult> GetPagedStudents(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var students = await context.Students
            .OrderBy(s => s.Name) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // TODO2: Top 5 courses by enrollment count
    [HttpGet("courses/top-enrolled")]
    public async Task<IActionResult> GetTopCourses(CancellationToken cancellationToken = default)
    {
        var topCourses = await context.Courses
            .Select(c => new 
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return Ok(topCourses);
    }
}