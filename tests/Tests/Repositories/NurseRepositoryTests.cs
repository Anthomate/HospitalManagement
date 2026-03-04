using Application.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class NurseRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly NurseRepository _repository;

    public NurseRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new NurseRepository(_context);
    }

    private static Nurse CreateNurse(Guid departmentId, string license, string email)
        => new()
        {
            FirstName     = "Sophie",
            LastName      = "Leclerc",
            Email         = email,
            LicenseNumber = license,
            Service       = "Emergency",
            Grade         = "Senior",
            HireDate      = new DateOnly(2021, 3, 1),
            Salary        = 2800m,
            Address       = new Address(),
            DepartmentId  = departmentId
        };

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNurse_WhenExists()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var nurse = CreateNurse(dept.Id, "NUR-001", "nurse@hospital.com");
        _context.Nurses.Add(nurse);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(nurse.Id);

        result.Should().NotBeNull();
        result!.LicenseNumber.Should().Be("NUR-001");
    }

    // [Fact]
    // public async Task ExistsByLicenseNumberAsync_ShouldReturnTrue_WhenExists()
    // {
    //     var dept = SeedData.CreateDepartment();
    //     _context.Departments.Add(dept);
    //     await _context.SaveChangesAsync();
    //
    //     _context.Nurses.Add(CreateNurse(dept.Id, "NUR-001", "nurse@hospital.com"));
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.ExistsByLicenseNumberAsync("NUR-001");
    //     result.Should().BeTrue();
    // }

    // [Fact]
    // public async Task ExistsByLicenseNumberAsync_ShouldReturnFalse_WhenExcludingCurrentNurse()
    // {
    //     var dept = SeedData.CreateDepartment();
    //     _context.Departments.Add(dept);
    //     await _context.SaveChangesAsync();
    //
    //     var nurse = CreateNurse(dept.Id, "NUR-001", "nurse@hospital.com");
    //     _context.Nurses.Add(nurse);
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.ExistsByLicenseNumberAsync("NUR-001", nurse.Id);
    //     result.Should().BeFalse();
    // }

    // [Fact]
    // public async Task GetByDepartmentAsync_ShouldReturnOnlyDepartmentNurses()
    // {
    //     var dept1 = SeedData.CreateDepartment("Cardiology");
    //     var dept2 = SeedData.CreateDepartment("Neurology");
    //     _context.Departments.AddRange(dept1, dept2);
    //     await _context.SaveChangesAsync();
    //
    //     _context.Nurses.AddRange(
    //         CreateNurse(dept1.Id, "NUR-001", "n1@hospital.com"),
    //         CreateNurse(dept1.Id, "NUR-002", "n2@hospital.com"),
    //         CreateNurse(dept2.Id, "NUR-003", "n3@hospital.com")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetByDepartmentAsync(
    //         dept1.Id, new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     result.TotalCount.Should().Be(2);
    //     result.Items.Should().AllSatisfy(n => n.DepartmentId.Should().Be(dept1.Id));
    // }

    public void Dispose() => _context.Dispose();
}