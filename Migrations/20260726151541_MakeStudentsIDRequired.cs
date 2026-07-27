using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class MakeStudentsIDRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes");

            migrationBuilder.AlterColumn<int>(
                name: "StudentsID",
                table: "CaseNotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes",
                column: "StudentsID",
                principalTable: "Students",
                principalColumn: "StudentsID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes");

            migrationBuilder.AlterColumn<int>(
                name: "StudentsID",
                table: "CaseNotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotes_Students_StudentsID",
                table: "CaseNotes",
                column: "StudentsID",
                principalTable: "Students",
                principalColumn: "StudentsID");
        }
    }
}
