using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();
        
        builder.Property(p => p.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(p => p.BirthDate)
            .HasColumnName("BirthDate")
            .HasColumnType("date")
            .IsRequired();
        
        builder.Property(p => p.RecordNumber)
            .HasColumnName("RecordNumber")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(p => p.Email)
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Phone)
            .HasColumnName("Phone")
            .HasMaxLength(20);

        builder.Property(p => p.Address)
            .HasColumnName("Address")
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
        
        builder.HasIndex(p => p.RecordNumber)
            .IsUnique()
            .HasDatabaseName("IX_Patients_RecordNumber");
        
        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Patients_Email");
    }
}