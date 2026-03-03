using Application.Common;
using Application.Consultations.DTOs;
using Application.Consultations.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ConsultationService(HospitalDbContext context) : IConsultationService
{
    private static ConsultationDto ToDto(Consultation c) => new(
        c.Id,
        c.PatientId,
        $"{c.Patient.FirstName} {c.Patient.LastName}",
        c.DoctorId,
        $"{c.Doctor.FirstName} {c.Doctor.LastName}",
        c.ScheduledAt,
        c.Status,
        c.Notes,
        c.CreatedAt
    );

    private IQueryable<Consultation> BaseQuery() =>
        context.Consultations
            .AsNoTracking()
            .Include(c => c.Patient)
            .Include(c => c.Doctor);

    public async Task<PagedResult<ConsultationDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = BaseQuery().OrderBy(c => c.ScheduledAt);
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = BaseQuery()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ScheduledAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetByDoctorAsync(
        Guid doctorId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = BaseQuery()
            .Where(c => c.DoctorId == doctorId)
            .OrderBy(c => c.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<ConsultationDto>> GetByStatusAsync(
        ConsultationStatus status,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = BaseQuery()
            .Where(c => c.Status == status)
            .OrderBy(c => c.ScheduledAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<ConsultationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await BaseQuery()
            .Where(c => c.Id == id)
            .Select(c => ToDto(c))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ConsultationDto> CreateAsync(
        CreateConsultationDto dto,
        CancellationToken ct = default)
    {
        var patientExists = await context.Patients.AnyAsync(p => p.Id == dto.PatientId, ct);
        if (!patientExists)
            throw new InvalidOperationException($"Patient '{dto.PatientId}' not found.");

        var doctorExists = await context.Doctors.AnyAsync(d => d.Id == dto.DoctorId, ct);
        if (!doctorExists)
            throw new InvalidOperationException($"Doctor '{dto.DoctorId}' not found.");

        var slotTaken = await context.Consultations.AnyAsync(
            c => c.PatientId == dto.PatientId
              && c.DoctorId == dto.DoctorId
              && c.ScheduledAt == dto.ScheduledAt, ct);

        if (slotTaken)
            throw new InvalidOperationException(
                "This patient already has an appointment with this doctor at the same time.");

        var consultation = new Consultation
        {
            PatientId   = dto.PatientId,
            DoctorId    = dto.DoctorId,
            ScheduledAt = dto.ScheduledAt,
            Notes       = dto.Notes
        };

        context.Consultations.Add(consultation);
        await context.SaveChangesAsync(ct);

        return (await GetByIdAsync(consultation.Id, ct))!;
    }

    public async Task<PagedResult<ConsultationDto>> GetUpcomingByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var query = BaseQuery()
            .Where(c => c.PatientId == patientId
                     && c.ScheduledAt > now
                     && c.Status == ConsultationStatus.Scheduled)
            .OrderBy(c => c.ScheduledAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return new PagedResult<ConsultationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<IReadOnlyList<ConsultationDto>> GetTodayByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await BaseQuery()
            .Where(c => c.DoctorId == doctorId
                     && c.ScheduledAt >= today
                     && c.ScheduledAt < tomorrow
                     && c.Status != ConsultationStatus.Cancelled)
            .OrderBy(c => c.ScheduledAt)
            .Select(c => ToDto(c))
            .ToListAsync(ct);
    }

    public async Task<ConsultationDto?> UpdateStatusAsync(
        Guid id,
        ConsultationStatus newStatus,
        CancellationToken ct = default)
    {
        var consultation = await context.Consultations.FindAsync([id], ct);
        if (consultation is null) return null;

        var allowed = (consultation.Status, newStatus) switch
        {
            (ConsultationStatus.Scheduled,  ConsultationStatus.Completed)  => true,
            (ConsultationStatus.Scheduled,  ConsultationStatus.Cancelled)  => true,
            (ConsultationStatus.Cancelled,  _)                             => false,
            (ConsultationStatus.Completed,  _)                             => false,
            _                                                               => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Transition from '{consultation.Status}' to '{newStatus}' is not allowed.");

        consultation.Status = newStatus;
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var dbValues = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new InvalidOperationException("The record was deleted by another user.");

            throw new ConcurrencyConflictException(
                "The record was modified by another user. Please review and retry.",
                clientValues: entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject()
            );
        }

        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var result = await context.Consultations
            .Where(c => c.Id == id && c.Status == ConsultationStatus.Scheduled)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Status, ConsultationStatus.Cancelled)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow),
                ct);

        if (result == 0)
        {
            var exists = await context.Consultations.AnyAsync(c => c.Id == id, ct);
            if (!exists)
                throw new InvalidOperationException($"Consultation '{id}' not found.");

            throw new InvalidOperationException(
                "Only scheduled consultations can be cancelled.");
        }

        return true;
    }
}