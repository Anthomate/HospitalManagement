using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Patients.DTOs;
using Application.Patients.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PatientService(
    IUnitOfWork uow,
    ILogger<PatientService> logger) : IPatientService
{
    public async Task<PagedResult<PatientDto>> GetAllAsync(
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var result = await uow.Patients.GetAllAlphabeticalAsync(pagination, ct);

        return new PagedResult<PatientDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
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
                Items = [], TotalCount = 0,
                Page = pagination.Page, PageSize = pagination.PageSize
            };

        var result = await uow.Patients.SearchByNameAsync(name, pagination, ct);

        return new PagedResult<PatientDto>
        {
            Items      = result.Items.Select(ToDto).ToList(),
            TotalCount = result.TotalCount,
            Page       = result.Page,
            PageSize   = result.PageSize
        };
    }

    public async Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await uow.Patients.GetByIdAsync(id, ct);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<PatientDto?> GetByRecordNumberAsync(
        string recordNumber,
        CancellationToken ct = default)
    {
        var patient = await uow.Patients.GetByRecordNumberAsync(recordNumber, ct);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(
        CreatePatientDto dto,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating patient {FirstName} {LastName} with RecordNumber {RecordNumber}",
            dto.FirstName, dto.LastName, dto.RecordNumber);

        if (await uow.Patients.ExistsByRecordNumberAsync(dto.RecordNumber, ct))
            throw new AlreadyExistsException("Patient", "RecordNumber", dto.RecordNumber);

        if (await uow.Patients.ExistsByEmailAsync(dto.Email, null, ct))
            throw new AlreadyExistsException("Patient", "Email", dto.Email);

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

        await uow.Patients.AddAsync(patient, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Patient {Id} created successfully", patient.Id);
        return ToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(
        Guid id,
        UpdatePatientDto dto,
        CancellationToken ct = default)
    {
        var patient = await uow.Patients.GetByIdAsync(id, ct);
        if (patient is null) return null;

        if (await uow.Patients.ExistsByEmailAsync(dto.Email, id, ct))
            throw new AlreadyExistsException("Patient", "Email", dto.Email);

        patient.FirstName = dto.FirstName;
        patient.LastName  = dto.LastName;
        patient.BirthDate = dto.BirthDate;
        patient.Email     = dto.Email;
        patient.Phone     = dto.Phone;
        patient.Address   = dto.Address;

        uow.Patients.Update(patient);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry     = ex.Entries.Single();
            var dbValues  = await entry.GetDatabaseValuesAsync(ct);

            if (dbValues is null)
                throw new NotFoundException("Patient", id);

            throw new ConcurrencyConflictException(
                "The patient was modified by another user. Please review and retry.",
                clientValues:   entry.CurrentValues.ToObject(),
                databaseValues: dbValues.ToObject());
        }

        logger.LogInformation("Patient {Id} updated successfully", id);
        return ToDto(patient);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogWarning("Attempting to delete patient {PatientId}", id);

        var patient = await uow.Patients.GetByIdAsync(id, ct);
        if (patient is null) return false;

        if (await uow.Patients.HasActiveConsultationsAsync(id, ct))
            throw new BusinessRuleException(
                "Cannot delete a patient with active or completed consultations. " +
                "Cancel all consultations before deleting.");

        uow.Patients.Remove(patient);
        await uow.SaveChangesAsync(ct);

        logger.LogWarning("Patient {PatientId} deleted", id);
        return true;
    }

    private static PatientDto ToDto(Patient p) => new(
        p.Id, p.FirstName, p.LastName, p.BirthDate,
        p.RecordNumber, p.Email, p.Phone, p.Address, p.CreatedAt);
}