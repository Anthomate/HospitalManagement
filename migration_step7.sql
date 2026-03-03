CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Departments" (
    "Id" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Location" character varying(200) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Departments" PRIMARY KEY ("Id")
);

CREATE TABLE "Patients" (
    "Id" uuid NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "BirthDate" date NOT NULL,
    "RecordNumber" character varying(20) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "Phone" character varying(20),
    "Address" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Patients" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX ix_departments_name ON "Departments" ("Name");

CREATE UNIQUE INDEX "IX_Patients_Email" ON "Patients" ("Email");

CREATE UNIQUE INDEX "IX_Patients_RecordNumber" ON "Patients" ("RecordNumber");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303093217_InitialCreate', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Departments" ADD "MedicalDirectorId" uuid;

CREATE TABLE "Doctors" (
    "Id" uuid NOT NULL,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "Specialty" character varying(150) NOT NULL,
    "LicenseNumber" character varying(50) NOT NULL,
    "DepartmentId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Doctors" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Doctors_Departments" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Departments_MedicalDirectorId" ON "Departments" ("MedicalDirectorId");

CREATE INDEX "IX_Doctors_DepartmentId" ON "Doctors" ("DepartmentId");

CREATE UNIQUE INDEX "IX_Doctors_LicenseNumber" ON "Doctors" ("LicenseNumber");

ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_MedicalDirector" FOREIGN KEY ("MedicalDirectorId") REFERENCES "Doctors" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303100300_AddDoctor', '10.0.3');

COMMIT;

START TRANSACTION;
CREATE TABLE "Consultations" (
    "Id" uuid NOT NULL,
    "PatientId" uuid NOT NULL,
    "DoctorId" uuid NOT NULL,
    "ScheduledAt" timestamp with time zone NOT NULL,
    "Status" character varying(20) NOT NULL,
    "Notes" character varying(2000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Consultations" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Consultations_Doctors" FOREIGN KEY ("DoctorId") REFERENCES "Doctors" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Consultations_Patients" FOREIGN KEY ("PatientId") REFERENCES "Patients" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Consultations_DoctorId" ON "Consultations" ("DoctorId");

CREATE UNIQUE INDEX "IX_Consultations_Patient_Doctor_ScheduledAt" ON "Consultations" ("PatientId", "DoctorId", "ScheduledAt");

CREATE INDEX "IX_Consultations_PatientId" ON "Consultations" ("PatientId");

CREATE INDEX "IX_Consultations_ScheduledAt" ON "Consultations" ("ScheduledAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303102935_AddConsultation', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Doctors" DROP CONSTRAINT "FK_Doctors_Departments";

ALTER TABLE "Consultations" DROP CONSTRAINT "FK_Consultations_Doctors";

ALTER TABLE "Departments" DROP CONSTRAINT "FK_Departments_MedicalDirector";

ALTER TABLE "Doctors" DROP CONSTRAINT "PK_Doctors";

ALTER TABLE "Patients" DROP COLUMN "Address";

ALTER TABLE "Doctors" RENAME TO "StaffMembers";

ALTER INDEX "IX_Doctors_DepartmentId" RENAME TO "IX_StaffMembers_DepartmentId";

ALTER TABLE "Patients" ADD "AddressCity" character varying(100);

ALTER TABLE "Patients" ADD "AddressCountry" character varying(100);

ALTER TABLE "Patients" ADD "AddressStreet" character varying(200);

ALTER TABLE "Patients" ADD "AddressZipCode" character varying(10);

ALTER TABLE "Departments" ADD "ParentDepartmentId" uuid;

ALTER TABLE "StaffMembers" ALTER COLUMN "Specialty" DROP NOT NULL;

ALTER TABLE "StaffMembers" ALTER COLUMN "LicenseNumber" DROP NOT NULL;

ALTER TABLE "StaffMembers" ADD "Address_City" character varying(100);

ALTER TABLE "StaffMembers" ADD "Address_Country" character varying(100);

ALTER TABLE "StaffMembers" ADD "Address_Street" character varying(200);

ALTER TABLE "StaffMembers" ADD "Address_ZipCode" character varying(10);

ALTER TABLE "StaffMembers" ADD "Discriminator" character varying(20) NOT NULL DEFAULT '';

ALTER TABLE "StaffMembers" ADD "Email" character varying(255) NOT NULL DEFAULT '';

ALTER TABLE "StaffMembers" ADD "Phone" character varying(20);

ALTER TABLE "StaffMembers" ADD "Role" character varying(100);

ALTER TABLE "StaffMembers" ADD CONSTRAINT "PK_StaffMembers" PRIMARY KEY ("Id");

CREATE INDEX "IX_Departments_ParentDepartmentId" ON "Departments" ("ParentDepartmentId");

CREATE UNIQUE INDEX "IX_StaffMembers_Email" ON "StaffMembers" ("Email");

ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_ParentDepartment" FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT;

ALTER TABLE "StaffMembers" ADD CONSTRAINT "FK_StaffMembers_Departments" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Consultations" ADD CONSTRAINT "FK_Consultations_Doctors" FOREIGN KEY ("DoctorId") REFERENCES "StaffMembers" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_MedicalDirector" FOREIGN KEY ("MedicalDirectorId") REFERENCES "StaffMembers" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303130229_AddComplexTypeTPHAndHierarchy', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "StaffMembers" RENAME COLUMN "Role" TO "Service";

ALTER INDEX "IX_Doctors_LicenseNumber" RENAME TO "IX_StaffMembers_LicenseNumber";

ALTER TABLE "StaffMembers" ADD "Function" character varying(100);

ALTER TABLE "StaffMembers" ADD "Grade" character varying(100);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303132512_AddTPHRolesAndHierarchy', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "StaffMembers" ADD "HireDate" date NOT NULL DEFAULT DATE '-infinity';

ALTER TABLE "StaffMembers" ADD "Salary" numeric(10,2) NOT NULL DEFAULT 0.0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303133102_AddStaffMemberHireDateAndSalary', '10.0.3');

COMMIT;

START TRANSACTION;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX "IX_Patients_FirstName_Trgm" ON "Patients" USING gin ("FirstName" gin_trgm_ops);

CREATE INDEX "IX_Patients_LastName_Trgm" ON "Patients" USING gin ("LastName" gin_trgm_ops);

CREATE INDEX "IX_Consultations_DoctorId_ScheduledAt_Scheduled" ON "Consultations" ("DoctorId", "ScheduledAt") WHERE "Status" = 'Scheduled';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303141048_AddIndexesAndConcurrencyToken', '10.0.3');

COMMIT;

