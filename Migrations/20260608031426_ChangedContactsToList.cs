using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class ChangedContactsToList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Counselors");

            migrationBuilder.CreateTable(
                name: "CounselorContactNumbers",
                columns: table => new
                {
                    CounselorContactNumberID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CounselorID = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounselorContactNumbers", x => x.CounselorContactNumberID);
                    table.ForeignKey(
                        name: "FK_CounselorContactNumbers_Counselors_CounselorID",
                        column: x => x.CounselorID,
                        principalTable: "Counselors",
                        principalColumn: "CounselorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounselorContactNumbers_CounselorID",
                table: "CounselorContactNumbers",
                column: "CounselorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CounselorContactNumbers");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Counselors",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");
        }
    }
}
