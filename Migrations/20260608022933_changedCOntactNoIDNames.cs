using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class changedCOntactNoIDNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StudentContactNumbers",
                newName: "StudentContactNumberID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "FamilyContactNumbers",
                newName: "FamilyContactNumberID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmergencyContactNumbers",
                newName: "EmergencyContactNumberID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentContactNumberID",
                table: "StudentContactNumbers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "FamilyContactNumberID",
                table: "FamilyContactNumbers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EmergencyContactNumberID",
                table: "EmergencyContactNumbers",
                newName: "Id");
        }
    }
}
