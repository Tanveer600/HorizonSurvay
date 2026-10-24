using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorizonSurvey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManageEquipmentCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategories", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "EquipmentAssets",
                type: "uniqueidentifier",
                nullable: true);

            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    INSERT INTO EquipmentCategories (Id, Name)
                    SELECT lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(2)) || '-' || hex(randomblob(6))), Category
                    FROM EquipmentAssets
                    WHERE Category IS NOT NULL
                    GROUP BY Category;
                    """);
            }
            else
            {
                migrationBuilder.Sql("""
                    INSERT INTO EquipmentCategories (Id, Name)
                    SELECT NEWID(), Category
                    FROM EquipmentAssets
                    WHERE Category IS NOT NULL
                    GROUP BY Category;
                    """);
            }

            migrationBuilder.Sql("""
                UPDATE EquipmentAssets
                SET CategoryId = (
                    SELECT Id
                    FROM EquipmentCategories
                    WHERE EquipmentCategories.Name = EquipmentAssets.Category
                );
                """);

            migrationBuilder.DropColumn(
                name: "Category",
                table: "EquipmentAssets");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAssets_CategoryId",
                table: "EquipmentAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentCategories_Name",
                table: "EquipmentCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentAssets_EquipmentCategories_CategoryId",
                table: "EquipmentAssets",
                column: "CategoryId",
                principalTable: "EquipmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentAssets_EquipmentCategories_CategoryId",
                table: "EquipmentAssets");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentAssets_CategoryId",
                table: "EquipmentAssets");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "EquipmentAssets",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Rover");

            migrationBuilder.Sql("""
                UPDATE EquipmentAssets
                SET Category = COALESCE((
                    SELECT Name
                    FROM EquipmentCategories
                    WHERE EquipmentCategories.Id = EquipmentAssets.CategoryId
                ), 'Rover');
                """);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "EquipmentAssets");

            migrationBuilder.DropTable(
                name: "EquipmentCategories");
        }
    }
}
