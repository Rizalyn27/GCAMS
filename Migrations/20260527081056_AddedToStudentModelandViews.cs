using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class AddedToStudentModelandViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FName",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "MName",
                table: "Students",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "LName",
                table: "Students",
                newName: "StuName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Students",
                newName: "MName");

            migrationBuilder.RenameColumn(
                name: "StuName",
                table: "Students",
                newName: "LName");

            migrationBuilder.AddColumn<string>(
                name: "FName",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
