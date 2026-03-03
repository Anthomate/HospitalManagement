namespace Domain.Entities;

public abstract class MedicalStaff : StaffMember
{
    public string LicenseNumber { get; set; } = string.Empty;
}