using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class authfuncationality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompanies_Companies_CompanyId",
                table: "UserCompanies");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompanies_Roles_RoleId",
                table: "UserCompanies");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompanies_Users_UserId",
                table: "UserCompanies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCompanies",
                table: "UserCompanies");

            migrationBuilder.RenameTable(
                name: "UserCompanies",
                newName: "UserCompany");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompanies_UserId",
                table: "UserCompany",
                newName: "IX_UserCompany_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompanies_RoleId",
                table: "UserCompany",
                newName: "IX_UserCompany_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompanies_CompanyId",
                table: "UserCompany",
                newName: "IX_UserCompany_CompanyId");

            migrationBuilder.AddColumn<int>(
                name: "FinancialYearId",
                table: "UserCompany",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserCompany",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCompany",
                table: "UserCompany",
                column: "UserCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_FinancialYearId",
                table: "UserCompany",
                column: "FinancialYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_Companies_CompanyId",
                table: "UserCompany",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany",
                column: "FinancialYearId",
                principalTable: "FinancialYears",
                principalColumn: "FinancialYearId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_Roles_RoleId",
                table: "UserCompany",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompany_Users_UserId",
                table: "UserCompany",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_Companies_CompanyId",
                table: "UserCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_FinancialYears_FinancialYearId",
                table: "UserCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_Roles_RoleId",
                table: "UserCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompany_Users_UserId",
                table: "UserCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCompany",
                table: "UserCompany");

            migrationBuilder.DropIndex(
                name: "IX_UserCompany_FinancialYearId",
                table: "UserCompany");

            migrationBuilder.DropColumn(
                name: "FinancialYearId",
                table: "UserCompany");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserCompany");

            migrationBuilder.RenameTable(
                name: "UserCompany",
                newName: "UserCompanies");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompany_UserId",
                table: "UserCompanies",
                newName: "IX_UserCompanies_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompany_RoleId",
                table: "UserCompanies",
                newName: "IX_UserCompanies_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompany_CompanyId",
                table: "UserCompanies",
                newName: "IX_UserCompanies_CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCompanies",
                table: "UserCompanies",
                column: "UserCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompanies_Companies_CompanyId",
                table: "UserCompanies",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompanies_Roles_RoleId",
                table: "UserCompanies",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompanies_Users_UserId",
                table: "UserCompanies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
