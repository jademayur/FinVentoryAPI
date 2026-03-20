using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ItemCategory = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: true),
                    HSNCodeId = table.Column<int>(type: "int", nullable: false),
                    BaseUnitId = table.Column<int>(type: "int", nullable: false),
                    AlternateUnitId = table.Column<int>(type: "int", nullable: true),
                    ConversionFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AllowNagativeStock = table.Column<bool>(type: "bit", nullable: false),
                    ItemManageBy = table.Column<int>(type: "int", nullable: false),
                    CostingMethod = table.Column<int>(type: "int", nullable: false),
                    InventoryAccountId = table.Column<int>(type: "int", nullable: true),
                    COGSAccountId = table.Column<int>(type: "int", nullable: true),
                    SalesAccountId = table.Column<int>(type: "int", nullable: true),
                    PurchaseAccountId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_Items_Accounts_COGSAccountId",
                        column: x => x.COGSAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_Items_Accounts_InventoryAccountId",
                        column: x => x.InventoryAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_Items_Accounts_PurchaseAccountId",
                        column: x => x.PurchaseAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_Items_Accounts_SalesAccountId",
                        column: x => x.SalesAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_Items_ItemGroups_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalTable: "ItemGroups",
                        principalColumn: "ItemGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemsPrices",
                columns: table => new
                {
                    ItemPriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsPrices", x => x.ItemPriceId);
                    table.ForeignKey(
                        name: "FK_ItemsPrices_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_COGSAccountId",
                table: "Items",
                column: "COGSAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InventoryAccountId",
                table: "Items",
                column: "InventoryAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemGroupId",
                table: "Items",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PurchaseAccountId",
                table: "Items",
                column: "PurchaseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SalesAccountId",
                table: "Items",
                column: "SalesAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemsPrices_ItemId",
                table: "ItemsPrices",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemsPrices");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
