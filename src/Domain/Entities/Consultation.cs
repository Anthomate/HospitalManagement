using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Consultation : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    public ConsultationStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}