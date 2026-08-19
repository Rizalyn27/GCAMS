using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
