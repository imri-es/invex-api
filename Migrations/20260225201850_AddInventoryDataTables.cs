using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace invex_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryCustomIds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ElementId = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "InventoryData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomID = table.Column<string>(type: "text", nullable: true),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryData_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ValueString = table.Column<string>(type: "text", nullable: true),
                    ValueNumeric = table.Column<decimal>(type: "numeric", nullable: true),
                    ValueBoolean = table.Column<bool>(type: "boolean", nullable: true),
                    IsDisplay = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryFields_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCustomIds_InventoryId",
                table: "InventoryCustomIds",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryData_InventoryId",
                table: "InventoryData",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryFields_InventoryId",
                table: "InventoryFields",
                column: "InventoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryCustomIds");

            migrationBuilder.DropTable(
                name: "InventoryData");

            migrationBuilder.DropTable(
                name: "InventoryFields");
        }
    }
}
