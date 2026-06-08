using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class modelchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Students");

            // Fix EmploymentStatus: can't directly cast 'Active'/'Inactive' to bit
            migrationBuilder.AddColumn<bool>(
                name: "EmploymentStatus_New",
                table: "Counselors",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                "UPDATE Counselors SET EmploymentStatus_New = CASE WHEN EmploymentStatus = 'Active' THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Counselors");

            migrationBuilder.RenameColumn(
                name: "EmploymentStatus_New",
                table: "Counselors",
                newName: "EmploymentStatus");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "Counselors",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "EmploymentStatus",
                table: "Counselors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ContactNumber",
                table: "Counselors",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(11)",
                oldMaxLength: 11);
        }
    }
}
