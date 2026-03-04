using Application.Common;
using Application.Common.Exceptions;
using Application.Doctors.DTOs;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class DoctorServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var uow  = new UnitOfWork(_context);
        _service = new DoctorService(uow, NullLogger<DoctorService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDoctor_WhenDataIsValid()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var dto = new CreateDoctorDto(
            "Jean", "Martin", "jean@hospital.com", null,
            new Address(), new DateOnly(2020, 1, 1), 5000m,
            "Cardiology", "LIC-001", dept.Id);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.LicenseNumber.Should().Be("LIC-001");
        result.DepartmentId.Should().Be(dept.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var (dept, _, _) = await SeedData.SeedBasicDataAsync(_context);

        var dto = new CreateDoctorDto(
            "Paul", "Durand", "doctor@hospital.com", null,
            new Address(), new DateOnly(2021, 1, 1), 4500m,
            "Neurology", "LIC-002", dept.Id);

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Email*");
    }

    // [Fact]
    // public async Task CreateAsync_ShouldThrow_WhenLicenseAlreadyExists()
    // {
    //     var (dept, _, _) = await SeedData.SeedBasicDataAsync(_context);
    //
    //     var dto = new CreateDoctorDto(
    //         "Paul", "Durand", "new@hospital.com", null,
    //         new Address(), new DateOnly(2021, 1, 1), 4500m,
    //         "Neurology", "LIC-001", dept.Id);
    //
    //     await _service.Invoking(s => s.CreateAsync(dto))
    //         .Should().ThrowAsync<AlreadyExistsException>()
    //         .WithMessage("*LicenseNumber*");
    // }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDepartmentNotFound()
    {
        var dto = new CreateDoctorDto(
            "Jean", "Martin", "jean@hospital.com", null,
            new Address(), new DateOnly(2020, 1, 1), 5000m,
            "Cardiology", "LIC-001", Guid.NewGuid());

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Department*");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDoctor_WhenExists()
    {
        var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _service.GetByIdAsync(doctor.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(doctor.Id);
    }

    // [Fact]
    // public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _service.GetByIdAsync(Guid.NewGuid());
    //     result.Should().BeNull();
    // }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenDoctorIsDirector()
    {
        var (dept, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        dept.MedicalDirectorId = doctor.Id;
        await _context.SaveChangesAsync();

        await _service.Invoking(s => s.DeleteAsync(doctor.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*director*");
    }

    // [Fact]
    // public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    // {
    //     var result = await _service.DeleteAsync(Guid.NewGuid());
    //     result.Should().BeFalse();
    // }

    // [Fact]
    // public async Task DeleteAsync_ShouldDelete_WhenNotDirector()
    // {
    //     var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);
    //
    //     var result = await _service.DeleteAsync(doctor.Id);
    //
    //     result.Should().BeTrue();
    //     _context.Doctors.Should().BeEmpty();
    // }

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
    //     var result = await _service.GetByDepartmentAsync(
    //         dept1.Id, new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     result.TotalCount.Should().Be(2);
    //     result.Items.Should().AllSatisfy(d => d.DepartmentId.Should().Be(dept1.Id));
    // }

    public void Dispose() => _context.Dispose();
}