using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class notifs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CounselorID",
                table: "CaseNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounselorID",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifs",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifs", x => x.NotificationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseNotes_CounselorID",
                table: "CaseNotes",
                column: "CounselorID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CounselorID",
                table: "Appointments",
                column: "CounselorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Counselors_CounselorID",
                table: "Appointments",
                column: "CounselorID",
                principalTable: "Counselors",
                principalColumn: "CounselorID");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseNotes_Counselors_CounselorID",
                table: "CaseNotes",
                column: "CounselorID",
                principalTable: "Counselors",
                principalColumn: "CounselorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Counselors_CounselorID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseNotes_Counselors_CounselorID",
                table: "CaseNotes");

            migrationBuilder.DropTable(
                name: "Notifs");

            migrationBuilder.DropIndex(
                name: "IX_CaseNotes_CounselorID",
                table: "CaseNotes");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CounselorID",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CounselorID",
                table: "CaseNotes");

            migrationBuilder.DropColumn(
                name: "CounselorID",
                table: "Appointments");
        }
    }
}
