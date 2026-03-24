using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedCessPostingAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Hsns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CessPostingAc",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "CessPostingAc",
                table: "Hsns");
        }
    }
}
