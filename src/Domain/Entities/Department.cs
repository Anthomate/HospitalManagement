using Domain.Common;

namespace Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    public Guid? MedicalDirectorId { get; set; }
    
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = [];
    
    public Doctor? MedicalDirector { get; set; }
    public ICollection<StaffMember> StaffMembers { get; set; } = new List<StaffMember>();
}