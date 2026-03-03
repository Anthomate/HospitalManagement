using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTPHRolesAndHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "StaffMembers",
                newName: "Service");

            migrationBuilder.RenameIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "StaffMembers",
                newName: "IX_StaffMembers_LicenseNumber");

            migrationBuilder.AddColumn<string>(
                name: "Function",
                table: "StaffMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "StaffMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Function",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "StaffMembers");

            migrationBuilder.RenameColumn(
                name: "Service",
                table: "StaffMembers",
                newName: "Role");

            migrationBuilder.RenameIndex(
                name: "IX_StaffMembers_LicenseNumber",
                table: "StaffMembers",
                newName: "IX_Doctors_LicenseNumber");
        }
    }
}
