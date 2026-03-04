using Application.Common;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class DoctorRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly DoctorRepository _repository;

    public DoctorRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new DoctorRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDoctor_WhenExists()
    {
        var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _repository.GetByIdAsync(doctor.Id);

        result.Should().NotBeNull();
        result!.LicenseNumber.Should().Be("LIC-001");
    }

    // [Fact]
    // public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _repository.GetByIdAsync(Guid.NewGuid());
    //     result.Should().BeNull();
    // }

    // [Fact]
    // public async Task ExistsByLicenseNumberAsync_ShouldReturnTrue_WhenExists()
    // {
    //     await SeedData.SeedBasicDataAsync(_context);
    //
    //     var result = await _repository.ExistsByLicenseNumberAsync("LIC-001");
    //     result.Should().BeTrue();
    // }

    // [Fact]
    // public async Task ExistsByLicenseNumberAsync_ShouldReturnFalse_WhenExcludingCurrentDoctor()
    // {
    //     var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);
    //
    //     var result = await _repository.ExistsByLicenseNumberAsync("LIC-001", doctor.Id);
    //     result.Should().BeFalse();
    // }

    [Fact]
    public async Task IsDirectorAsync_ShouldReturnTrue_WhenDoctorIsDirector()
    {
        var (dept, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        dept.MedicalDirectorId = doctor.Id;
        await _context.SaveChangesAsync();

        var result = await _repository.IsDirectorAsync(doctor.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsDirectorAsync_ShouldReturnFalse_WhenDoctorIsNotDirector()
    {
        var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _repository.IsDirectorAsync(doctor.Id);
        result.Should().BeFalse();
    }

    // [Fact]
    // public async Task GetByDepartmentAsync_ShouldReturnOnlyDepartmentDoctors()
    // {
    //     var dept1 = SeedData.CreateDepartment("Cardiology");
    //     var dept2 = SeedData.CreateDepartment("Neurology");
    //     _context.Departments.AddRange(dept1, dept2);
    //     await _context.SaveChangesAsync();
    //
    //     _context.Doctors.AddRange(
    //         SeedData.CreateDoctor(dept1.Id, "LIC-001", "d1@hospital.com"),
    //         SeedData.CreateDoctor(dept1.Id, "LIC-002", "d2@hospital.com"),
    //         SeedData.CreateDoctor(dept2.Id, "LIC-003", "d3@hospital.com")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetByDepartmentAsync(
    //         dept1.Id, new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     result.TotalCount.Should().Be(2);
    //     result.Items.Should().AllSatisfy(d => d.DepartmentId.Should().Be(dept1.Id));
    // }

    public void Dispose() => _context.Dispose();
}