using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class RemovedGradeSpecificAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "Announcements");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Announcements");

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                table: "Announcements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "Announcements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
