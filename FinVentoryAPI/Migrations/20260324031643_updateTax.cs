using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hsns_Accounts_AccountId",
                table: "Hsns");

            migrationBuilder.DropIndex(
                name: "IX_Hsns_AccountId",
                table: "Hsns");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Hsns");

            migrationBuilder.AddColumn<int>(
                name: "CGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CGSTPostingAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IGSTPostingAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SGSTPostingAccountAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SGSTPostingAccountId",
                table: "Taxes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CGSTPostingAccountAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_CGSTPostingAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IGSTPostingAccountAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IGSTPostingAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_SGSTPostingAccountAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_SGSTPostingAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Hsns_CessPostingAc",
                table: "Hsns",
                column: "CessPostingAc");

            migrationBuilder.AddForeignKey(
                name: "FK_Hsns_Accounts_CessPostingAc",
                table: "Hsns",
                column: "CessPostingAc",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountId",
                table: "Taxes",
                column: "CGSTPostingAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountId",
                table: "Taxes",
                column: "IGSTPostingAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountId",
                table: "Taxes",
                column: "SGSTPostingAccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hsns_Accounts_CessPostingAc",
                table: "Hsns");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_CGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_IGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropForeignKey(
                name: "FK_Taxes_Accounts_SGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_CGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_IGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_SGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_SGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Hsns_CessPostingAc",
                table: "Hsns");

            migrationBuilder.DropColumn(
                name: "CGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "CGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "IGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "IGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "SGSTPostingAccountAccountId",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "SGSTPostingAccountId",
                table: "Taxes");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Hsns",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hsns_AccountId",
                table: "Hsns",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hsns_Accounts_AccountId",
                table: "Hsns",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }
    }
}
