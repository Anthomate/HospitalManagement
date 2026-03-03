using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public abstract class StaffMember : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Address Address { get; set; } = new();
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public DateOnly HireDate { get; set; } 
    public decimal Salary { get; set; }
}