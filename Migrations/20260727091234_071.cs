using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class _071 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnecRecs",
                columns: table => new
                {
                    AnecRecsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentsID = table.Column<int>(type: "int", nullable: false),
                    StuName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfObserv = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ObservedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeopleInvolved = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SceneMood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentBehavior = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObserverRecs = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnecRecs", x => x.AnecRecsId);
                    table.ForeignKey(
                        name: "FK_AnecRecs_Students_StudentsID",
                        column: x => x.StudentsID,
                        principalTable: "Students",
                        principalColumn: "StudentsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnecRecs_StudentsID",
                table: "AnecRecs",
                column: "StudentsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnecRecs");
        }
    }
}
