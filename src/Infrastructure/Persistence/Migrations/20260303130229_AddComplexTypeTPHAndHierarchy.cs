using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplexTypeTPHAndHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Departments",
                table: "Doctors");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Doctors",
                table: "Consultations");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_MedicalDirector",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doctors",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "Doctors",
                newName: "StaffMembers");

            migrationBuilder.RenameIndex(
                name: "IX_Doctors_DepartmentId",
                table: "StaffMembers",
                newName: "IX_StaffMembers_DepartmentId");

            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCountry",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "Patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressZipCode",
                table: "Patients",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentDepartmentId",
                table: "Departments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specialty",
                table: "StaffMembers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "StaffMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "StaffMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Country",
                table: "StaffMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "StaffMembers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "StaffMembers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "StaffMembers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StaffMembers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "StaffMembers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "StaffMembers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffMembers",
                table: "StaffMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_Email",
                table: "StaffMembers",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_ParentDepartment",
                table: "Departments",
                column: "ParentDepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffMembers_Departments",
                table: "StaffMembers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Doctors",
                table: "Consultations",
                column: "DoctorId",
                principalTable: "StaffMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.AddForeignKey(
                name: "FK_Departments_MedicalDirector",
                table: "Departments",
                column: "MedicalDirectorId",
                principalTable: "StaffMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_ParentDepartment",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffMembers_Departments",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffMembers",
                table: "StaffMembers");

            migrationBuilder.DropIndex(
                name: "IX_StaffMembers_Email",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "AddressCity",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressCountry",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AddressZipCode",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ParentDepartmentId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Address_Country",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "StaffMembers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "StaffMembers");

            migrationBuilder.RenameTable(
                name: "StaffMembers",
                newName: "Doctors");

            migrationBuilder.RenameIndex(
                name: "IX_StaffMembers_DepartmentId",
                table: "Doctors",
                newName: "IX_Doctors_DepartmentId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Patients",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Specialty",
                table: "Doctors",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doctors",
                table: "Doctors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Departments",
                table: "Doctors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Doctors",
                table: "Consultations",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.AddForeignKey(
                name: "FK_Departments_MedicalDirector",
                table: "Departments",
                column: "MedicalDirectorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
