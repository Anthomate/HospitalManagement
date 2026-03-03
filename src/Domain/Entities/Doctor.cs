using Domain.Common;

namespace Domain.Entities;

public class Doctor : MedicalStaff
{
    public string Specialty { get; set; } = string.Empty;
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
}