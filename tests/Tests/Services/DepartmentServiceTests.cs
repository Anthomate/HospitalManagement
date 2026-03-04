using Application.Common;
using Application.Common.Exceptions;
using Application.Departments.DTOs;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class DepartmentServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var uow  = new UnitOfWork(_context);
        _service = new DepartmentService(uow, NullLogger<DepartmentService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDepartment_WhenDataIsValid()
    {
        var dto = new CreateDepartmentDto("Cardiology", "Building A");

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("Cardiology");
        _context.Departments.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenNameAlreadyExists()
    {
        _context.Departments.Add(SeedData.CreateDepartment("Cardiology"));
        await _context.SaveChangesAsync();

        var dto = new CreateDepartmentDto("Cardiology", "Building B");

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Name*Cardiology*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDepartmentHasStaff()
    {
        var (dept, _, _) = await SeedData.SeedBasicDataAsync(_context);

        await _service.Invoking(s => s.DeleteAsync(dept.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*staff members*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDepartmentHasSubDepartments()
    {
        var parent = SeedData.CreateDepartment("Medecine");
        _context.Departments.Add(parent);
        await _context.SaveChangesAsync();

        var child = SeedData.CreateDepartment("Cardiology");
        child.ParentDepartmentId = parent.Id;
        _context.Departments.Add(child);
        await _context.SaveChangesAsync();

        await _service.Invoking(s => s.DeleteAsync(parent.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*sub-departments*");
    }

    [Fact]
    public async Task AssignDirectorAsync_ShouldThrow_WhenDoctorNotInDepartment()
    {
        var dept1 = SeedData.CreateDepartment("Cardiology");
        var dept2 = SeedData.CreateDepartment("Neurology");
        _context.Departments.AddRange(dept1, dept2);
        await _context.SaveChangesAsync();

        var doctor = SeedData.CreateDoctor(dept2.Id);
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        await _service.Invoking(s => s.AssignDirectorAsync(dept1.Id, doctor.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*belonging to this department*");
    }

    [Fact]
    public async Task AssignDirectorAsync_ShouldAssign_WhenDoctorBelongsToDepartment()
    {
        var (dept, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _service.AssignDirectorAsync(dept.Id, doctor.Id);

        result.Should().NotBeNull();
        _context.Departments.Find(dept.Id)!.MedicalDirectorId.Should().Be(doctor.Id);
    }

    [Fact]
    public async Task SetParentAsync_ShouldThrow_WhenCycleDetected()
    {
        var parent = SeedData.CreateDepartment("Medecine");
        var child  = SeedData.CreateDepartment("Cardiology");
        _context.Departments.AddRange(parent, child);
        await _context.SaveChangesAsync();

        child.ParentDepartmentId = parent.Id;
        await _context.SaveChangesAsync();

        await _service.Invoking(s => s.SetParentAsync(parent.Id, child.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*circular*");
    }

    // [Fact]
    // public async Task GetDepartmentTreeAsync_ShouldReturnHierarchy()
    // {
    //     var parent = SeedData.CreateDepartment("Medecine");
    //     _context.Departments.Add(parent);
    //     await _context.SaveChangesAsync();
    //
    //     var child = SeedData.CreateDepartment("Cardiology");
    //     child.ParentDepartmentId = parent.Id;
    //     _context.Departments.Add(child);
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _service.GetDepartmentTreeAsync();
    //
    //     result.Should().HaveCount(1);
    //     result[0].Name.Should().Be("Medecine");
    //     result[0].SubDepartments.Should().HaveCount(1);
    //     result[0].SubDepartments[0].Name.Should().Be("Cardiology");
    // }

    public void Dispose() => _context.Dispose();
}