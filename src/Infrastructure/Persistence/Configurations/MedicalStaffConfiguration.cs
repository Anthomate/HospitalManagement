using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MedicalStaffConfiguration : IEntityTypeConfiguration<MedicalStaff>
{
    public void Configure(EntityTypeBuilder<MedicalStaff> builder)
    {
        builder.Property(m => m.LicenseNumber)
            .HasColumnName("LicenseNumber")
            .HasMaxLength(50);

        builder.HasIndex(m => m.LicenseNumber)
            .IsUnique()
            .HasDatabaseName("IX_StaffMembers_LicenseNumber");
        
        builder.Property(m => m.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();
    }
}