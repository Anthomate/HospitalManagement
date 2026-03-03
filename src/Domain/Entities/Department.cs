using Domain.Common;

namespace Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    public Guid? MedicalDirectorId { get; set; }
    public Doctor? MedicalDirector { get; set; }
    
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}