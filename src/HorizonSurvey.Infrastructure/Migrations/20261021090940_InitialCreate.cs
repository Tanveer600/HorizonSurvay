using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HorizonSurvey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PooledStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PooledStocks", x => x.Id);
                    table.CheckConstraint("CK_PooledStock_Quantities", "[TotalQuantity] >= 0 AND [AvailableQuantity] >= 0 AND [AvailableQuantity] <= [TotalQuantity]");
                });

            migrationBuilder.CreateTable(
                name: "DroneApprovals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Holder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<long>(type: "bigint", nullable: false),
                    CheckoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DroneApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DroneApprovals_EquipmentAssets_EquipmentAssetId",
                        column: x => x.EquipmentAssetId,
                        principalTable: "EquipmentAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Checkouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Holder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpectedReturnAt = table.Column<long>(type: "bigint", nullable: false),
                    EquipmentAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PooledStockId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReturnedQuantity = table.Column<int>(type: "int", nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    CheckedOutAt = table.Column<long>(type: "bigint", nullable: false),
                    ReturnedAt = table.Column<long>(type: "bigint", nullable: true),
                    ApprovalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkouts", x => x.Id);
                    table.CheckConstraint("CK_Checkout_ReturnedQuantity", "[ReturnedQuantity] >= 0 AND [ReturnedQuantity] <= [Quantity]");
                    table.CheckConstraint("CK_Checkout_Target", "(([EquipmentAssetId] IS NOT NULL AND [PooledStockId] IS NULL AND [Quantity] = 1) OR ([EquipmentAssetId] IS NULL AND [PooledStockId] IS NOT NULL AND [Quantity] > 0))");
                    table.ForeignKey(
                        name: "FK_Checkouts_DroneApprovals_ApprovalId",
                        column: x => x.ApprovalId,
                        principalTable: "DroneApprovals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checkouts_EquipmentAssets_EquipmentAssetId",
                        column: x => x.EquipmentAssetId,
                        principalTable: "EquipmentAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Checkouts_PooledStocks_PooledStockId",
                        column: x => x.PooledStockId,
                        principalTable: "PooledStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CheckoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EquipmentAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PooledStockId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OccurredAt = table.Column<long>(type: "bigint", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Checkouts_CheckoutId",
                        column: x => x.CheckoutId,
                        principalTable: "Checkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_EquipmentAssets_EquipmentAssetId",
                        column: x => x.EquipmentAssetId,
                        principalTable: "EquipmentAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_PooledStocks_PooledStockId",
                        column: x => x.PooledStockId,
                        principalTable: "PooledStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_ApprovalId",
                table: "Checkouts",
                column: "ApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_EquipmentAssetId",
                table: "Checkouts",
                column: "EquipmentAssetId",
                unique: true,
                filter: "[EquipmentAssetId] IS NOT NULL AND [IsOpen] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Checkouts_PooledStockId",
                table: "Checkouts",
                column: "PooledStockId");

            migrationBuilder.CreateIndex(
                name: "IX_DroneApprovals_EquipmentAssetId",
                table: "DroneApprovals",
                column: "EquipmentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CheckoutId",
                table: "InventoryMovements",
                column: "CheckoutId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_EquipmentAssetId",
                table: "InventoryMovements",
                column: "EquipmentAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_OccurredAt",
                table: "InventoryMovements",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_PooledStockId",
                table: "InventoryMovements",
                column: "PooledStockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "Checkouts");

            migrationBuilder.DropTable(
                name: "DroneApprovals");

            migrationBuilder.DropTable(
                name: "PooledStocks");

            migrationBuilder.DropTable(
                name: "EquipmentAssets");
        }
    }
}
