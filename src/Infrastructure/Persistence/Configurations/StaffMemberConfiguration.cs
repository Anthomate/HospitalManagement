using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.ToTable("StaffMembers");
        
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();
        
        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<Doctor>("Doctor")
            .HasValue<Nurse>("Nurse")
            .HasValue<AdminStaff>("AdminStaff");

        builder.Property<string>("Discriminator")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(s => s.FirstName)
            .HasColumnName("FirstName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.LastName)
            .HasColumnName("LastName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Email)
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.Phone)
            .HasColumnName("Phone")
            .HasMaxLength(20);

        builder.Property(s => s.DepartmentId)
            .HasColumnName("DepartmentId")
            .IsRequired();
        
        builder.Property(s => s.HireDate)
            .HasColumnName("HireDate")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(s => s.Salary)
            .HasColumnName("Salary")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired();
        
        builder.Property(s => s.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();
        
        builder.ComplexProperty(s => s.Address, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(200)
                .IsRequired(false);

            address.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100)
                .IsRequired(false);

            address.Property(a => a.ZipCode)
                .HasColumnName("Address_ZipCode")
                .HasMaxLength(10)
                .IsRequired(false);

            address.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .HasMaxLength(100)
                .IsRequired(false);
            
            builder.HasOne(s => s.Department)
                .WithMany(d => d.StaffMembers)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StaffMembers_Departments");

            builder.HasIndex(s => s.DepartmentId)
                .HasDatabaseName("IX_StaffMembers_DepartmentId");

            builder.HasIndex(s => s.Email)
                .IsUnique()
                .HasDatabaseName("IX_StaffMembers_Email");
        });
    }
}