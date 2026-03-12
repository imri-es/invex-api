using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invex_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryAccesses_UserId",
                table: "InventoryAccesses");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId_InventoryId",
                table: "InventoryAccesses",
                columns: new[] { "UserId", "InventoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId_Id",
                table: "Inventories",
                columns: new[] { "OwnerId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryAccesses_UserId_InventoryId",
                table: "InventoryAccesses");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_OwnerId_Id",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId",
                table: "InventoryAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_OwnerId",
                table: "Inventories",
                column: "OwnerId");
        }
    }
}
