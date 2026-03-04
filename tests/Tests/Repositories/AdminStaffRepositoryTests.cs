using Application.Common;
using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class AdminStaffRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly AdminStaffRepository _repository;

    public AdminStaffRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new AdminStaffRepository(_context);
    }

    private static AdminStaff CreateAdminStaff(Guid departmentId, string email)
        => new()
        {
            FirstName    = "Lucas",
            LastName     = "Bernard",
            Email        = email,
            Function     = "Receptionist",
            HireDate     = new DateOnly(2022, 6, 1),
            Salary       = 2200m,
            Address      = new Address(),
            DepartmentId = departmentId
        };

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAdminStaff_WhenExists()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var admin = CreateAdminStaff(dept.Id, "admin@hospital.com");
        _context.AdminStaffs.Add(admin);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(admin.Id);

        result.Should().NotBeNull();
        result!.Function.Should().Be("Receptionist");
    }

    // [Fact]
    // public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _repository.GetByIdAsync(Guid.NewGuid());
    //     result.Should().BeNull();
    // }

    // [Fact]
    // public async Task GetByDepartmentAsync_ShouldReturnOnlyDepartmentAdminStaff()
    // {
    //     var dept1 = SeedData.CreateDepartment("Cardiology");
    //     var dept2 = SeedData.CreateDepartment("Neurology");
    //     _context.Departments.AddRange(dept1, dept2);
    //     await _context.SaveChangesAsync();
    //
    //     _context.AdminStaffs.AddRange(
    //         CreateAdminStaff(dept1.Id, "a1@hospital.com"),
    //         CreateAdminStaff(dept2.Id, "a2@hospital.com")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetByDepartmentAsync(
    //         dept1.Id, new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     result.TotalCount.Should().Be(1);
    //     result.Items[0].DepartmentId.Should().Be(dept1.Id);
    // }

    // [Fact]
    // public async Task GetAllPagedAsync_ShouldReturnPagedResult()
    // {
    //     var dept = SeedData.CreateDepartment();
    //     _context.Departments.Add(dept);
    //     await _context.SaveChangesAsync();
    //
    //     _context.AdminStaffs.AddRange(
    //         CreateAdminStaff(dept.Id, "a1@hospital.com"),
    //         CreateAdminStaff(dept.Id, "a2@hospital.com"),
    //         CreateAdminStaff(dept.Id, "a3@hospital.com")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetAllPagedAsync(
    //         new PaginationParams { Page = 1, PageSize = 2 });
    //
    //     result.TotalCount.Should().Be(3);
    //     result.Items.Should().HaveCount(2);
    //     result.HasNextPage.Should().BeTrue();
    // }

    public void Dispose() => _context.Dispose();
}