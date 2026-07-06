using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };
        
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        
        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    // Session 2: Pagination, Filtering, and Sorting implementation
    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        // 1. Start with no-tracking query
        IQueryable<Course> query = context.Courses.AsNoTracking();

        // 2. Apply Search Filter (ILike is case-insensitive in PostgreSQL)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => 
                EF.Functions.ILike(c.Title, $"%{request.Search}%") || 
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        // 3. Count BEFORE paging (Crucial for accurate TotalCount)
        var totalCount = await query.CountAsync(ct);

        // 4. Apply Sorting (Whitelist safe columns only)
        IQueryable<Course> sortedQuery = request.OrderBy.ToLower() switch
        {
            "code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
            "maxcapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
        };

        // 5. Apply Skip/Take and Project to DTO
        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .ToListAsync(ct);

        // 6. Return the paged response
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}