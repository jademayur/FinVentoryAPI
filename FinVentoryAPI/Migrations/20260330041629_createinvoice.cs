using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class createinvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesInvoiceMains",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceMains", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceMains_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddisDiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddisDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetails_SalesInvoiceMains_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "SalesInvoiceMains",
                        principalColumn: "InvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    SGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_Accounts_CGSTPostingAccountId",
                        column: x => x.CGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_Accounts_CessPostingAccountId",
                        column: x => x.CessPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_Accounts_IGSTPostingAccountId",
                        column: x => x.IGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_Accounts_SGSTPostingAccountId",
                        column: x => x.SGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_SalesInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesInvoiceDetails",
                        principalColumn: "DetailId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_SalesInvoiceMains_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "SalesInvoiceMains",
                        principalColumn: "InvoiceId");
                    table.ForeignKey(
                        name: "FK_SalesInvoiceTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetails_HsnId",
                table: "SalesInvoiceDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetails_InvoiceId",
                table: "SalesInvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetails_ItemId",
                table: "SalesInvoiceDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_AccountId",
                table: "SalesInvoiceMains",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_BusinessPartnerId",
                table: "SalesInvoiceMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_LocationId",
                table: "SalesInvoiceMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_CessPostingAccountId",
                table: "SalesInvoiceTaxDetails",
                column: "CessPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_CGSTPostingAccountId",
                table: "SalesInvoiceTaxDetails",
                column: "CGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_DetailId",
                table: "SalesInvoiceTaxDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_IGSTPostingAccountId",
                table: "SalesInvoiceTaxDetails",
                column: "IGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_InvoiceId",
                table: "SalesInvoiceTaxDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_SGSTPostingAccountId",
                table: "SalesInvoiceTaxDetails",
                column: "SGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceTaxDetails_TaxId",
                table: "SalesInvoiceTaxDetails",
                column: "TaxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesInvoiceTaxDetails");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDetails");

            migrationBuilder.DropTable(
                name: "SalesInvoiceMains");
        }
    }
}
