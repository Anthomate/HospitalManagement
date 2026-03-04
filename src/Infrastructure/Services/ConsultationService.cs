using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Consultations.DTOs;
using Application.Consultations.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ConsultationService(
    IUnitOfWork uow,
    ILogger<ConsultationService> logger) : IConsultationService
{
    public async Task<PagedResult<ConsultationDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetAllPagedAsync(pagination, ct);

        return new PagedResult<ConsultationDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<ConsultationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var consultation = await uow.Consultations.GetByIdAsync(id, ct);
        return consultation is null ? null : ToDto(consultation);
    }

    public async Task<PagedResult<ConsultationDto>> GetByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetByPatientAsync(patientId, pagination, ct);

        return new PagedResult<ConsultationDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetByDoctorAsync(
        Guid doctorId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetByDoctorAsync(doctorId, pagination, ct);

        return new PagedResult<ConsultationDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetByStatusAsync(
        ConsultationStatus status,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetByStatusAsync(status, pagination, ct);

        return new PagedResult<ConsultationDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetUpcomingByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetUpcomingByPatientAsync(patientId, pagination, ct);

        return new PagedResult<ConsultationDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<IReadOnlyList<ConsultationDto>> GetTodayByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        var result = await uow.Consultations.GetTodayByDoctorAsync(doctorId, ct);
        return result.Select(ToDto).ToList();
    }

    public async Task<ConsultationDto> CreateAsync(
        CreateConsultationDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating consultation — Patient: {PatientId}, Doctor: {DoctorId}, ScheduledAt: {ScheduledAt}",
            dto.PatientId, dto.DoctorId, dto.ScheduledAt);

        var patient = await uow.Patients.GetByIdAsync(dto.PatientId, ct);
        if (patient is null)
            throw new NotFoundException("Patient", dto.PatientId);

        var doctor = await uow.Doctors.GetByIdAsync(dto.DoctorId, ct);
        if (doctor is null)
            throw new NotFoundException("Doctor", dto.DoctorId);

        if (await uow.Consultations.SlotTakenAsync(dto.PatientId, dto.DoctorId, dto.ScheduledAt, ct))
            throw new BusinessRuleException(
                "This patient already has an appointment with this doctor at the same time.");

        var consultation = new Consultation
        {
            PatientId   = dto.PatientId,
            DoctorId    = dto.DoctorId,
            ScheduledAt = dto.ScheduledAt,
            Notes       = dto.Notes
        };

        await uow.Consultations.AddAsync(consultation, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Consultation {Id} created successfully", consultation.Id);
        return (await GetByIdAsync(consultation.Id, ct))!;
    }

    public async Task<ConsultationDto?> UpdateStatusAsync(
        Guid id,
        ConsultationStatus newStatus,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Updating status of consultation {ConsultationId} to {Status}", id, newStatus);

        var consultation = await uow.Consultations.GetByIdAsync(id, ct);
        if (consultation is null)
            throw new NotFoundException("Consultation", id);

        var allowed = (consultation.Status, newStatus) switch
        {
            (ConsultationStatus.Scheduled, ConsultationStatus.Completed) => true,
            (ConsultationStatus.Scheduled, ConsultationStatus.Cancelled) => true,
            _                                                             => false
        };

        if (!allowed)
            throw new BusinessRuleException(
                $"Transition from '{consultation.Status}' to '{newStatus}' is not allowed.");

        consultation.Status = newStatus;
        uow.Consultations.Update(consultation);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry    = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Consultation", id);

            throw new ConcurrencyConflictException(
                "The consultation was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Cancelling consultation {ConsultationId}", id);

        var consultation = await uow.Consultations.GetByIdAsync(id, ct);
        if (consultation is null)
            throw new NotFoundException("Consultation", id);

        if (consultation.Status != ConsultationStatus.Scheduled)
            throw new BusinessRuleException("Only scheduled consultations can be cancelled.");

        consultation.Status = ConsultationStatus.Cancelled;
        uow.Consultations.Update(consultation);
        await uow.SaveChangesAsync(ct);

        return true;
    }

    private static ConsultationDto ToDto(Consultation c) => new(
        c.Id,
        c.PatientId,
        c.Patient is not null ? $"{c.Patient.FirstName} {c.Patient.LastName}" : string.Empty,
        c.DoctorId,
        c.Doctor is not null  ? $"{c.Doctor.FirstName} {c.Doctor.LastName}"   : string.Empty,
        c.ScheduledAt,
        c.Status,
        c.Notes,
        c.CreatedAt
    );
}