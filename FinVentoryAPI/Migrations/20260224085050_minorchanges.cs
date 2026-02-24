using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class minorchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany");

            migrationBuilder.AlterColumn<int>(
                name: "FinancialYearId",
                table: "UserCompany",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany",
                column: "FinancialYearId",
                principalTable: "FinancialYears",
                principalColumn: "FinancialYearId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany");

            migrationBuilder.AlterColumn<int>(
                name: "FinancialYearId",
                table: "UserCompany",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany",
                column: "FinancialYearId",
                principalTable: "FinancialYears",
                principalColumn: "FinancialYearId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
