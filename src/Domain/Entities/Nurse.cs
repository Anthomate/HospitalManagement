namespace Domain.Entities;

public class Nurse : MedicalStaff
{
    public string Service { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
}