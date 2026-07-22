using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class MultivaluedStudentContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherContactNumber",
                table: "FamilyBackgrounds");

            migrationBuilder.DropColumn(
                name: "MotherContactNumber",
                table: "FamilyBackgrounds");

            migrationBuilder.DropColumn(
                name: "EmergencyContactNumber",
                table: "EmergencyContacts");

            migrationBuilder.CreateTable(
                name: "EmergencyContactNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmergencyContactID = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContactNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContactNumbers_EmergencyContacts_EmergencyContactID",
                        column: x => x.EmergencyContactID,
                        principalTable: "EmergencyContacts",
                        principalColumn: "EmergencyContactID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyContactNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyBackgroundID = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyContactNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyContactNumbers_FamilyBackgrounds_FamilyBackgroundID",
                        column: x => x.FamilyBackgroundID,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "FamilyBackgroundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentContactNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentContactNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentContactNumbers_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContactNumbers_EmergencyContactID",
                table: "EmergencyContactNumbers",
                column: "EmergencyContactID");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyContactNumbers_FamilyBackgroundID",
                table: "FamilyContactNumbers",
                column: "FamilyBackgroundID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentContactNumbers_StudentsID",
                table: "StudentContactNumbers",
                column: "StudentsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmergencyContactNumbers");

            migrationBuilder.DropTable(
                name: "FamilyContactNumbers");

            migrationBuilder.DropTable(
                name: "StudentContactNumbers");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherContactNumber",
                table: "FamilyBackgrounds",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherContactNumber",
                table: "FamilyBackgrounds",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactNumber",
                table: "EmergencyContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
