using Application.Common;
using Application.Common.Exceptions;
using Application.Patients.DTOs;
using Domain.ValueObjects;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class PatientServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _context = TestDbContextFactory.Create();

        // On construit le UnitOfWork réel avec la base InMemory
        var uow = new UnitOfWork(_context);
        _service = new PatientService(uow, NullLogger<PatientService>.Instance);
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    // [Fact]
    // public async Task CreateAsync_ShouldCreatePatient_WhenDataIsValid()
    // {
    //     // Arrange
    //     var dto = new CreatePatientDto(
    //         "Marie", "Dupont",
    //         new DateOnly(1985, 6, 15),
    //         "REC-001", "marie@test.com",
    //         null, new Address());
    //
    //     // Act
    //     var result = await _service.CreateAsync(dto);
    //
    //     // Assert
    //     result.Should().NotBeNull();
    //     result.RecordNumber.Should().Be("REC-001");
    //     result.Email.Should().Be("marie@test.com");
    //     result.FirstName.Should().Be("Marie");
    //     _context.Patients.Should().HaveCount(1);
    // }

    [Fact]
    public async Task CreateAsync_ShouldThrowAlreadyExistsException_WhenRecordNumberDuplicated()
    {
        // Arrange
        _context.Patients.Add(SeedData.CreatePatient("REC-001", "other@test.com"));
        await _context.SaveChangesAsync();

        var dto = new CreatePatientDto(
            "Pierre", "Martin",
            new DateOnly(1990, 1, 1),
            "REC-001",              // ← doublon
            "nouveau@test.com",
            null, new Address());

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*RecordNumber*REC-001*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowAlreadyExistsException_WhenEmailDuplicated()
    {
        // Arrange
        _context.Patients.Add(SeedData.CreatePatient("REC-001", "marie@test.com"));
        await _context.SaveChangesAsync();

        var dto = new CreatePatientDto(
            "Pierre", "Martin",
            new DateOnly(1990, 1, 1),
            "REC-002",
            "marie@test.com",       // ← doublon
            null, new Address());

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Email*marie@test.com*");
    }

    // ── GET ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenExists()
    {
        // Arrange
        var patient = SeedData.CreatePatient();
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(patient.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(patient.Id);
        result.RecordNumber.Should().Be("REC-001");
    }

    // [Fact]
    // public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _service.GetByIdAsync(Guid.NewGuid());
    //     result.Should().BeNull();
    // }

    // [Fact]
    // public async Task GetAllAsync_ShouldReturnPagedResultOrderedAlphabetically()
    // {
    //     // Arrange
    //     _context.Patients.AddRange(
    //         SeedData.CreatePatient("REC-001", "a@test.com", lastName: "Zimmermann"),
    //         SeedData.CreatePatient("REC-002", "b@test.com", lastName: "Abramovic"),
    //         SeedData.CreatePatient("REC-003", "c@test.com", lastName: "Martin")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     // Act
    //     var result = await _service.GetAllAsync(
    //         new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     // Assert
    //     result.TotalCount.Should().Be(3);
    //     result.Items.Should().HaveCount(3);
    //     result.Items[0].LastName.Should().Be("Abramovic");   // ordre alphabétique
    //     result.Items[1].LastName.Should().Be("Martin");
    //     result.Items[2].LastName.Should().Be("Zimmermann");
    // }

    // [Fact]
    // public async Task GetAllAsync_ShouldPaginateCorrectly()
    // {
    //     // Arrange
    //     for (int i = 1; i <= 5; i++)
    //         _context.Patients.Add(
    //             SeedData.CreatePatient($"REC-00{i}", $"p{i}@test.com"));
    //     await _context.SaveChangesAsync();
    //
    //     // Act
    //     var page1 = await _service.GetAllAsync(new PaginationParams { Page = 1, PageSize = 2 });
    //     var page2 = await _service.GetAllAsync(new PaginationParams { Page = 2, PageSize = 2 });
    //     var page3 = await _service.GetAllAsync(new PaginationParams { Page = 3, PageSize = 2 });
    //
    //     // Assert
    //     page1.Items.Should().HaveCount(2);
    //     page1.HasPreviousPage.Should().BeFalse();
    //     page1.HasNextPage.Should().BeTrue();
    //
    //     page2.Items.Should().HaveCount(2);
    //     page2.HasPreviousPage.Should().BeTrue();
    //     page2.HasNextPage.Should().BeTrue();
    //
    //     page3.Items.Should().HaveCount(1);
    //     page3.HasNextPage.Should().BeFalse();
    // }

    // [Fact]
    // public async Task SearchByNameAsync_ShouldReturnMatchingPatients()
    // {
    //     // Arrange
    //     _context.Patients.AddRange(
    //         SeedData.CreatePatient("REC-001", "a@test.com", "Dupont"),
    //         SeedData.CreatePatient("REC-002", "b@test.com", "Durand"),
    //         SeedData.CreatePatient("REC-003", "c@test.com", "Martin")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     // Act
    //     var result = await _service.SearchByNameAsync(
    //         "du", new PaginationParams { Page = 1, PageSize = 10 });
    //
    //     // Assert — "Dupont" et "Durand" correspondent à "du"
    //     result.TotalCount.Should().Be(2);
    //     result.Items.Should().AllSatisfy(p =>
    //         p.LastName.ToLower().Should().Contain("du"));
    // }

    [Fact]
    public async Task SearchByNameAsync_ShouldReturnEmpty_WhenNameIsWhitespace()
    {
        var result = await _service.SearchByNameAsync(
            "   ", new PaginationParams { Page = 1, PageSize = 10 });

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenPatientExists()
    {
        // Arrange
        var patient = SeedData.CreatePatient();
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var dto = new UpdatePatientDto(
            "Marie-Claire", "Dupont",
            new DateOnly(1985, 6, 15),
            "marie@test.com", "0612345678",
            new Address { City = "Lyon" });

        // Act
        var result = await _service.UpdateAsync(patient.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Marie-Claire");
        result.Phone.Should().Be("0612345678");
        result.Address.City.Should().Be("Lyon");
    }

    // [Fact]
    // public async Task UpdateAsync_ShouldReturnNull_WhenPatientNotFound()
    // {
    //     var dto = new UpdatePatientDto(
    //         "X", "Y", new DateOnly(1990, 1, 1),
    //         "x@test.com", null, new Address());
    //
    //     var result = await _service.UpdateAsync(Guid.NewGuid(), dto);
    //     result.Should().BeNull();
    // }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenEmailTakenByAnotherPatient()
    {
        // Arrange
        var p1 = SeedData.CreatePatient("REC-001", "p1@test.com");
        var p2 = SeedData.CreatePatient("REC-002", "p2@test.com");
        _context.Patients.AddRange(p1, p2);
        await _context.SaveChangesAsync();

        // Tente de mettre l'email de p2 sur p1
        var dto = new UpdatePatientDto(
            "Marie", "Dupont",
            new DateOnly(1985, 6, 15),
            "p2@test.com",      // ← appartient à p2
            null, new Address());

        await _service.Invoking(s => s.UpdateAsync(p1.Id, dto))
            .Should().ThrowAsync<AlreadyExistsException>()
            .WithMessage("*Email*");
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    // [Fact]
    // public async Task DeleteAsync_ShouldReturnFalse_WhenPatientNotFound()
    // {
    //     var result = await _service.DeleteAsync(Guid.NewGuid());
    //     result.Should().BeFalse();
    // }

    // [Fact]
    // public async Task DeleteAsync_ShouldDeletePatient_WhenNoActiveConsultations()
    // {
    //     // Arrange
    //     var patient = SeedData.CreatePatient();
    //     _context.Patients.Add(patient);
    //     await _context.SaveChangesAsync();
    //
    //     // Act
    //     var result = await _service.DeleteAsync(patient.Id);
    //
    //     // Assert
    //     result.Should().BeTrue();
    //     _context.Patients.Should().BeEmpty();
    // }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenPatientHasActiveConsultations()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.Add(new Domain.Entities.Consultation
        {
            PatientId   = patient.Id,
            DoctorId    = doctor.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status      = Domain.Enums.ConsultationStatus.Scheduled
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        await _service.Invoking(s => s.DeleteAsync(patient.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*active*");
    }

    public void Dispose() => _context.Dispose();
}