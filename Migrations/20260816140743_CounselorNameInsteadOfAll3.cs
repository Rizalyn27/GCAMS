using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class CounselorNameInsteadOfAll3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Counselors");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Counselors",
                newName: "CounselorName");

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpDate",
                table: "CaseNotes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "CaseNotes");

            migrationBuilder.RenameColumn(
                name: "CounselorName",
                table: "Counselors",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Counselors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Counselors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
