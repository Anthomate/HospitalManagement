using Application.Common;
using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ConsultationRepository(HospitalDbContext context)
    : BaseRepository<Consultation>(context), IConsultationRepository
{
    public async Task<bool> SlotTakenAsync(
        Guid patientId,
        Guid doctorId,
        DateTime scheduledAt,
        CancellationToken ct = default)
        => await DbSet.AnyAsync(
            c => c.PatientId == patientId &&
                 c.DoctorId == doctorId &&
                 c.ScheduledAt == scheduledAt, ct);

    public async Task<PagedResult<Consultation>> GetByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.ScheduledAt);

        return await ToPagedResultAsync(query, pagination, ct);
    }

    public async Task<PagedResult<Consultation>> GetByDoctorAsync(
        Guid doctorId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(c => c.DoctorId == doctorId)
            .OrderBy(c => c.ScheduledAt);

        return await ToPagedResultAsync(query, pagination, ct);
    }

    public async Task<PagedResult<Consultation>> GetByStatusAsync(
        ConsultationStatus status,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(c => c.Status == status)
            .OrderBy(c => c.ScheduledAt);

        return await ToPagedResultAsync(query, pagination, ct);
    }

    public async Task<PagedResult<Consultation>> GetUpcomingByPatientAsync(
        Guid patientId,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var query = DbSet.AsNoTracking()
            .Where(c => c.PatientId == patientId &&
                        c.ScheduledAt > now &&
                        c.Status == ConsultationStatus.Scheduled)
            .OrderBy(c => c.ScheduledAt);

        return await ToPagedResultAsync(query, pagination, ct);
    }

    public async Task<IReadOnlyList<Consultation>> GetTodayByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await DbSet.AsNoTracking()
            .Where(c => c.DoctorId == doctorId &&
                        c.ScheduledAt >= today &&
                        c.ScheduledAt < tomorrow &&
                        c.Status != ConsultationStatus.Cancelled)
            .OrderBy(c => c.ScheduledAt)
            .ToListAsync(ct);
    }

    private static async Task<PagedResult<Consultation>> ToPagedResultAsync(
        IQueryable<Consultation> query,
        PaginationParams pagination,
        CancellationToken ct)
    {
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Consultation>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }
}