using Application.Common;
using Application.Common.Exceptions;
using Application.Consultations.DTOs;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Tests.Helpers;

namespace Tests.Services;

public class ConsultationServiceTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly ConsultationService _service;

    public ConsultationServiceTests()
    {
        _context = TestDbContextFactory.Create();
        var uow = new UnitOfWork(_context);
        _service = new ConsultationService(uow, NullLogger<ConsultationService>.Instance);
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldCreateConsultation_WhenDataIsValid()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        var dto = new CreateConsultationDto(
            patient.Id, doctor.Id, scheduledAt, "First visit");

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patient.Id);
        result.DoctorId.Should().Be(doctor.Id);
        result.Status.Should().Be(ConsultationStatus.Scheduled);
        result.Notes.Should().Be("First visit");
    }

    // [Fact]
    // public async Task CreateAsync_ShouldThrow_WhenPatientNotFound()
    // {
    //     // Arrange
    //     var (_, doctor, _) = await SeedData.SeedBasicDataAsync(_context);
    //
    //     var dto = new CreateConsultationDto(
    //         Guid.NewGuid(),     // ← patient inexistant
    //         doctor.Id,
    //         DateTime.UtcNow.AddDays(1),
    //         null);
    //
    //     // Act & Assert
    //     await _service.Invoking(s => s.CreateAsync(dto))
    //         .Should().ThrowAsync<NotFoundException>()
    //         .WithMessage("*Patient*");
    // }

    // [Fact]
    // public async Task CreateAsync_ShouldThrow_WhenDoctorNotFound()
    // {
    //     // Arrange
    //     var (_, _, patient) = await SeedData.SeedBasicDataAsync(_context);
    //
    //     var dto = new CreateConsultationDto(
    //         patient.Id,
    //         Guid.NewGuid(),     // ← médecin inexistant
    //         DateTime.UtcNow.AddDays(1),
    //         null);
    //
    //     // Act & Assert
    //     await _service.Invoking(s => s.CreateAsync(dto))
    //         .Should().ThrowAsync<NotFoundException>()
    //         .WithMessage("*Doctor*");
    // }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenSlotAlreadyTaken()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        // Première consultation — succès
        await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, scheduledAt, null));

        // Act & Assert — même créneau
        await _service.Invoking(s => s.CreateAsync(
            new CreateConsultationDto(patient.Id, doctor.Id, scheduledAt, null)))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already has an appointment*");
    }

    [Fact]
    public async Task CreateAsync_ShouldAllow_DifferentSlotsForSameDoctor()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        // Act — deux créneaux différents pour le même médecin et patient
        var c1 = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));
        var c2 = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(2), null));

        // Assert
        c1.Id.Should().NotBe(c2.Id);
        _context.Consultations.Should().HaveCount(2);
    }

    // ── STATUS ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ShouldTransitionToCompleted_WhenScheduled()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        // Act
        var result = await _service.UpdateStatusAsync(
            consultation.Id, ConsultationStatus.Completed);

        // Assert
        result!.Status.Should().Be(ConsultationStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldTransitionToCancelled_WhenScheduled()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        // Act
        var result = await _service.UpdateStatusAsync(
            consultation.Id, ConsultationStatus.Cancelled);

        // Assert
        result!.Status.Should().Be(ConsultationStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenTransitionFromCompletedIsAttempted()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        await _service.UpdateStatusAsync(consultation.Id, ConsultationStatus.Completed);

        // Act & Assert — Completed → Scheduled interdit
        await _service.Invoking(s =>
            s.UpdateStatusAsync(consultation.Id, ConsultationStatus.Scheduled))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Transition*not allowed*");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenTransitionFromCancelledIsAttempted()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        await _service.UpdateStatusAsync(consultation.Id, ConsultationStatus.Cancelled);

        // Act & Assert — Cancelled → Completed interdit
        await _service.Invoking(s =>
            s.UpdateStatusAsync(consultation.Id, ConsultationStatus.Completed))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Transition*not allowed*");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenConsultationNotFound()
    {
        await _service.Invoking(s =>
            s.UpdateStatusAsync(Guid.NewGuid(), ConsultationStatus.Completed))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Consultation*");
    }

    // ── CANCEL ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_ShouldCancel_WhenConsultationIsScheduled()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        // Act
        var result = await _service.CancelAsync(consultation.Id);

        // Assert
        result.Should().BeTrue();

        var saved = await _context.Consultations.FindAsync(consultation.Id);
        saved!.Status.Should().Be(ConsultationStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenAlreadyCompleted()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = await _service.CreateAsync(new CreateConsultationDto(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1), null));

        await _service.UpdateStatusAsync(consultation.Id, ConsultationStatus.Completed);

        // Act & Assert
        await _service.Invoking(s => s.CancelAsync(consultation.Id))
            .Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*scheduled*");
    }

    [Fact]
    public async Task CancelAsync_ShouldThrow_WhenConsultationNotFound()
    {
        await _service.Invoking(s => s.CancelAsync(Guid.NewGuid()))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Consultation*");
    }

    // ── PLANNING ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTodayByDoctorAsync_ShouldReturnOnlyTodaysConsultations()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.AddRange(
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.Date.AddHours(9),  // aujourd'hui
                Status      = ConsultationStatus.Scheduled
            },
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(1),        // demain
                Status      = ConsultationStatus.Scheduled
            },
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.Date.AddHours(14), // aujourd'hui
                Status      = ConsultationStatus.Cancelled        // annulée → exclue
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTodayByDoctorAsync(doctor.Id);

        // Assert — seulement la consultation du matin (l'annulée est exclue)
        result.Should().HaveCount(1);
        result[0].DoctorId.Should().Be(doctor.Id);
    }

    [Fact]
    public async Task GetUpcomingByPatientAsync_ShouldReturnOnlyFutureScheduled()
    {
        // Arrange
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.AddRange(
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(1),    // futur scheduled ✔
                Status      = ConsultationStatus.Scheduled
            },
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(2),    // futur scheduled ✔
                Status      = ConsultationStatus.Scheduled
            },
            new Domain.Entities.Consultation
            {
                PatientId   = patient.Id,
                DoctorId    = doctor.Id,
                ScheduledAt = DateTime.UtcNow.AddDays(-1),   // passé → exclu
                Status      = ConsultationStatus.Completed
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetUpcomingByPatientAsync(
            patient.Id, new PaginationParams { Page = 1, PageSize = 10 });

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(c =>
            c.Status.Should().Be(ConsultationStatus.Scheduled));
    }

    public void Dispose() => _context.Dispose();
}