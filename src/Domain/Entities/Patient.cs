using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string RecordNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Address Address { get; set; } = new Address();
    
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}