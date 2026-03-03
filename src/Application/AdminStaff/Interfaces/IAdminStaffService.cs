using Application.AdminStaff.DTOs;
using Application.Common;

namespace Application.AdminStaff.Interfaces;

public interface IAdminStaffService
{
    Task<PagedResult<AdminStaffDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<AdminStaffDto>> GetByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken ct = default);
    Task<AdminStaffDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminStaffDto> CreateAsync(CreateAdminStaffDto dto, CancellationToken ct = default);
    Task<AdminStaffDto?> UpdateAsync(Guid id, UpdateAdminStaffDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}