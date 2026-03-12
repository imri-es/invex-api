using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invex_api.Migrations
{
    /// <inheritdoc />
    public partial class SeparateInventoryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_OwnerId_Id",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_Id",
                table: "Inventories",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_Id",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId_Id",
                table: "Inventories",
                columns: new[] { "OwnerId", "Id" });
        }
    }
}
