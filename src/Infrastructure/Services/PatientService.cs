using Application.Common;
using Application.Patients.DTOs;
using Application.Patients.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PatientService(HospitalDbContext context) : IPatientService
{
    public async Task<PagedResult<PatientDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Patients
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PatientDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => ToDto(p))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatientDto?> GetByRecordNumberAsync(
        string recordNumber,
        CancellationToken ct = default)
    {
        return await context.Patients
            .AsNoTracking()
            .Where(p => p.RecordNumber == recordNumber)
            .Select(p => ToDto(p))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatientDto> CreateAsync(
        CreatePatientDto dto,
        CancellationToken ct = default)
    {
        var exists = await context.Patients.AnyAsync(
            p => p.RecordNumber == dto.RecordNumber || p.Email == dto.Email, ct);

        if (exists)
            throw new InvalidOperationException(
                $"A patient with RecordNumber '{dto.RecordNumber}' or Email '{dto.Email}' already exists.");

        var patient = new Patient
        {
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            BirthDate    = dto.BirthDate,
            RecordNumber = dto.RecordNumber,
            Email        = dto.Email,
            Phone        = dto.Phone,
            Address      = dto.Address
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync(ct);

        return ToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(
        Guid id,
        UpdatePatientDto dto,
        CancellationToken ct = default)
    {
        var patient = await context.Patients.FindAsync([id], ct);
        if (patient is null) return null;

        var emailTaken = await context.Patients.AnyAsync(
            p => p.Email == dto.Email && p.Id != id, ct);

        if (emailTaken)
            throw new InvalidOperationException(
                $"Email '{dto.Email}' is already used by another patient.");

        patient.FirstName = dto.FirstName;
        patient.LastName  = dto.LastName;
        patient.BirthDate = dto.BirthDate;
        patient.Email     = dto.Email;
        patient.Phone     = dto.Phone;
        patient.Address   = dto.Address;

        await context.SaveChangesAsync(ct);
        return ToDto(patient);
    }

    public async Task<PagedResult<PatientDto>> GetAllAlphabeticalAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var query = context.Patients
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PatientDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<PatientDto>> SearchByNameAsync(
        string name,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new PagedResult<PatientDto>
            {
                Items = [],
                TotalCount = 0,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            };

        var normalized = name.Trim().ToLower();

        var query = context.Patients
            .AsNoTracking()
            .Where(p =>
                p.LastName.ToLower().Contains(normalized) ||
                p.FirstName.ToLower().Contains(normalized))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return new PagedResult<PatientDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var hasActiveConsultations = await context.Consultations.AnyAsync(
            c => c.PatientId == id &&
                 c.Status != ConsultationStatus.Cancelled, ct);

        if (hasActiveConsultations)
            throw new InvalidOperationException(
                "Cannot delete a patient with active or completed consultations. " +
                "Cancel all consultations before deleting.");

        var deleted = await context.Patients
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }

    private static PatientDto ToDto(Patient p) => new(
        p.Id, p.FirstName, p.LastName, p.BirthDate,
        p.RecordNumber, p.Email, p.Phone, p.Address, p.CreatedAt
    ); 
}