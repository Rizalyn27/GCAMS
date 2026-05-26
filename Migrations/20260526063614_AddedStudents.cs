using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class AddedStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    School = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    BirthOrder = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StayingWith = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    ParentsRelationshipStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactAge = table.Column<int>(type: "int", nullable: true),
                    EmergencyContactOccupation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ElementarySchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ElementaryYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ElementaryHonors = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SecondarySchool = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SecondaryYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryHonors = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
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
                    VisitedPsychiatristReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
