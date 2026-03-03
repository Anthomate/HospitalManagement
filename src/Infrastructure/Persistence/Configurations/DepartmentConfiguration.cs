using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();
        
        builder.Property(d => d.Name)
            .HasColumnName("Name")
            .HasMaxLength(150)
            .IsRequired();
        
        builder.Property(d => d.Location)
            .HasColumnName("Location")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(d => d.MedicalDirectorId)
            .HasColumnName("MedicalDirectorId");
        
        builder.Property(d => d.ParentDepartmentId)
            .HasColumnName("ParentDepartmentId");
        
        builder.Property(d => d.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
        
        builder.HasOne(d => d.ParentDepartment)
            .WithMany(d => d.SubDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Departments_ParentDepartment");
        
        builder.HasIndex(d => d.Name)
            .IsUnique()
            .HasDatabaseName("ix_departments_name");
        
        builder.HasOne(d => d.MedicalDirector)
            .WithMany()
            .HasForeignKey(d => d.MedicalDirectorId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Departments_MedicalDirector");
        
        builder.HasIndex(d => d.ParentDepartmentId)
            .HasDatabaseName("IX_Departments_ParentDepartmentId");
    }
}