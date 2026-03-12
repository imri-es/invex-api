using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invex_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFieldData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryCustomIds");

            migrationBuilder.DropColumn(
                name: "ValueBoolean",
                table: "InventoryFields");

            migrationBuilder.DropColumn(
                name: "ValueNumeric",
                table: "InventoryFields");

            migrationBuilder.DropColumn(
                name: "ValueString",
                table: "InventoryFields");

            migrationBuilder.AddColumn<string>(
                name: "CustomIdMask",
                table: "Inventories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "InventoryFieldData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValueString = table.Column<string>(type: "text", nullable: true),
                    ValueNumeric = table.Column<decimal>(type: "numeric", nullable: true),
                    ValueBoolean = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryFieldData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryFieldData_InventoryData_InventoryDataId",
                        column: x => x.InventoryDataId,
                        principalTable: "InventoryData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryFieldData_InventoryFields_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "InventoryFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryFieldData_CustomFieldId",
                table: "InventoryFieldData",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryFieldData_InventoryDataId",
                table: "InventoryFieldData",
                column: "InventoryDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryFieldData");

            migrationBuilder.DropColumn(
                name: "CustomIdMask",
                table: "Inventories");

            migrationBuilder.AddColumn<bool>(
                name: "ValueBoolean",
                table: "InventoryFields",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueNumeric",
                table: "InventoryFields",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueString",
                table: "InventoryFields",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryCustomIds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ElementId = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCustomIds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryCustomIds_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustomIds_InventoryId",
                table: "InventoryCustomIds",
                column: "InventoryId");
        }
    }
}
