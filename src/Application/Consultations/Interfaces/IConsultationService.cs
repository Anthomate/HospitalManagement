using Application.Common;
using Application.Consultations.DTOs;
using Domain.Enums;

namespace Application.Consultations.Interfaces;

public interface IConsultationService
{
    Task<PagedResult<ConsultationDto>> GetAllAsync(PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> GetByPatientAsync(Guid patientId, PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> GetByDoctorAsync(Guid doctorId, PaginationParams pagination, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> GetByStatusAsync(ConsultationStatus status, PaginationParams pagination, CancellationToken ct = default);
    Task<ConsultationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ConsultationDto> CreateAsync(CreateConsultationDto dto, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ConsultationDto>> GetUpcomingByPatientAsync(Guid patientId, PaginationParams pagination, CancellationToken ct = default);
    Task<IReadOnlyList<ConsultationDto>> GetTodayByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<ConsultationDto?> UpdateStatusAsync(Guid id, ConsultationStatus newStatus, CancellationToken ct = default);
}