using Application.Common;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class PatientRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly PatientRepository _repository;

    public PatientRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new PatientRepository(_context);
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenExists()
    {
        // Arrange
        var patient = SeedData.CreatePatient();
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(patient.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(patient.Id);
        result.RecordNumber.Should().Be("REC-001");
    }

    // [Fact]
    // public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _repository.GetByIdAsync(Guid.NewGuid());
    //     result.Should().BeNull();
    // }

    // ── EXISTS BY EMAIL ───────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var patient = SeedData.CreatePatient(email: "marie@test.com");
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByEmailAsync("marie@test.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailNotFound()
    {
        var result = await _repository.ExistsByEmailAsync("inconnu@test.com");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenExcludingCurrentPatient()
    {
        // Arrange — cas UPDATE : on exclut le patient lui-même
        var patient = SeedData.CreatePatient(email: "marie@test.com");
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // Act — même email mais on exclut cet Id
        var result = await _repository.ExistsByEmailAsync("marie@test.com", patient.Id);

        // Assert — ne doit pas se considérer comme doublon
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailBelongsToAnotherPatient()
    {
        // Arrange
        var p1 = SeedData.CreatePatient("REC-001", "marie@test.com");
        var p2 = SeedData.CreatePatient("REC-002", "pierre@test.com");
        _context.Patients.AddRange(p1, p2);
        await _context.SaveChangesAsync();

        // Act — p2 tente de prendre l'email de p1
        var result = await _repository.ExistsByEmailAsync("marie@test.com", p2.Id);

        // Assert — c'est bien un doublon (appartient à p1)
        result.Should().BeTrue();
    }

    // ── EXISTS BY RECORD NUMBER ───────────────────────────────────────────────

    [Fact]
    public async Task ExistsByRecordNumberAsync_ShouldReturnTrue_WhenExists()
    {
        var patient = SeedData.CreatePatient("REC-001");
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByRecordNumberAsync("REC-001");
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByRecordNumberAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _repository.ExistsByRecordNumberAsync("REC-999");
        result.Should().BeFalse();
    }

    // ── HAS ACTIVE CONSULTATIONS ──────────────────────────────────────────────

    [Fact]
    public async Task HasActiveConsultationsAsync_ShouldReturnTrue_WhenScheduledConsultationExists()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.Add(new Domain.Entities.Consultation
        {
            PatientId   = patient.Id,
            DoctorId    = doctor.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status      = ConsultationStatus.Scheduled
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasActiveConsultationsAsync(patient.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveConsultationsAsync_ShouldReturnFalse_WhenOnlyCancelledExists()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.Add(new Domain.Entities.Consultation
        {
            PatientId   = patient.Id,
            DoctorId    = doctor.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status      = ConsultationStatus.Cancelled   // ← annulée, ne bloque pas
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasActiveConsultationsAsync(patient.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveConsultationsAsync_ShouldReturnFalse_WhenNoConsultations()
    {
        var patient = SeedData.CreatePatient();
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var result = await _repository.HasActiveConsultationsAsync(patient.Id);
        result.Should().BeFalse();
    }

    // ── GET ALL ALPHABETICAL ──────────────────────────────────────────────────

    // [Fact]
    // public async Task GetAllAlphabeticalAsync_ShouldReturnCorrectPage()
    // {
    //     // Arrange — 5 patients
    //     _context.Patients.AddRange(
    //         SeedData.CreatePatient("REC-001", "a@test.com", "Zimmermann"),
    //         SeedData.CreatePatient("REC-002", "b@test.com", "Abramovic"),
    //         SeedData.CreatePatient("REC-003", "c@test.com", "Martin"),
    //         SeedData.CreatePatient("REC-004", "d@test.com", "Bernard"),
    //         SeedData.CreatePatient("REC-005", "e@test.com", "Fontaine")
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     // Act — page 2 avec 2 éléments par page
    //     var result = await _repository.GetAllAlphabeticalAsync(
    //         new PaginationParams { Page = 2, PageSize = 2 });
    //
    //     // Assert — ordre : Abramovic, Bernard, Fontaine, Martin, Zimmermann
    //     //          page 2 → Fontaine, Martin
    //     result.TotalCount.Should().Be(5);
    //     result.Items.Should().HaveCount(2);
    //     result.Items[0].LastName.Should().Be("Fontaine");
    //     result.Items[1].LastName.Should().Be("Martin");
    //     result.HasPreviousPage.Should().BeTrue();
    //     result.HasNextPage.Should().BeTrue();
    // }

    // ── GET BY RECORD NUMBER ──────────────────────────────────────────────────

    // [Fact]
    // public async Task GetByRecordNumberAsync_ShouldReturnPatient_WhenExists()
    // {
    //     var patient = SeedData.CreatePatient("REC-123");
    //     _context.Patients.Add(patient);
    //     await _context.SaveChangesAsync();
    //
    //     var result = await _repository.GetByRecordNumberAsync("REC-123");
    //
    //     result.Should().NotBeNull();
    //     result!.RecordNumber.Should().Be("REC-123");
    // }

    // [Fact]
    // public async Task GetByRecordNumberAsync_ShouldReturnNull_WhenNotFound()
    // {
    //     var result = await _repository.GetByRecordNumberAsync("REC-999");
    //     result.Should().BeNull();
    // }

    public void Dispose() => _context.Dispose();
}