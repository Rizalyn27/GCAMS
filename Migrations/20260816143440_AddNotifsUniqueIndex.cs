using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCAMS.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifsUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifs_RecipientUsername_Type_RelatedEntityType_RelatedEntityId",
                table: "Notifs",
                columns: new[] { "RecipientUsername", "Type", "RelatedEntityType", "RelatedEntityId" },
                unique: true,
                filter: "[RelatedEntityType] IS NOT NULL AND [RelatedEntityId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifs_RecipientUsername_Type_RelatedEntityType_RelatedEntityId",
                table: "Notifs");

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
