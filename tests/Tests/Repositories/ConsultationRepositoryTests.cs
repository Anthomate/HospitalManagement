using Application.Common;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Tests.Helpers;

namespace Tests.Repositories;

public class ConsultationRepositoryTests : IDisposable
{
    private readonly HospitalDbContext _context;
    private readonly ConsultationRepository _repository;

    public ConsultationRepositoryTests()
    {
        _context    = TestDbContextFactory.Create();
        _repository = new ConsultationRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConsultation_WhenExists()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var consultation = new Consultation
        {
            PatientId   = patient.Id,
            DoctorId    = doctor.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Status      = ConsultationStatus.Scheduled
        };
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(consultation.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(consultation.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task SlotTakenAsync_ShouldReturnTrue_WhenSlotExists()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);
        var scheduledAt = DateTime.UtcNow.AddDays(1);

        _context.Consultations.Add(new Consultation
        {
            PatientId   = patient.Id,
            DoctorId    = doctor.Id,
            ScheduledAt = scheduledAt,
            Status      = ConsultationStatus.Scheduled
        });
        await _context.SaveChangesAsync();

        var result = await _repository.SlotTakenAsync(patient.Id, doctor.Id, scheduledAt);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SlotTakenAsync_ShouldReturnFalse_WhenSlotIsFree()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        var result = await _repository.SlotTakenAsync(
            patient.Id, doctor.Id, DateTime.UtcNow.AddDays(1));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByPatientAsync_ShouldReturnOnlyPatientConsultations()
    {
        var (dept, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        var otherPatient = SeedData.CreatePatient("REC-002", "other@test.com");
        _context.Patients.Add(otherPatient);
        await _context.SaveChangesAsync();

        _context.Consultations.AddRange(
            new Consultation { PatientId = patient.Id,      DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(1), Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = patient.Id,      DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(2), Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = otherPatient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(3), Status = ConsultationStatus.Scheduled }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetByPatientAsync(
            patient.Id, new PaginationParams { Page = 1, PageSize = 10 });

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(c => c.PatientId.Should().Be(patient.Id));
    }

    [Fact]
    public async Task GetByDoctorAsync_ShouldReturnOnlyDoctorConsultations()
    {
        var (dept, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        var otherDoctor = SeedData.CreateDoctor(dept.Id, "LIC-002", "other@hospital.com");
        _context.Doctors.Add(otherDoctor);
        await _context.SaveChangesAsync();

        _context.Consultations.AddRange(
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id,      ScheduledAt = DateTime.UtcNow.AddDays(1), Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = patient.Id, DoctorId = otherDoctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(2), Status = ConsultationStatus.Scheduled }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetByDoctorAsync(
            doctor.Id, new PaginationParams { Page = 1, PageSize = 10 });

        result.TotalCount.Should().Be(1);
        result.Items[0].DoctorId.Should().Be(doctor.Id);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnOnlyMatchingStatus()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.AddRange(
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(1), Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(2), Status = ConsultationStatus.Completed },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(3), Status = ConsultationStatus.Cancelled }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetByStatusAsync(
            ConsultationStatus.Scheduled, new PaginationParams { Page = 1, PageSize = 10 });

        result.TotalCount.Should().Be(1);
        result.Items[0].Status.Should().Be(ConsultationStatus.Scheduled);
    }

    [Fact]
    public async Task GetUpcomingByPatientAsync_ShouldReturnOnlyFutureScheduled()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.AddRange(
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(1),  Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(-1), Status = ConsultationStatus.Completed },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(2),  Status = ConsultationStatus.Cancelled }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetUpcomingByPatientAsync(
            patient.Id, new PaginationParams { Page = 1, PageSize = 10 });

        result.TotalCount.Should().Be(1);
        result.Items[0].Status.Should().Be(ConsultationStatus.Scheduled);
        result.Items[0].ScheduledAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetTodayByDoctorAsync_ShouldReturnOnlyTodayNonCancelled()
    {
        var (_, doctor, patient) = await SeedData.SeedBasicDataAsync(_context);

        _context.Consultations.AddRange(
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.Date.AddHours(9),  Status = ConsultationStatus.Scheduled },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.Date.AddHours(14), Status = ConsultationStatus.Cancelled },
            new Consultation { PatientId = patient.Id, DoctorId = doctor.Id, ScheduledAt = DateTime.UtcNow.AddDays(1),        Status = ConsultationStatus.Scheduled }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetTodayByDoctorAsync(doctor.Id);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(ConsultationStatus.Scheduled);
    }

    public void Dispose() => _context.Dispose();
}