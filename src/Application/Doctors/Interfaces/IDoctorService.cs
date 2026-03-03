using Application.Common;
using Application.Doctors.DTOs;

namespace Application.Doctors.Interfaces;

public interface IDoctorService
{
    Task<PagedResult<DoctorDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<DoctorDto>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
    Task<DoctorDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DoctorDto> CreateAsync(CreateDoctorDto dto, CancellationToken ct = default);
    Task<DoctorDto?> UpdateAsync(Guid id, UpdateDoctorDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}