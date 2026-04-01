using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class addSalesAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Hsns_HsnId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_Accounts_AccountId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropIndex(
                name: "IX_Items_HsnId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "HsnId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "SalesInvoiceMains",
                newName: "SalesAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceMains_AccountId",
                table: "SalesInvoiceMains",
                newName: "IX_SalesInvoiceMains_SalesAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_HSNCodeId",
                table: "Items",
                column: "HSNCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Hsns_HSNCodeId",
                table: "Items",
                column: "HSNCodeId",
                principalTable: "Hsns",
                principalColumn: "HsnId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_Accounts_SalesAccountId",
                table: "SalesInvoiceMains",
                column: "SalesAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Hsns_HSNCodeId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_Accounts_SalesAccountId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropIndex(
                name: "IX_Items_HSNCodeId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "SalesAccountId",
                table: "SalesInvoiceMains",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_SalesInvoiceMains_SalesAccountId",
                table: "SalesInvoiceMains",
                newName: "IX_SalesInvoiceMains_AccountId");

            migrationBuilder.AddColumn<int>(
                name: "HsnId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_HsnId",
                table: "Items",
                column: "HsnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Hsns_HsnId",
                table: "Items",
                column: "HsnId",
                principalTable: "Hsns",
                principalColumn: "HsnId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_Accounts_AccountId",
                table: "SalesInvoiceMains",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }
    }
}
