using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class change_couns_mdl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateHired",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "EducationalAttainment",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Counselors");

            migrationBuilder.RenameColumn(
                name: "YearsOfExperience",
                table: "Counselors",
                newName: "NumberOfChildren");

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "Counselors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "College",
                table: "Counselors",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CollegeCourse",
                table: "Counselors",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "Counselors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PRCLicense",
                table: "Counselors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PRCLicense2",
                table: "Counselors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostGraduateCourse",
                table: "Counselors",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostGraduateStudies",
                table: "Counselors",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkExperience",
                table: "Counselors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkSchool",
                table: "Counselors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CaseNotes",
                columns: table => new
                {
                    CasenoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionNo = table.Column<int>(type: "int", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionTopics = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionRelevance = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GoalPlan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Interventions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CounselProgess = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BehaviorStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Homework = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StrengthsChallenges = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpecificGoal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OverallGoal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseNotes", x => x.CasenoteId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "College",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "CollegeCourse",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "PRCLicense",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "PRCLicense2",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "PostGraduateCourse",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "PostGraduateStudies",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "WorkExperience",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "WorkSchool",
                table: "Counselors");

            migrationBuilder.RenameColumn(
                name: "NumberOfChildren",
                table: "Counselors",
                newName: "YearsOfExperience");

            migrationBuilder.AlterColumn<bool>(
                name: "EmploymentStatus",
                table: "Counselors",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateHired",
                table: "Counselors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EducationalAttainment",
                table: "Counselors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Counselors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
