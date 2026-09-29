using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinVentoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Users",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Users",
                newName: "ModifiedBy");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BillAddressId",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillStateCode",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactPersonId",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LrDate",
                table: "SalesInvoiceMains",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LrNo",
                table: "SalesInvoiceMains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesPersonId",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesStateCode",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShipAddressId",
                table: "SalesInvoiceMains",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportName",
                table: "SalesInvoiceMains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleNo",
                table: "SalesInvoiceMains",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryDetailId",
                table: "SalesInvoiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryId",
                table: "SalesInvoiceDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderLevel",
                table: "Items",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "State",
                table: "Companies",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "Companies",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "State",
                table: "BusinessPartnerAddresses",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AccountLedgerPostings",
                columns: table => new
                {
                    PostingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountLedgerPostings", x => x.PostingId);
                    table.ForeignKey(
                        name: "FK_AccountLedgerPostings_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountLedgerPostings_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalLevels",
                columns: table => new
                {
                    ApprovalLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLevels", x => x.ApprovalLevelId);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalLogs",
                columns: table => new
                {
                    ApprovalLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    LevelNumber = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalLogs", x => x.ApprovalLogId);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EntityNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    BankId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterial",
                columns: table => new
                {
                    BomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    BomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OutputQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    BaseUnitId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterial", x => x.BomId);
                    table.ForeignKey(
                        name: "FK_BillOfMaterial_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashBankEntries",
                columns: table => new
                {
                    CashBankEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    EntryNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    HeadAccountId = table.Column<int>(type: "int", nullable: false),
                    AccountDrCr = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CashBankEntries", x => x.CashBankEntryId);
                    table.ForeignKey(
                        name: "FK_CashBankEntries_Accounts_HeadAccountId",
                        column: x => x.HeadAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ConfigKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyConfigs", x => x.ConfigId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentCopyLogs",
                columns: table => new
                {
                    CopyLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    SourceDetailId = table.Column<int>(type: "int", nullable: true),
                    SourceQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    TargetDetailId = table.Column<int>(type: "int", nullable: true),
                    CopiedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCopyLogs", x => x.CopyLogId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSeries",
                columns: table => new
                {
                    SeriesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: true),
                    ModuleId = table.Column<int>(type: "int", nullable: true),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SeriesCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SeriesName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Format = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentLength = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartFromNumber = table.Column<int>(type: "int", nullable: false),
                    NextNumber = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSeries", x => x.SeriesId);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                });

            migrationBuilder.CreateTable(
                name: "GRNMains",
                columns: table => new
                {
                    GRNId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierInvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    PurchaseStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRNMains", x => x.GRNId);
                    table.ForeignKey(
                        name: "FK_GRNMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_GRNMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_GRNMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId");
                    table.ForeignKey(
                        name: "FK_GRNMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomingPaymentMains",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    PaymentNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    DepositAccountId = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Cash"),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OnAccountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingPaymentMains", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_IncomingPaymentMains_Accounts_DepositAccountId",
                        column: x => x.DepositAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IncomingPaymentMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UsedQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AvailableQty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_ItemBatches_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemSerials",
                columns: table => new
                {
                    SerialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSerials", x => x.SerialId);
                    table.ForeignKey(
                        name: "FK_ItemSerials_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkIssueMains",
                columns: table => new
                {
                    JobWorkIssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    IssueNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkIssueMains", x => x.JobWorkIssueId);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueMains_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkReceiptMains",
                columns: table => new
                {
                    JobWorkReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    ReceiptNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkReceiptMains", x => x.JobWorkReceiptId);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptMains_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    JournalEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    EntryNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.JournalEntryId);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutgoingPaymentMains",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    PaymentNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    PaymentAccountId = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OnAccountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingPaymentMains", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_OutgoingPaymentMains_Accounts_PaymentAccountId",
                        column: x => x.PaymentAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutgoingPaymentMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceMains",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierInvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    PurchaseAccountId = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    TransportName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LrNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LrDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceMains", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_Accounts_PurchaseAccountId",
                        column: x => x.PurchaseAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderMains",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    PurchaseStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderMains", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesPersons",
                columns: table => new
                {
                    SalesPersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    SalesPersonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SalesPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CommissionPct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPersons", x => x.SalesPersonId);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnMains",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalInvoiceId = table.Column<int>(type: "int", nullable: true),
                    OriginalInvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SalesAccountId = table.Column<int>(type: "int", nullable: false),
                    SalesStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsExport = table.Column<bool>(type: "bit", nullable: true),
                    IsReverseCharge = table.Column<bool>(type: "bit", nullable: true),
                    IsNonGST = table.Column<bool>(type: "bit", nullable: true),
                    PortCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingBillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingBillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnMains", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_Accounts_SalesAccountId",
                        column: x => x.SalesAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnMains_SalesInvoiceMains_OriginalInvoiceId",
                        column: x => x.OriginalInvoiceId,
                        principalTable: "SalesInvoiceMains",
                        principalColumn: "InvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentMains",
                columns: table => new
                {
                    AdjustmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentMains", x => x.AdjustmentId);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentMains_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLedgers",
                columns: table => new
                {
                    LedgerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VoucherType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedgers", x => x.LedgerId);
                    table.ForeignKey(
                        name: "FK_StockLedgers_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId");
                    table.ForeignKey(
                        name: "FK_StockLedgers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockLedgers_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId");
                });

            migrationBuilder.CreateTable(
                name: "StockTransferMains",
                columns: table => new
                {
                    TransferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    TransferNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferMains", x => x.TransferId);
                    table.ForeignKey(
                        name: "FK_StockTransferMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferMains_Warehouses_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferMains_Warehouses_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BomLines",
                columns: table => new
                {
                    BomLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BomId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ConversionFactor = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    WastagePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomLines", x => x.BomLineId);
                    table.ForeignKey(
                        name: "FK_BomLines_BillOfMaterial_BomId",
                        column: x => x.BomId,
                        principalTable: "BillOfMaterial",
                        principalColumn: "BomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BomLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrders",
                columns: table => new
                {
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    BomId = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ActualCompletionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrders", x => x.ProductionOrderId);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_BillOfMaterial_BomId",
                        column: x => x.BomId,
                        principalTable: "BillOfMaterial",
                        principalColumn: "BomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrders_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashBankEntryLines",
                columns: table => new
                {
                    CashBankEntryLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashBankEntryId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    DrCr = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashBankEntryLines", x => x.CashBankEntryLineId);
                    table.ForeignKey(
                        name: "FK_CashBankEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashBankEntryLines_CashBankEntries_CashBankEntryId",
                        column: x => x.CashBankEntryId,
                        principalTable: "CashBankEntries",
                        principalColumn: "CashBankEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentSeriesMappings",
                columns: table => new
                {
                    MappingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    SeriesId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSeriesMappings", x => x.MappingId);
                    table.ForeignKey(
                        name: "FK_DocumentSeriesMappings_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentSeriesMappings_DocumentSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "DocumentSeries",
                        principalColumn: "SeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomingPaymentAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingPaymentAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_IncomingPaymentAllocations_IncomingPaymentMains_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "IncomingPaymentMains",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomingPaymentAllocations_SalesInvoiceMains_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "SalesInvoiceMains",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetailBatches_ItemBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetailBatches_SalesInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesInvoiceDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    SerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetailSerials_ItemSerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesInvoiceDetailSerials_SalesInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesInvoiceDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkIssueDetails",
                columns: table => new
                {
                    JobWorkIssueDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkIssueId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkIssueDetails", x => x.JobWorkIssueDetailId);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetails_JobWorkIssueMains_JobWorkIssueId",
                        column: x => x.JobWorkIssueId,
                        principalTable: "JobWorkIssueMains",
                        principalColumn: "JobWorkIssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkReceiptDetails",
                columns: table => new
                {
                    JobWorkReceiptDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkReceiptId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkReceiptDetails", x => x.JobWorkReceiptDetailId);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetails_JobWorkReceiptMains_JobWorkReceiptId",
                        column: x => x.JobWorkReceiptId,
                        principalTable: "JobWorkReceiptMains",
                        principalColumn: "JobWorkReceiptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    LineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutgoingPaymentAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    BillId = table.Column<int>(type: "int", nullable: false),
                    AmountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingPaymentAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_OutgoingPaymentAllocations_OutgoingPaymentMains_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "OutgoingPaymentMains",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OutgoingPaymentAllocations_PurchaseInvoiceMains_BillId",
                        column: x => x.BillId,
                        principalTable: "PurchaseInvoiceMains",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnMains",
                columns: table => new
                {
                    ReturnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginalInvoiceId = table.Column<int>(type: "int", nullable: true),
                    OriginalInvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NoteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    PurchaseAccountId = table.Column<int>(type: "int", nullable: false),
                    PurchaseStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnMains", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_Accounts_PurchaseAccountId",
                        column: x => x.PurchaseAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnMains_PurchaseInvoiceMains_OriginalInvoiceId",
                        column: x => x.OriginalInvoiceId,
                        principalTable: "PurchaseInvoiceMains",
                        principalColumn: "InvoiceId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PriceType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AddisDiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddisDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_PurchaseOrderMains_OrderId",
                        column: x => x.OrderId,
                        principalTable: "PurchaseOrderMains",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodsDeliveryMains",
                columns: table => new
                {
                    DeliveryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    DeliveryNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    SalesPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    SalesStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsDeliveryMains", x => x.DeliveryId);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryMains_SalesPersons_SalesPersonId",
                        column: x => x.SalesPersonId,
                        principalTable: "SalesPersons",
                        principalColumn: "SalesPersonId");
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationMains",
                columns: table => new
                {
                    QuotationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    QuotationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidUntilDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentQuotationId = table.Column<int>(type: "int", nullable: true),
                    RevisionNo = table.Column<int>(type: "int", nullable: false),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SalesStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    SalesPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationMains", x => x.QuotationId);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_SalesPersons_SalesPersonId",
                        column: x => x.SalesPersonId,
                        principalTable: "SalesPersons",
                        principalColumn: "SalesPersonId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesQuotationMains_SalesQuotationMains_ParentQuotationId",
                        column: x => x.ParentQuotationId,
                        principalTable: "SalesQuotationMains",
                        principalColumn: "QuotationId");
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SalesReturnDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetails_SalesReturnMains_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "SalesReturnMains",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetails",
                columns: table => new
                {
                    AdjustmentDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetails", x => x.AdjustmentDetailId);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetails_StockAdjustmentMains_AdjustmentId",
                        column: x => x.AdjustmentId,
                        principalTable: "StockAdjustmentMains",
                        principalColumn: "AdjustmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferDetails",
                columns: table => new
                {
                    TransferDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferDetails", x => x.TransferDetailId);
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferDetails_StockTransferMains_TransferId",
                        column: x => x.TransferId,
                        principalTable: "StockTransferMains",
                        principalColumn: "TransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueMains",
                columns: table => new
                {
                    ProductionIssueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    IssueNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueMains", x => x.ProductionIssueId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueMains_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueMains_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderLines",
                columns: table => new
                {
                    ProductionOrderLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WastagePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderLines", x => x.ProductionOrderLineId);
                    table.ForeignKey(
                        name: "FK_ProductionOrderLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionOrderLines_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceiptMains",
                columns: table => new
                {
                    ProductionReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    ReceiptNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductionOrderId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceiptMains", x => x.ProductionReceiptId);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptMains_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "ProductionOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptMains_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkIssueDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkIssueDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkIssueDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetailBatches_JobWorkIssueDetails_JobWorkIssueDetailId",
                        column: x => x.JobWorkIssueDetailId,
                        principalTable: "JobWorkIssueDetails",
                        principalColumn: "JobWorkIssueDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkIssueDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkIssueDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkIssueDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkIssueDetailSerials_JobWorkIssueDetails_JobWorkIssueDetailId",
                        column: x => x.JobWorkIssueDetailId,
                        principalTable: "JobWorkIssueDetails",
                        principalColumn: "JobWorkIssueDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkReceiptDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkReceiptDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkReceiptDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetailBatches_JobWorkReceiptDetails_JobWorkReceiptDetailId",
                        column: x => x.JobWorkReceiptDetailId,
                        principalTable: "JobWorkReceiptDetails",
                        principalColumn: "JobWorkReceiptDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobWorkReceiptDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobWorkReceiptDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobWorkReceiptDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobWorkReceiptDetailSerials_JobWorkReceiptDetails_JobWorkReceiptDetailId",
                        column: x => x.JobWorkReceiptDetailId,
                        principalTable: "JobWorkReceiptDetails",
                        principalColumn: "JobWorkReceiptDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturnDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetails_PurchaseReturnMains_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "PurchaseReturnMains",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GRNDetails",
                columns: table => new
                {
                    GRNDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: true),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: true),
                    HsnCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PriceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OrderedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviouslyReceivedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_GRNDetails", x => x.GRNDetailId);
                    table.ForeignKey(
                        name: "FK_GRNDetails_GRNMains_GRNId",
                        column: x => x.GRNId,
                        principalTable: "GRNMains",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId");
                    table.ForeignKey(
                        name: "FK_GRNDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNDetails_PurchaseOrderDetails_PurchaseOrderDetailId",
                        column: x => x.PurchaseOrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_GRNDetails_PurchaseOrderMains_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrderMains",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderDetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderTaxDetails_PurchaseOrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "PurchaseOrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderTaxDetails_PurchaseOrderMains_OrderId",
                        column: x => x.OrderId,
                        principalTable: "PurchaseOrderMains",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderMains",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinYearId = table.Column<int>(type: "int", nullable: false),
                    OrderNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuotationId = table.Column<int>(type: "int", nullable: true),
                    QuotationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessPartnerId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    SalesPersonId = table.Column<int>(type: "int", nullable: true),
                    BillAddressId = table.Column<int>(type: "int", nullable: true),
                    ShipAddressId = table.Column<int>(type: "int", nullable: true),
                    SalesStateCode = table.Column<int>(type: "int", nullable: true),
                    BillStateCode = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderMains", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_BusinessPartnerAddresses_BillAddressId",
                        column: x => x.BillAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_BusinessPartnerAddresses_ShipAddressId",
                        column: x => x.ShipAddressId,
                        principalTable: "BusinessPartnerAddresses",
                        principalColumn: "BPAddressId");
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_BusinessPartnerContacts_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "BusinessPartnerContacts",
                        principalColumn: "BPContactId");
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_BusinessPartners_BusinessPartnerId",
                        column: x => x.BusinessPartnerId,
                        principalTable: "BusinessPartners",
                        principalColumn: "BusinessPartnerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_SalesPersons_SalesPersonId",
                        column: x => x.SalesPersonId,
                        principalTable: "SalesPersons",
                        principalColumn: "SalesPersonId");
                    table.ForeignKey(
                        name: "FK_SalesOrderMains_SalesQuotationMains_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "SalesQuotationMains",
                        principalColumn: "QuotationId");
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PriceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddisDiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AddisDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_SalesQuotationDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationDetails_SalesQuotationMains_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "SalesQuotationMains",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetailBatches_ItemBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetailBatches_SalesReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    SerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetailSerials_ItemSerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetailSerials_SalesReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    SGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CessPostingAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_Accounts_CGSTPostingAccountId",
                        column: x => x.CGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_Accounts_CessPostingAccountId",
                        column: x => x.CessPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_Accounts_IGSTPostingAccountId",
                        column: x => x.IGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_Accounts_SGSTPostingAccountId",
                        column: x => x.SGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_SalesReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_SalesReturnMains_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "SalesReturnMains",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetailBatches_StockAdjustmentDetails_AdjustmentDetailId",
                        column: x => x.AdjustmentDetailId,
                        principalTable: "StockAdjustmentDetails",
                        principalColumn: "AdjustmentDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetailSerials_StockAdjustmentDetails_AdjustmentDetailId",
                        column: x => x.AdjustmentDetailId,
                        principalTable: "StockAdjustmentDetails",
                        principalColumn: "AdjustmentDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferDetailBatches_StockTransferDetails_TransferDetailId",
                        column: x => x.TransferDetailId,
                        principalTable: "StockTransferDetails",
                        principalColumn: "TransferDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferDetailSerials_StockTransferDetails_TransferDetailId",
                        column: x => x.TransferDetailId,
                        principalTable: "StockTransferDetails",
                        principalColumn: "TransferDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueDetails",
                columns: table => new
                {
                    ProductionIssueDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionIssueId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueDetails", x => x.ProductionIssueDetailId);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetails_ProductionIssueMains_ProductionIssueId",
                        column: x => x.ProductionIssueId,
                        principalTable: "ProductionIssueMains",
                        principalColumn: "ProductionIssueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceiptDetails",
                columns: table => new
                {
                    ProductionReceiptDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionReceiptId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceiptDetails", x => x.ProductionReceiptDetailId);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetails_ProductionReceiptMains_ProductionReceiptId",
                        column: x => x.ProductionReceiptId,
                        principalTable: "ProductionReceiptMains",
                        principalColumn: "ProductionReceiptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetailBatches_ItemBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetailBatches_PurchaseReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    SerialId = table.Column<int>(type: "int", nullable: false),
                    SerialNo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetailSerials_ItemSerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetailSerials_PurchaseReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    SGSTPostingAccountId = table.Column<int>(type: "int", nullable: true),
                    CessPostingAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_Accounts_CGSTPostingAccountId",
                        column: x => x.CGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_Accounts_CessPostingAccountId",
                        column: x => x.CessPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_Accounts_IGSTPostingAccountId",
                        column: x => x.IGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_Accounts_SGSTPostingAccountId",
                        column: x => x.SGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_PurchaseReturnDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseReturnDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_PurchaseReturnMains_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "PurchaseReturnMains",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GRNTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GRNId = table.Column<int>(type: "int", nullable: false),
                    GRNDetailId = table.Column<int>(type: "int", nullable: true),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRNTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_GRNTaxDetails_GRNDetails_GRNDetailId",
                        column: x => x.GRNDetailId,
                        principalTable: "GRNDetails",
                        principalColumn: "GRNDetailId");
                    table.ForeignKey(
                        name: "FK_GRNTaxDetails_GRNMains_GRNId",
                        column: x => x.GRNId,
                        principalTable: "GRNMains",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GRNTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AddisDiscountRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddisDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxIncluded = table.Column<bool>(type: "bit", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GRNId = table.Column<int>(type: "int", nullable: true),
                    GRNDetailId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetails", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetails_GRNDetails_GRNDetailId",
                        column: x => x.GRNDetailId,
                        principalTable: "GRNDetails",
                        principalColumn: "GRNDetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetails_GRNMains_GRNId",
                        column: x => x.GRNId,
                        principalTable: "GRNMains",
                        principalColumn: "GRNId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetails_PurchaseInvoiceMains_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "PurchaseInvoiceMains",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SalesOrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetails_SalesOrderMains_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SalesOrderMains",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_SalesQuotationTaxDetails_SalesQuotationDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "SalesQuotationDetails",
                        principalColumn: "DetailId");
                    table.ForeignKey(
                        name: "FK_SalesQuotationTaxDetails_SalesQuotationMains_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "SalesQuotationMains",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesQuotationTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionIssueDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetailBatches_ProductionIssueDetails_ProductionIssueDetailId",
                        column: x => x.ProductionIssueDetailId,
                        principalTable: "ProductionIssueDetails",
                        principalColumn: "ProductionIssueDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionIssueDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionIssueDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIssueDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionIssueDetailSerials_ProductionIssueDetails_ProductionIssueDetailId",
                        column: x => x.ProductionIssueDetailId,
                        principalTable: "ProductionIssueDetails",
                        principalColumn: "ProductionIssueDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceiptDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionReceiptDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemBatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceiptDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetailBatches_ItemBatches_ItemBatchId",
                        column: x => x.ItemBatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetailBatches_ProductionReceiptDetails_ProductionReceiptDetailId",
                        column: x => x.ProductionReceiptDetailId,
                        principalTable: "ProductionReceiptDetails",
                        principalColumn: "ProductionReceiptDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionReceiptDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionReceiptDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionReceiptDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetailSerials_ItemSerials_ItemSerialId",
                        column: x => x.ItemSerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionReceiptDetailSerials_ProductionReceiptDetails_ProductionReceiptDetailId",
                        column: x => x.ProductionReceiptDetailId,
                        principalTable: "ProductionReceiptDetails",
                        principalColumn: "ProductionReceiptDetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetailBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetailBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetailBatches_ItemBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "ItemBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetailBatches_PurchaseInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseInvoiceDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetailSerials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetailId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    SerialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetailSerials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetailSerials_ItemSerials_SerialId",
                        column: x => x.SerialId,
                        principalTable: "ItemSerials",
                        principalColumn: "SerialId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetailSerials_PurchaseInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseInvoiceDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceTaxDetails",
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
                    table.PrimaryKey("PK_PurchaseInvoiceTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_Accounts_CGSTPostingAccountId",
                        column: x => x.CGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_Accounts_CessPostingAccountId",
                        column: x => x.CessPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_Accounts_IGSTPostingAccountId",
                        column: x => x.IGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_Accounts_SGSTPostingAccountId",
                        column: x => x.SGSTPostingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_PurchaseInvoiceDetails_DetailId",
                        column: x => x.DetailId,
                        principalTable: "PurchaseInvoiceDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_PurchaseInvoiceMains_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "PurchaseInvoiceMains",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsDeliveryDetails",
                columns: table => new
                {
                    DeliveryDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderDetailId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviouslyDeliveredQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_GoodsDeliveryDetails", x => x.DeliveryDetailId);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryDetails_GoodsDeliveryMains_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "GoodsDeliveryMains",
                        principalColumn: "DeliveryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryDetails_Hsns_HsnId",
                        column: x => x.HsnId,
                        principalTable: "Hsns",
                        principalColumn: "HsnId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryDetails_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryDetails_SalesOrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryDetails_SalesOrderMains_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SalesOrderMains",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderDetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_SalesOrderTaxDetails_SalesOrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "SalesOrderDetails",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_SalesOrderTaxDetails_SalesOrderMains_OrderId",
                        column: x => x.OrderId,
                        principalTable: "SalesOrderMains",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId");
                });

            migrationBuilder.CreateTable(
                name: "GoodsDeliveryTaxDetails",
                columns: table => new
                {
                    TaxDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDetailId = table.Column<int>(type: "int", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: false),
                    IGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SGSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CessAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsDeliveryTaxDetails", x => x.TaxDetailId);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryTaxDetails_GoodsDeliveryDetails_DeliveryDetailId",
                        column: x => x.DeliveryDetailId,
                        principalTable: "GoodsDeliveryDetails",
                        principalColumn: "DeliveryDetailId");
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryTaxDetails_GoodsDeliveryMains_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "GoodsDeliveryMains",
                        principalColumn: "DeliveryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsDeliveryTaxDetails_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "TaxId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_BillAddressId",
                table: "SalesInvoiceMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_ContactPersonId",
                table: "SalesInvoiceMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_SalesPersonId",
                table: "SalesInvoiceMains",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceMains_ShipAddressId",
                table: "SalesInvoiceMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountLedgerPostings_AccountId",
                table: "AccountLedgerPostings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountLedgerPostings_BusinessPartnerId",
                table: "AccountLedgerPostings",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLevels_CompanyId_DocumentType_LevelNumber",
                table: "ApprovalLevels",
                columns: new[] { "CompanyId", "DocumentType", "LevelNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalLogs_CompanyId_DocumentType_DocumentId",
                table: "ApprovalLogs",
                columns: new[] { "CompanyId", "DocumentType", "DocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterial_ItemId",
                table: "BillOfMaterial",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BomLines_BomId",
                table: "BomLines",
                column: "BomId");

            migrationBuilder.CreateIndex(
                name: "IX_BomLines_ItemId",
                table: "BomLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBankEntries_HeadAccountId",
                table: "CashBankEntries",
                column: "HeadAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBankEntryLines_AccountId",
                table: "CashBankEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashBankEntryLines_CashBankEntryId",
                table: "CashBankEntryLines",
                column: "CashBankEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyConfigs_CompanyId_ConfigKey",
                table: "CompanyConfigs",
                columns: new[] { "CompanyId", "ConfigKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CopyLog_CompanyItem",
                table: "DocumentCopyLogs",
                columns: new[] { "CompanyId", "ItemId", "SourceType", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CopyLog_Source",
                table: "DocumentCopyLogs",
                columns: new[] { "SourceType", "SourceId", "SourceDetailId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CopyLog_Target",
                table: "DocumentCopyLogs",
                columns: new[] { "TargetType", "TargetId", "TargetDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSeriesMappings_AccountId",
                table: "DocumentSeriesMappings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSeriesMappings_CompanyId_AccountId",
                table: "DocumentSeriesMappings",
                columns: new[] { "CompanyId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSeriesMappings_SeriesId",
                table: "DocumentSeriesMappings",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryDetails_DeliveryId",
                table: "GoodsDeliveryDetails",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryDetails_HsnId",
                table: "GoodsDeliveryDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryDetails_ItemId",
                table: "GoodsDeliveryDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryDetails_OrderDetailId",
                table: "GoodsDeliveryDetails",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryDetails_OrderId",
                table: "GoodsDeliveryDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_BillAddressId",
                table: "GoodsDeliveryMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_BusinessPartnerId",
                table: "GoodsDeliveryMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_ContactPersonId",
                table: "GoodsDeliveryMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_LocationId",
                table: "GoodsDeliveryMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_SalesPersonId",
                table: "GoodsDeliveryMains",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryMains_ShipAddressId",
                table: "GoodsDeliveryMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryTaxDetails_DeliveryDetailId",
                table: "GoodsDeliveryTaxDetails",
                column: "DeliveryDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryTaxDetails_DeliveryId",
                table: "GoodsDeliveryTaxDetails",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsDeliveryTaxDetails_TaxId",
                table: "GoodsDeliveryTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_GRNId",
                table: "GRNDetails",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_HsnId",
                table: "GRNDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_ItemId",
                table: "GRNDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_PurchaseOrderDetailId",
                table: "GRNDetails",
                column: "PurchaseOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_PurchaseOrderId",
                table: "GRNDetails",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNMains_BillAddressId",
                table: "GRNMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNMains_BusinessPartnerId",
                table: "GRNMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNMains_ContactPersonId",
                table: "GRNMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNMains_LocationId",
                table: "GRNMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNMains_ShipAddressId",
                table: "GRNMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNTaxDetails_GRNDetailId",
                table: "GRNTaxDetails",
                column: "GRNDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNTaxDetails_GRNId",
                table: "GRNTaxDetails",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNTaxDetails_TaxId",
                table: "GRNTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPaymentAllocations_InvoiceId",
                table: "IncomingPaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPaymentAllocations_PaymentId",
                table: "IncomingPaymentAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPaymentMains_BusinessPartnerId",
                table: "IncomingPaymentMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPaymentMains_DepositAccountId",
                table: "IncomingPaymentMains",
                column: "DepositAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_CompanyId_ItemId_BatchNo",
                table: "ItemBatches",
                columns: new[] { "CompanyId", "ItemId", "BatchNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_ItemId",
                table: "ItemBatches",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSerials_CompanyId_ItemId_SerialNo",
                table: "ItemSerials",
                columns: new[] { "CompanyId", "ItemId", "SerialNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSerials_ItemId",
                table: "ItemSerials",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetailBatches_ItemBatchId",
                table: "JobWorkIssueDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetailBatches_JobWorkIssueDetailId",
                table: "JobWorkIssueDetailBatches",
                column: "JobWorkIssueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetails_ItemId",
                table: "JobWorkIssueDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetails_JobWorkIssueId",
                table: "JobWorkIssueDetails",
                column: "JobWorkIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetailSerials_ItemSerialId",
                table: "JobWorkIssueDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueDetailSerials_JobWorkIssueDetailId",
                table: "JobWorkIssueDetailSerials",
                column: "JobWorkIssueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueMains_BusinessPartnerId",
                table: "JobWorkIssueMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueMains_LocationId",
                table: "JobWorkIssueMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkIssueMains_WarehouseId",
                table: "JobWorkIssueMains",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetailBatches_ItemBatchId",
                table: "JobWorkReceiptDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetailBatches_JobWorkReceiptDetailId",
                table: "JobWorkReceiptDetailBatches",
                column: "JobWorkReceiptDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetails_ItemId",
                table: "JobWorkReceiptDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetails_JobWorkReceiptId",
                table: "JobWorkReceiptDetails",
                column: "JobWorkReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetailSerials_ItemSerialId",
                table: "JobWorkReceiptDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptDetailSerials_JobWorkReceiptDetailId",
                table: "JobWorkReceiptDetailSerials",
                column: "JobWorkReceiptDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptMains_BusinessPartnerId",
                table: "JobWorkReceiptMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptMains_LocationId",
                table: "JobWorkReceiptMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobWorkReceiptMains_WarehouseId",
                table: "JobWorkReceiptMains",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_AccountId",
                table: "JournalEntries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPaymentAllocations_BillId",
                table: "OutgoingPaymentAllocations",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPaymentAllocations_PaymentId",
                table: "OutgoingPaymentAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPaymentMains_BusinessPartnerId",
                table: "OutgoingPaymentMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPaymentMains_PaymentAccountId",
                table: "OutgoingPaymentMains",
                column: "PaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetailBatches_ItemBatchId",
                table: "ProductionIssueDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetailBatches_ProductionIssueDetailId",
                table: "ProductionIssueDetailBatches",
                column: "ProductionIssueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetails_ItemId",
                table: "ProductionIssueDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetails_ProductionIssueId",
                table: "ProductionIssueDetails",
                column: "ProductionIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetailSerials_ItemSerialId",
                table: "ProductionIssueDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueDetailSerials_ProductionIssueDetailId",
                table: "ProductionIssueDetailSerials",
                column: "ProductionIssueDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueMains_LocationId",
                table: "ProductionIssueMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueMains_ProductionOrderId",
                table: "ProductionIssueMains",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionIssueMains_WarehouseId",
                table: "ProductionIssueMains",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLines_ItemId",
                table: "ProductionOrderLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderLines_ProductionOrderId",
                table: "ProductionOrderLines",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_BomId",
                table: "ProductionOrders",
                column: "BomId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_ItemId",
                table: "ProductionOrders",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetailBatches_ItemBatchId",
                table: "ProductionReceiptDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetailBatches_ProductionReceiptDetailId",
                table: "ProductionReceiptDetailBatches",
                column: "ProductionReceiptDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetails_ItemId",
                table: "ProductionReceiptDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetails_ProductionReceiptId",
                table: "ProductionReceiptDetails",
                column: "ProductionReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetailSerials_ItemSerialId",
                table: "ProductionReceiptDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptDetailSerials_ProductionReceiptDetailId",
                table: "ProductionReceiptDetailSerials",
                column: "ProductionReceiptDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptMains_LocationId",
                table: "ProductionReceiptMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptMains_ProductionOrderId",
                table: "ProductionReceiptMains",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionReceiptMains_WarehouseId",
                table: "ProductionReceiptMains",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetailBatches_BatchId",
                table: "PurchaseInvoiceDetailBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetailBatches_DetailId",
                table: "PurchaseInvoiceDetailBatches",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_GRNDetailId",
                table: "PurchaseInvoiceDetails",
                column: "GRNDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_GRNId",
                table: "PurchaseInvoiceDetails",
                column: "GRNId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_HsnId",
                table: "PurchaseInvoiceDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_InvoiceId",
                table: "PurchaseInvoiceDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetails_ItemId",
                table: "PurchaseInvoiceDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetailSerials_DetailId",
                table: "PurchaseInvoiceDetailSerials",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetailSerials_SerialId",
                table: "PurchaseInvoiceDetailSerials",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_BillAddressId",
                table: "PurchaseInvoiceMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_BusinessPartnerId",
                table: "PurchaseInvoiceMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_ContactPersonId",
                table: "PurchaseInvoiceMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_LocationId",
                table: "PurchaseInvoiceMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_PurchaseAccountId",
                table: "PurchaseInvoiceMains",
                column: "PurchaseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceMains_ShipAddressId",
                table: "PurchaseInvoiceMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_CessPostingAccountId",
                table: "PurchaseInvoiceTaxDetails",
                column: "CessPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_CGSTPostingAccountId",
                table: "PurchaseInvoiceTaxDetails",
                column: "CGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_DetailId",
                table: "PurchaseInvoiceTaxDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_IGSTPostingAccountId",
                table: "PurchaseInvoiceTaxDetails",
                column: "IGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_InvoiceId",
                table: "PurchaseInvoiceTaxDetails",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_SGSTPostingAccountId",
                table: "PurchaseInvoiceTaxDetails",
                column: "SGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceTaxDetails_TaxId",
                table: "PurchaseInvoiceTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_HsnId",
                table: "PurchaseOrderDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_ItemId",
                table: "PurchaseOrderDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_OrderId",
                table: "PurchaseOrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMains_BillAddressId",
                table: "PurchaseOrderMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMains_BusinessPartnerId",
                table: "PurchaseOrderMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMains_ContactPersonId",
                table: "PurchaseOrderMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMains_LocationId",
                table: "PurchaseOrderMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderMains_ShipAddressId",
                table: "PurchaseOrderMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderTaxDetails_OrderDetailId",
                table: "PurchaseOrderTaxDetails",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderTaxDetails_OrderId",
                table: "PurchaseOrderTaxDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderTaxDetails_TaxId",
                table: "PurchaseOrderTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetailBatches_BatchId",
                table: "PurchaseReturnDetailBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetailBatches_DetailId",
                table: "PurchaseReturnDetailBatches",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_HsnId",
                table: "PurchaseReturnDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_ItemId",
                table: "PurchaseReturnDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetails_ReturnId",
                table: "PurchaseReturnDetails",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetailSerials_DetailId",
                table: "PurchaseReturnDetailSerials",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetailSerials_SerialId",
                table: "PurchaseReturnDetailSerials",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_BillAddressId",
                table: "PurchaseReturnMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_BusinessPartnerId",
                table: "PurchaseReturnMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_CompanyId",
                table: "PurchaseReturnMains",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_LocationId",
                table: "PurchaseReturnMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_OriginalInvoiceId",
                table: "PurchaseReturnMains",
                column: "OriginalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnMains_PurchaseAccountId",
                table: "PurchaseReturnMains",
                column: "PurchaseAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_CessPostingAccountId",
                table: "PurchaseReturnTaxDetails",
                column: "CessPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_CGSTPostingAccountId",
                table: "PurchaseReturnTaxDetails",
                column: "CGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_DetailId",
                table: "PurchaseReturnTaxDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_IGSTPostingAccountId",
                table: "PurchaseReturnTaxDetails",
                column: "IGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_ReturnId",
                table: "PurchaseReturnTaxDetails",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_SGSTPostingAccountId",
                table: "PurchaseReturnTaxDetails",
                column: "SGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnTaxDetails_TaxId",
                table: "PurchaseReturnTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetailBatches_BatchId",
                table: "SalesInvoiceDetailBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetailBatches_DetailId",
                table: "SalesInvoiceDetailBatches",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetailSerials_DetailId",
                table: "SalesInvoiceDetailSerials",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceDetailSerials_SerialId",
                table: "SalesInvoiceDetailSerials",
                column: "SerialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_HsnId",
                table: "SalesOrderDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_ItemId",
                table: "SalesOrderDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetails_OrderId",
                table: "SalesOrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_BillAddressId",
                table: "SalesOrderMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_BusinessPartnerId",
                table: "SalesOrderMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_CompanyId",
                table: "SalesOrderMains",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_ContactPersonId",
                table: "SalesOrderMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_LocationId",
                table: "SalesOrderMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_QuotationId",
                table: "SalesOrderMains",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_SalesPersonId",
                table: "SalesOrderMains",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderMains_ShipAddressId",
                table: "SalesOrderMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTaxDetails_OrderDetailId",
                table: "SalesOrderTaxDetails",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTaxDetails_OrderId",
                table: "SalesOrderTaxDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTaxDetails_TaxId",
                table: "SalesOrderTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetails_HsnId",
                table: "SalesQuotationDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetails_ItemId",
                table: "SalesQuotationDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetails_QuotationId",
                table: "SalesQuotationDetails",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_BillAddressId",
                table: "SalesQuotationMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_BusinessPartnerId",
                table: "SalesQuotationMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_ContactPersonId",
                table: "SalesQuotationMains",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_LocationId",
                table: "SalesQuotationMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_ParentQuotationId",
                table: "SalesQuotationMains",
                column: "ParentQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_SalesPersonId",
                table: "SalesQuotationMains",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationMains_ShipAddressId",
                table: "SalesQuotationMains",
                column: "ShipAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTaxDetails_DetailId",
                table: "SalesQuotationTaxDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTaxDetails_QuotationId",
                table: "SalesQuotationTaxDetails",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationTaxDetails_TaxId",
                table: "SalesQuotationTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetailBatches_BatchId",
                table: "SalesReturnDetailBatches",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetailBatches_DetailId",
                table: "SalesReturnDetailBatches",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_HsnId",
                table: "SalesReturnDetails",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ItemId",
                table: "SalesReturnDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetails_ReturnId",
                table: "SalesReturnDetails",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetailSerials_DetailId",
                table: "SalesReturnDetailSerials",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetailSerials_SerialId",
                table: "SalesReturnDetailSerials",
                column: "SerialId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_BillAddressId",
                table: "SalesReturnMains",
                column: "BillAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_BusinessPartnerId",
                table: "SalesReturnMains",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_CompanyId",
                table: "SalesReturnMains",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_LocationId",
                table: "SalesReturnMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_OriginalInvoiceId",
                table: "SalesReturnMains",
                column: "OriginalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnMains_SalesAccountId",
                table: "SalesReturnMains",
                column: "SalesAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_CessPostingAccountId",
                table: "SalesReturnTaxDetails",
                column: "CessPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_CGSTPostingAccountId",
                table: "SalesReturnTaxDetails",
                column: "CGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_DetailId",
                table: "SalesReturnTaxDetails",
                column: "DetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_IGSTPostingAccountId",
                table: "SalesReturnTaxDetails",
                column: "IGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_ReturnId",
                table: "SalesReturnTaxDetails",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_SGSTPostingAccountId",
                table: "SalesReturnTaxDetails",
                column: "SGSTPostingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnTaxDetails_TaxId",
                table: "SalesReturnTaxDetails",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetailBatches_AdjustmentDetailId",
                table: "StockAdjustmentDetailBatches",
                column: "AdjustmentDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetailBatches_ItemBatchId",
                table: "StockAdjustmentDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetails_AdjustmentId",
                table: "StockAdjustmentDetails",
                column: "AdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetails_ItemId",
                table: "StockAdjustmentDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetailSerials_AdjustmentDetailId",
                table: "StockAdjustmentDetailSerials",
                column: "AdjustmentDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetailSerials_ItemSerialId",
                table: "StockAdjustmentDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentMains_LocationId",
                table: "StockAdjustmentMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentMains_WarehouseId",
                table: "StockAdjustmentMains",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgers_BusinessPartnerId",
                table: "StockLedgers",
                column: "BusinessPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgers_ItemId",
                table: "StockLedgers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgers_WarehouseId",
                table: "StockLedgers",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetailBatches_ItemBatchId",
                table: "StockTransferDetailBatches",
                column: "ItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetailBatches_TransferDetailId",
                table: "StockTransferDetailBatches",
                column: "TransferDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetails_ItemId",
                table: "StockTransferDetails",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetails_TransferId",
                table: "StockTransferDetails",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetailSerials_ItemSerialId",
                table: "StockTransferDetailSerials",
                column: "ItemSerialId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetailSerials_TransferDetailId",
                table: "StockTransferDetailSerials",
                column: "TransferDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferMains_FromWarehouseId",
                table: "StockTransferMains",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferMains_LocationId",
                table: "StockTransferMains",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferMains_ToWarehouseId",
                table: "StockTransferMains",
                column: "ToWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerAddresses_BillAddressId",
                table: "SalesInvoiceMains",
                column: "BillAddressId",
                principalTable: "BusinessPartnerAddresses",
                principalColumn: "BPAddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerAddresses_ShipAddressId",
                table: "SalesInvoiceMains",
                column: "ShipAddressId",
                principalTable: "BusinessPartnerAddresses",
                principalColumn: "BPAddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerContacts_ContactPersonId",
                table: "SalesInvoiceMains",
                column: "ContactPersonId",
                principalTable: "BusinessPartnerContacts",
                principalColumn: "BPContactId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceMains_SalesPersons_SalesPersonId",
                table: "SalesInvoiceMains",
                column: "SalesPersonId",
                principalTable: "SalesPersons",
                principalColumn: "SalesPersonId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerAddresses_BillAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerAddresses_ShipAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_BusinessPartnerContacts_ContactPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceMains_SalesPersons_SalesPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropTable(
                name: "AccountLedgerPostings");

            migrationBuilder.DropTable(
                name: "ApprovalLevels");

            migrationBuilder.DropTable(
                name: "ApprovalLogs");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "BomLines");

            migrationBuilder.DropTable(
                name: "CashBankEntryLines");

            migrationBuilder.DropTable(
                name: "CompanyConfigs");

            migrationBuilder.DropTable(
                name: "DocumentCopyLogs");

            migrationBuilder.DropTable(
                name: "DocumentSeriesMappings");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "GoodsDeliveryTaxDetails");

            migrationBuilder.DropTable(
                name: "GRNTaxDetails");

            migrationBuilder.DropTable(
                name: "IncomingPaymentAllocations");

            migrationBuilder.DropTable(
                name: "JobWorkIssueDetailBatches");

            migrationBuilder.DropTable(
                name: "JobWorkIssueDetailSerials");

            migrationBuilder.DropTable(
                name: "JobWorkReceiptDetailBatches");

            migrationBuilder.DropTable(
                name: "JobWorkReceiptDetailSerials");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "OutgoingPaymentAllocations");

            migrationBuilder.DropTable(
                name: "ProductionIssueDetailBatches");

            migrationBuilder.DropTable(
                name: "ProductionIssueDetailSerials");

            migrationBuilder.DropTable(
                name: "ProductionOrderLines");

            migrationBuilder.DropTable(
                name: "ProductionReceiptDetailBatches");

            migrationBuilder.DropTable(
                name: "ProductionReceiptDetailSerials");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetailBatches");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetailSerials");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceTaxDetails");

            migrationBuilder.DropTable(
                name: "PurchaseOrderTaxDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetailBatches");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetailSerials");

            migrationBuilder.DropTable(
                name: "PurchaseReturnTaxDetails");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDetailBatches");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDetailSerials");

            migrationBuilder.DropTable(
                name: "SalesOrderTaxDetails");

            migrationBuilder.DropTable(
                name: "SalesQuotationTaxDetails");

            migrationBuilder.DropTable(
                name: "SalesReturnDetailBatches");

            migrationBuilder.DropTable(
                name: "SalesReturnDetailSerials");

            migrationBuilder.DropTable(
                name: "SalesReturnTaxDetails");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDetailBatches");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDetailSerials");

            migrationBuilder.DropTable(
                name: "StockLedgers");

            migrationBuilder.DropTable(
                name: "StockTransferDetailBatches");

            migrationBuilder.DropTable(
                name: "StockTransferDetailSerials");

            migrationBuilder.DropTable(
                name: "CashBankEntries");

            migrationBuilder.DropTable(
                name: "DocumentSeries");

            migrationBuilder.DropTable(
                name: "GoodsDeliveryDetails");

            migrationBuilder.DropTable(
                name: "IncomingPaymentMains");

            migrationBuilder.DropTable(
                name: "JobWorkIssueDetails");

            migrationBuilder.DropTable(
                name: "JobWorkReceiptDetails");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "OutgoingPaymentMains");

            migrationBuilder.DropTable(
                name: "ProductionIssueDetails");

            migrationBuilder.DropTable(
                name: "ProductionReceiptDetails");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetails");

            migrationBuilder.DropTable(
                name: "SalesQuotationDetails");

            migrationBuilder.DropTable(
                name: "SalesReturnDetails");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDetails");

            migrationBuilder.DropTable(
                name: "ItemBatches");

            migrationBuilder.DropTable(
                name: "ItemSerials");

            migrationBuilder.DropTable(
                name: "StockTransferDetails");

            migrationBuilder.DropTable(
                name: "GoodsDeliveryMains");

            migrationBuilder.DropTable(
                name: "SalesOrderDetails");

            migrationBuilder.DropTable(
                name: "JobWorkIssueMains");

            migrationBuilder.DropTable(
                name: "JobWorkReceiptMains");

            migrationBuilder.DropTable(
                name: "ProductionIssueMains");

            migrationBuilder.DropTable(
                name: "ProductionReceiptMains");

            migrationBuilder.DropTable(
                name: "GRNDetails");

            migrationBuilder.DropTable(
                name: "PurchaseReturnMains");

            migrationBuilder.DropTable(
                name: "SalesReturnMains");

            migrationBuilder.DropTable(
                name: "StockAdjustmentMains");

            migrationBuilder.DropTable(
                name: "StockTransferMains");

            migrationBuilder.DropTable(
                name: "SalesOrderMains");

            migrationBuilder.DropTable(
                name: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "GRNMains");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceMains");

            migrationBuilder.DropTable(
                name: "SalesQuotationMains");

            migrationBuilder.DropTable(
                name: "BillOfMaterial");

            migrationBuilder.DropTable(
                name: "PurchaseOrderMains");

            migrationBuilder.DropTable(
                name: "SalesPersons");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceMains_BillAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceMains_ContactPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceMains_SalesPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceMains_ShipAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BillAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "BillStateCode",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "ContactPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "LrDate",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "LrNo",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "SalesPersonId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "SalesStateCode",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "ShipAddressId",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "TransportName",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "VehicleNo",
                table: "SalesInvoiceMains");

            migrationBuilder.DropColumn(
                name: "DeliveryDetailId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "DeliveryId",
                table: "SalesInvoiceDetails");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "Users",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "Users",
                newName: "UpdatedBy");

            migrationBuilder.AlterColumn<int>(
                name: "RoleId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "BusinessPartnerAddresses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
