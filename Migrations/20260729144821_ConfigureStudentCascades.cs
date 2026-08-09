using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureStudentCascades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Students_StudentsID",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Students_StudentsID",
                table: "Appointments",
                column: "StudentsID",
                principalTable: "Students",
                principalColumn: "StudentsID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Students_StudentsID",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Students_StudentsID",
                table: "Appointments",
                column: "StudentsID",
                principalTable: "Students",
                principalColumn: "StudentsID");
        }
    }
}
