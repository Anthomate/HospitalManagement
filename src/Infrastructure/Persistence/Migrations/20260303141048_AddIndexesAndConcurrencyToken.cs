using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "StaffMembers",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Patients",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Departments",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Consultations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_FirstName_Trgm",
                table: "Patients",
                column: "FirstName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_LastName_Trgm",
                table: "Patients",
                column: "LastName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorId_ScheduledAt_Scheduled",
                table: "Consultations",
                columns: new[] { "DoctorId", "ScheduledAt" },
                filter: "\"Status\" = 'Scheduled'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Patients_FirstName_Trgm",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_LastName_Trgm",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_DoctorId_ScheduledAt_Scheduled",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Consultations");
        }
    }
}
