using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invex_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryItemEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_InventoryFieldData_InventoryDataId_CustomFieldId",
                table: "InventoryFieldData",
                columns: new[] { "InventoryDataId", "CustomFieldId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryData_CreatedAt",
                table: "InventoryData",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryFieldData_InventoryDataId_CustomFieldId",
                table: "InventoryFieldData");

            migrationBuilder.DropIndex(
                name: "IX_InventoryData_CreatedAt",
                table: "InventoryData");
        }
    }
}
