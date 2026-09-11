using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentsIDToCaseNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentsID",
                table: "CaseNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_StudentsID",
                table: "CaseNotes",
                column: "StudentsID");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes",
                column: "StudentsID",
                principalTable: "Students",
                principalColumn: "StudentsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes");

            migrationBuilder.DropIndex(
                name: "IX_CaseNotes_StudentsID",
                table: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "StudentsID",
                table: "CaseNotes");
        }
    }
}
