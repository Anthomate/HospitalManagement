using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("Consultations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(c => c.PatientId)
            .HasColumnName("PatientId")
            .IsRequired();

        builder.Property(c => c.DoctorId)
            .HasColumnName("DoctorId")
            .IsRequired();

        builder.Property(c => c.ScheduledAt)
            .HasColumnName("ScheduledAt")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Notes)
            .HasColumnName("Notes")
            .HasMaxLength(2000);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
        
        builder.Property(c => c.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(c => new { c.PatientId, c.DoctorId, c.ScheduledAt })
            .IsUnique()
            .HasDatabaseName("IX_Consultations_Patient_Doctor_ScheduledAt");

        builder.HasIndex(c => c.PatientId)
            .HasDatabaseName("IX_Consultations_PatientId");
        
        builder.HasIndex(c => new { c.DoctorId, c.ScheduledAt })
            .HasDatabaseName("IX_Consultations_DoctorId_ScheduledAt");

        builder.HasIndex(c => c.DoctorId)
            .HasDatabaseName("IX_Consultations_DoctorId");

        builder.HasIndex(c => c.ScheduledAt)
            .HasDatabaseName("IX_Consultations_ScheduledAt");

        builder.HasIndex(c => new { c.DoctorId, c.ScheduledAt })
            .HasFilter("\"Status\" = 'Scheduled'")
            .HasDatabaseName("IX_Consultations_DoctorId_ScheduledAt_Scheduled");
        
        builder.HasOne(c => c.Patient)
            .WithMany(p => p.Consultations)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Consultations_Patients");

        builder.HasOne(c => c.Doctor)
            .WithMany(d => d.Consultations)
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Consultations_Doctors");
    }
}