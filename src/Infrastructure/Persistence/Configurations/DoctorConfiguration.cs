using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();
        
        builder.Property(d => d.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(d => d.Specialty)
            .HasColumnName("Specialty")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .HasColumnName("LicenseNumber")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.DepartmentId)
            .HasColumnName("DepartmentId")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
        
        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique()
            .HasDatabaseName("IX_Doctors_LicenseNumber");
        
        builder.HasIndex(d => d.DepartmentId)
            .HasDatabaseName("IX_Doctors_DepartmentId");
        
        builder.HasOne(d => d.Department)
            .WithMany(dep => dep.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Doctors_Departments");
    }
}