using Application.AdminStaff.DTOs;
using Application.Common.Exceptions;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class AdminStaffServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly AdminStaffService _service;

    public AdminStaffServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var uow  = new UnitOfWork(_context);
        _service = new AdminStaffService(uow, NullLogger<AdminStaffService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAdminStaff_WhenDataIsValid()
    {
        var dept = SeedData.CreateDepartment();
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var dto = new CreateAdminStaffDto(
            "Lucas", "Bernard", "lucas@hospital.com", null,
            new Address(), new DateOnly(2022, 6, 1), 2200m,
            "Receptionist", dept.Id);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Function.Should().Be("Receptionist");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var (dept, doctor, _) = await SeedData.SeedBasicDataAsync(_context);

        var dto = new CreateAdminStaffDto(
            "Lucas", "Bernard", "doctor@hospital.com", null,
            new Address(), new DateOnly(2022, 6, 1), 2200m,
            "Receptionist", dept.Id);

        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenDepartmentNotFound()
    {
        var dto = new CreateAdminStaffDto(
            "Lucas", "Bernard", "lucas@hospital.com", null,
            new Address(), new DateOnly(2022, 6, 1), 2200m,
            "Receptionist", Guid.NewGuid());

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
    //     var dto = new CreateAdminStaffDto(
    //         "Lucas", "Bernard", "lucas@hospital.com", null,
    //         new Address(), new DateOnly(2022, 6, 1), 2200m,
    //         "Receptionist", dept.Id);
    //
    //     var admin = await _service.CreateAsync(dto);
    //     var result = await _service.DeleteAsync(admin.Id);
    //
    //     result.Should().BeTrue();
    //     _context.AdminStaffs.Should().BeEmpty();
    // }

    public void Dispose() => _context.Dispose();
}