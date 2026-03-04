using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Persistence;

namespace Tests.Helpers;

public static class SeedData
{
    public static Department CreateDepartment(string name = "Cardiology")
        => new() { Name = name, Location = "Building A" };

    public static Doctor CreateDoctor(
        Guid departmentId,
        string license = "LIC-001",
        string email = "doctor@hospital.com")
        => new()
        {
            FirstName     = "Jean",
            LastName      = "Martin",
            Email         = email,
            Specialty     = "Cardiology",
            LicenseNumber = license,
            HireDate      = new DateOnly(2020, 1, 1),
            Salary        = 5000m,
            Address       = new Address(),
            DepartmentId  = departmentId
        };

    public static Patient CreatePatient(
        string recordNumber = "REC-001",
        string email = "patient@test.com",
        string lastName = "Dupont")
        => new()
        {
            FirstName    = "Marie",
            LastName     = lastName,
            BirthDate    = new DateOnly(1985, 6, 15),
            RecordNumber = recordNumber,
            Email        = email,
            Address      = new Address()
        };

    // Seed complet : département + médecin + patient en une seule ligne
    public static async Task<(Department dept, Doctor doctor, Patient patient)>
        SeedBasicDataAsync(HospitalDbContext context)
    {
        var dept = CreateDepartment();
        context.Departments.Add(dept);
        await context.SaveChangesAsync();

        var doctor = CreateDoctor(dept.Id);
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        var patient = CreatePatient();
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        return (dept, doctor, patient);
    }
}