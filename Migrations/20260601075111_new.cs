using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VisitedPsychiatrist",
                table: "HealthInformations",
                newName: "PsychiatricConsultation");

            migrationBuilder.RenameColumn(
                name: "VictimOfAbuse",
                table: "HealthInformations",
                newName: "SuicideRiskHistory");

            migrationBuilder.RenameColumn(
                name: "SuicidalAttempts",
                table: "HealthInformations",
                newName: "FamilyMentalHealthHistory");

            migrationBuilder.RenameColumn(
                name: "MentallyChallengedRelative",
                table: "HealthInformations",
                newName: "DrugHistory");

            migrationBuilder.RenameColumn(
                name: "InvolvedWithDrugs",
                table: "HealthInformations",
                newName: "AbuseHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SuicideRiskHistory",
                table: "HealthInformations",
                newName: "VictimOfAbuse");

            migrationBuilder.RenameColumn(
                name: "PsychiatricConsultation",
                table: "HealthInformations",
                newName: "VisitedPsychiatrist");

            migrationBuilder.RenameColumn(
                name: "FamilyMentalHealthHistory",
                table: "HealthInformations",
                newName: "SuicidalAttempts");

            migrationBuilder.RenameColumn(
                name: "DrugHistory",
                table: "HealthInformations",
                newName: "MentallyChallengedRelative");

            migrationBuilder.RenameColumn(
                name: "AbuseHistory",
                table: "HealthInformations",
                newName: "InvolvedWithDrugs");
        }
    }
}
