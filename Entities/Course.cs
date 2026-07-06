namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; } // Surrogate primary key
    public required string Code { get; set; } // Natural key
    public required string Title { get; set; }
    public int MaxCapacity { get; set; }
    
    // Navigation property
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}