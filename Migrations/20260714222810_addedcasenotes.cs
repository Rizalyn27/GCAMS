using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class addedcasenotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppointmentDate",
                table: "CaseNotes",
                newName: "SessionDate");

            migrationBuilder.AlterColumn<string>(
                name: "Homework",
                table: "CaseNotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionDate",
                table: "CaseNotes",
                newName: "AppointmentDate");

            migrationBuilder.AlterColumn<string>(
                name: "Homework",
                table: "CaseNotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
