namespace TmsApi.Entities;

public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Session 3: Soft-delete and Concurrency
    public bool IsDeleted { get; set; } = false;
    public uint Version { get; set; } // Mapped to PostgreSQL xmin for concurrency

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}