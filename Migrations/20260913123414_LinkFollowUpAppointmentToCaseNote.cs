using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class LinkFollowUpAppointmentToCaseNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CasenoteId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CasenoteId",
                table: "Appointments",
                column: "CasenoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_CaseNotes_CasenoteId",
                table: "Appointments",
                column: "CasenoteId",
                principalTable: "CaseNotes",
                principalColumn: "CasenoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_CaseNotes_CasenoteId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CasenoteId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CasenoteId",
                table: "Appointments");
        }
    }
}
