using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateTaxaccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_SGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "SGSTPostingAccountAccountId",
                table: "Taxes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CGSTPostingAccountAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IGSTPostingAccountAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_SGSTPostingAccountAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }
    }
}
