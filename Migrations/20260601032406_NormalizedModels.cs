using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Ailments",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ElementaryHonors",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ElementarySchool",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ElementaryYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmergencyContactAddress",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmergencyContactAge",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmergencyContactOccupation",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPerson",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherAge",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherEducationalAttainment",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherOccupation",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "InvolvedWithDrugs",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Medication",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MentallyChallengedRelative",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MonthlyFamilyIncome",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherAge",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherEducationalAttainment",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherOccupation",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentsRelationshipStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SecondaryHonors",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SecondarySchool",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SecondaryYear",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SuicidalAttempts",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "VictimOfAbuse",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "VisitedPsychiatrist",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Students",
                newName: "StuID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Students",
                newName: "StudentsID");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Birthday",
                table: "Students",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "EducationalBackgrounds",
                columns: table => new
                {
                    EducationalBackgroundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: true),
                    ElementarySchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ElementaryYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ElementaryHonors = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SecondarySchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryHonors = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalBackgrounds", x => x.EducationalBackgroundID);
                    table.ForeignKey(
                        name: "FK_EducationalBackgrounds_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    EmergencyContactID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: true),
                    EmergencyContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactAge = table.Column<int>(type: "int", nullable: true),
                    EmergencyContactOccupation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.EmergencyContactID);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyBackgrounds",
                columns: table => new
                {
                    FamilyBackgroundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FatherAge = table.Column<int>(type: "int", nullable: true),
                    FatherEducationalAttainment = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FatherOccupation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FatherContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MotherAge = table.Column<int>(type: "int", nullable: true),
                    MotherEducationalAttainment = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MotherOccupation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MotherContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MonthlyFamilyIncome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentsRelationshipStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyBackgrounds", x => x.FamilyBackgroundID);
                    table.ForeignKey(
                        name: "FK_FamilyBackgrounds_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthInformations",
                columns: table => new
                {
                    HealthInformationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Ailments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Medication = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuicidalAttempts = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VictimOfAbuse = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InvolvedWithDrugs = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MentallyChallengedRelative = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VisitedPsychiatrist = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthInformations", x => x.HealthInformationID);
                    table.ForeignKey(
                        name: "FK_HealthInformations_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EducationalBackgrounds_StudentsID",
                table: "EducationalBackgrounds",
                column: "StudentsID",
                unique: true,
                filter: "[StudentsID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_StudentsID",
                table: "EmergencyContacts",
                column: "StudentsID",
                unique: true,
                filter: "[StudentsID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyBackgrounds_StudentsID",
                table: "FamilyBackgrounds",
                column: "StudentsID",
                unique: true,
                filter: "[StudentsID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HealthInformations_StudentsID",
                table: "HealthInformations",
                column: "StudentsID",
                unique: true,
                filter: "[StudentsID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EducationalBackgrounds");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "FamilyBackgrounds");

            migrationBuilder.DropTable(
                name: "HealthInformations");

            migrationBuilder.RenameColumn(
                name: "StuID",
                table: "Students",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "StudentsID",
                table: "Students",
                newName: "Id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Birthday",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "Students",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ailments",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodType",
                table: "Students",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ElementaryHonors",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ElementarySchool",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ElementaryYear",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactAddress",
                table: "Students",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyContactAge",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactOccupation",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPerson",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FatherAge",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherContactNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherEducationalAttainment",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherOccupation",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Height",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvolvedWithDrugs",
                table: "Students",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medication",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MentallyChallengedRelative",
                table: "Students",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MonthlyFamilyIncome",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotherAge",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherContactNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherEducationalAttainment",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherOccupation",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentsRelationshipStatus",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryHonors",
                table: "Students",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondarySchool",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryYear",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuicidalAttempts",
                table: "Students",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VictimOfAbuse",
                table: "Students",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitedPsychiatrist",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Weight",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
