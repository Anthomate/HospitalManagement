using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class DepartmentRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly DepartmentRepository _repository;

    public DepartmentRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new DepartmentRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDepartment_WhenExists()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(dept.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenNameExists()
    {
        _context.Departments.Add(SeedData.CreateDepartment("Cardiology"));
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByNameAsync("Cardiology");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse_WhenExcludingCurrentDepartment()
    {
        var dept = SeedData.CreateDepartment("Cardiology");
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByNameAsync("Cardiology", dept.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue_WhenNameBelongsToAnotherDepartment()
    {
        var d1 = SeedData.CreateDepartment("Cardiology");
        var d2 = SeedData.CreateDepartment("Neurology");
        _context.Departments.AddRange(d1, d2);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByNameAsync("Cardiology", d2.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasStaffMembersAsync_ShouldReturnTrue_WhenDoctorAttached()
    {
        var (dept, _, _) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _repository.HasStaffMembersAsync(dept.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasStaffMembersAsync_ShouldReturnFalse_WhenEmpty()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var result = await _repository.HasStaffMembersAsync(dept.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasSubDepartmentsAsync_ShouldReturnTrue_WhenChildExists()
    {
        var parent = SeedData.CreateDepartment("Medecine");
        _context.Departments.Add(parent);
        await _context.SaveChangesAsync();

        var child = SeedData.CreateDepartment("Cardiology");
        child.ParentDepartmentId = parent.Id;
        _context.Departments.Add(child);
        await _context.SaveChangesAsync();

        var result = await _repository.HasSubDepartmentsAsync(parent.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasSubDepartmentsAsync_ShouldReturnFalse_WhenNoChildren()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var result = await _repository.HasSubDepartmentsAsync(dept.Id);
        result.Should().BeFalse();
    }

    // [Fact]
    // public async Task GetAllWithDetailsAsync_ShouldReturnAllDepartments()
    // {
    //     _context.Departments.AddRange(
    //         SeedData.CreateDepartment("Cardiology"),
    //         SeedData.CreateDepartment("Neurology"),
    //         SeedData.CreateDepartment("Pediatrics")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetAllWithDetailsAsync();
    //
    //     result.Should().HaveCount(3);
    //     result.Select(d => d.Name).Should().BeEquivalentTo(
    //         ["Cardiology", "Neurology", "Pediatrics"]);
    // }

    public void Dispose() => _context.Dispose();
}