using Application.Common.Exceptions;
using Application.Nurses.DTOs;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class NurseServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly NurseService _service;

    public NurseServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var uow  = new UnitOfWork(_context);
        _service = new NurseService(uow, NullLogger<NurseService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNurse_WhenDataIsValid()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var dto = new CreateNurseDto(
            "Sophie", "Leclerc", "sophie@hospital.com", null,
            new Address(), new DateOnly(2021, 3, 1), 2800m,
            "NUR-001", "Emergency", "Senior", dept.Id);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.LicenseNumber.Should().Be("NUR-001");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var (dept, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var dto = new CreateNurseDto(
            "Sophie", "Leclerc", "doctor@hospital.com", null,
            new Address(), new DateOnly(2021, 3, 1), 2800m,
            "NUR-001", "Emergency", "Senior", dept.Id);

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDepartmentNotFound()
    {
        var dto = new CreateNurseDto(
            "Sophie", "Leclerc", "sophie@hospital.com", null,
            new Address(), new DateOnly(2021, 3, 1), 2800m,
            "NUR-001", "Emergency", "Senior", Guid.NewGuid());

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Department*");
    }

    // [Fact]
    // public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    // {
    //     var result = await _service.DeleteAsync(Guid.NewGuid());
    //     result.Should().BeFalse();
    // }

    // [Fact]
    // public async Task DeleteAsync_ShouldDelete_WhenExists()
    // {
    //     var dept = SeedData.CreateDepartment();
    //     _context.Departments.Add(dept);
    //     await _context.SaveChangesAsync();
    //
    //     var dto = new CreateNurseDto(
    //         "Sophie", "Leclerc", "sophie@hospital.com", null,
    //         new Address(), new DateOnly(2021, 3, 1), 2800m,
    //         "NUR-001", "Emergency", "Senior", dept.Id);
    //
    //     var nurse = await _service.CreateAsync(dto);
    //     var result = await _service.DeleteAsync(nurse.Id);
    //
    //     result.Should().BeTrue();
    //     _context.Nurses.Should().BeEmpty();
    // }

    public void Dispose() => _context.Dispose();
}