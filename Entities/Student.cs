namespace TmsApi.Entities;

public class Student
{
    public int Id { get; set; } // Surrogate primary key
    public required string RegistrationNumber { get; set; } // Natural key
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}