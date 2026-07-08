using FinVentoryAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<MenuGroup> MenuGroups { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<RoleRight> RoleRights { get; set; }
        public DbSet<AccountGroup> AccountGroups { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet <Tax> Taxes { get; set; }
        public DbSet<Hsn> Hsns { get; set; }
        public DbSet <ItemGroup> ItemGroups { get; set; }
        public DbSet <Brand>  Brands { get; set; }
        public DbSet <Warehouse> Warehouses { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemPrice> ItemsPrices { get; set; }
        public DbSet<BusinessPartner> BusinessPartners { get; set; }
        public DbSet<BusinessPartnerAddress> BusinessPartnerAddresses { get; set; }
        public DbSet<BusinessPartnerContact> BusinessPartnerContacts { get; set; }
        public DbSet<OpeningBalance> OpeningBalances { get; set; }
        public DbSet<OpeningItemBalance> OpeningItemBalances { get; set; }
        public DbSet<SalesInvoiceMain> SalesInvoiceMains { get; set; }
        public DbSet<SalesInvoiceDetail> SalesInvoiceDetails { get; set; }
        public DbSet<SalesInvoiceTaxDetail> SalesInvoiceTaxDetails { get; set; }
        public DbSet<SalesPerson> SalesPersons { get; set; }
        public DbSet<DocumentSeries> DocumentSeries { get; set; }
        public DbSet<DocumentSeriesMapping> DocumentSeriesMappings { get; set; }
        public DbSet<StockLedger> StockLedgers { get; set; }
        public DbSet<AccountLedgerPosting> AccountLedgerPostings { get; set; }
        public DbSet<ItemBatch> ItemBatches { get; set; }
        public DbSet<ItemSerial> ItemSerials { get; set; }
        public DbSet<SalesInvoiceDetailBatch> SalesInvoiceDetailBatches { get; set; }
        public DbSet<SalesInvoiceDetailSerial> SalesInvoiceDetailSerials { get; set; }
        public DbSet<IncomingPaymentMain> IncomingPaymentMains { get; set; }
        public DbSet<IncomingPaymentAllocation> IncomingPaymentAllocations { get; set; }
        public DbSet<PurchaseInvoiceMain> PurchaseInvoiceMains { get; set; }
        public DbSet<PurchaseInvoiceDetail> PurchaseInvoiceDetails { get; set; }
        public DbSet<PurchaseInvoiceTaxDetail> PurchaseInvoiceTaxDetails { get; set; }
        public DbSet<PurchaseInvoiceDetailBatch> PurchaseInvoiceDetailBatches { get; set; }
        public DbSet<PurchaseInvoiceDetailSerial> PurchaseInvoiceDetailSerials { get; set; }
        public DbSet<OutgoingPaymentMain> OutgoingPaymentMains { get; set; }
        public DbSet<OutgoingPaymentAllocation> OutgoingPaymentAllocations { get; set; }
        public DbSet<CashBankEntry> CashBankEntries { get; set; }
        public DbSet<CashBankEntryLine> CashBankEntryLines { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<BillOfMaterial> BillOfMaterial { get; set; }
        public DbSet<BomLine> BomLines { get; set; }
        public DbSet<SalesReturnMain> SalesReturnMains { get; set; }
        public DbSet<SalesReturnDetail> SalesReturnDetails { get; set; }
        public DbSet<SalesReturnTaxDetail> SalesReturnTaxDetails { get; set; }  
        public DbSet<SalesReturnDetailBatch> SalesReturnDetailBatches { get; set; }
        public DbSet<SalesReturnDetailSerial> SalesReturnDetailSerials { get; set; }

        public DbSet<PurchaseReturnMain> PurchaseReturnMains { get; set; }
        public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }
        public DbSet<PurchaseReturnTaxDetail> PurchaseReturnTaxDetails { get; set; }
        public DbSet<PurchaseReturnDetailBatch> PurchaseReturnDetailBatches { get; set; }
        public DbSet<PurchaseReturnDetailSerial> PurchaseReturnDetailSerials { get; set; }
        public DbSet<DocumentCopyLog> DocumentCopyLogs { get; set; }
        public DbSet<SalesQuotationMain> SalesQuotationMains { get; set; }
        public DbSet<SalesQuotationDetail> SalesQuotationDetails { get; set; }
        public DbSet<SalesQuotationTaxDetail> SalesQuotationTaxDetails { get; set; }

        public DbSet<SalesOrderMain> SalesOrderMains { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<SalesOrderTaxDetail> SalesOrderTaxDetails { get; set; }

        public DbSet<GoodsDeliveryMain> GoodsDeliveryMains { get; set; }
        public DbSet<GoodsDeliveryDetail> GoodsDeliveryDetails { get; set; }
        public DbSet<GoodsDeliveryTaxDetail> GoodsDeliveryTaxDetails { get; set; }

               
        public DbSet<PurchaseOrderMain> PurchaseOrderMains { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<PurchaseOrderTaxDetail> PurchaseOrderTaxDetails { get; set; }

        public DbSet<GRNMain> GRNMains { get; set; }
        public DbSet<GRNDetail> GRNDetails { get; set; }
        public DbSet<GRNTaxDetail> GRNTaxDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionOrderLine> ProductionOrderLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MenuItem → Module
            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Module)
                .WithMany(m => m.MenuItems)
                .HasForeignKey(mi => mi.ModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            // RoleRight → Role
            modelBuilder.Entity<RoleRight>()
                .HasOne(rr => rr.Role)
                .WithMany(r => r.RoleRights)
                .HasForeignKey(rr => rr.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            // RoleRight → Module ✅ fix multiple cascade path
            modelBuilder.Entity<RoleRight>()
                .HasOne(rr => rr.Module)
                .WithMany() // no navigation to RoleRights
                .HasForeignKey(rr => rr.ModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            // User → Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.NoAction); // important to prevent cascade issues

            modelBuilder.Entity<Item>()
                 .HasOne(i => i.Brand)
                 .WithMany()
                 .HasForeignKey(i => i.BrandId)
                 .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<BusinessPartnerAddress>()
              .HasKey(x => x.BPAddressId);

            modelBuilder.Entity<BusinessPartnerContact>()
                .HasKey(x => x.BPContactId);

            modelBuilder.Entity<BusinessPartner>()
                 .Property(x => x.CreditLimit)
                 .HasPrecision(18, 2);

                         modelBuilder.Entity<Hsn>()
                 .Property(x => x.Cess)
                 .HasPrecision(5, 2);

            modelBuilder.Entity<Item>()
                .Property(x => x.ConversionFactor)
                .HasPrecision(18, 4);

            modelBuilder.Entity<ItemPrice>()
                .Property(x => x.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Tax>(entity =>
            {
                entity.Property(x => x.CGST).HasPrecision(5, 2);
                entity.Property(x => x.SGST).HasPrecision(5, 2);
                entity.Property(x => x.IGST).HasPrecision(5, 2);
                entity.Property(x => x.TaxRate).HasPrecision(5, 2);
            });

            modelBuilder.Entity<BusinessPartnerAddress>()
                .HasOne<BusinessPartner>()
                .WithMany(x => x.BPAddresses)
                .HasForeignKey(x => x.BusinessPartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BusinessPartnerContact>()
                .HasOne<BusinessPartner>()
                .WithMany(x => x.BPContacts)
                .HasForeignKey(x => x.BusinessPartnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BusinessPartner>()
                .HasOne(x => x.accountGroup)
                .WithMany()
                .HasForeignKey(x => x.AccountGroupId);

            modelBuilder.Entity<Hsn>()
                .HasOne(x => x.account)
                .WithMany()
                .HasForeignKey(x => x.CessPostingAc);

            modelBuilder.Entity<Tax>()
                 .HasOne(x => x.IGSTAccount)   // ← tells EF to use this navigation
                 .WithMany()
                 .HasForeignKey(x => x.IGSTPostingAccountId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tax>()
                .HasOne(x => x.CGSTAccount)
                .WithMany()
                .HasForeignKey(x => x.CGSTPostingAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tax>()
                .HasOne(x => x.SGSTAccount)
                .WithMany()
                .HasForeignKey(x => x.SGSTPostingAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // ────────────────────────────────────────────────────
            // SalesInvoiceMain — disable cascade on all FKs
            // ────────────────────────────────────────────────────
            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.BusinessPartner)
                .WithMany()
                .HasForeignKey(x => x.BusinessPartnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.SalesAccount)
                .WithMany()
                .HasForeignKey(x => x.SalesAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            // ────────────────────────────────────────────────────
            // SalesInvoiceDetail — disable cascade on all FKs
            // ────────────────────────────────────────────────────
            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Invoice)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Hsn)
                .WithMany()
                .HasForeignKey(x => x.HsnId)
                .OnDelete(DeleteBehavior.NoAction);

            // ────────────────────────────────────────────────────
            // SalesInvoiceTaxDetail — disable cascade on all FKs
            // ────────────────────────────────────────────────────
            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Invoice)
                .WithMany(x => x.TaxDetails)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Detail)
                .WithMany(x => x.TaxDetails)
                .HasForeignKey(x => x.DetailId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Tax)
                .WithMany()
                .HasForeignKey(x => x.TaxId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.IGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.IGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.CGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.CGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.SGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.SGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.CessPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.CessPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            // ── Item → Hsn ───────────────────────────────────────
            modelBuilder.Entity<Item>()
                .HasOne(x => x.Hsn)
                .WithMany()
                .HasForeignKey(x => x.HSNCodeId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Item → Accounts ──────────────────────────────────
            modelBuilder.Entity<Item>()
                .HasOne(x => x.InventoryAccount)
                .WithMany()
                .HasForeignKey(x => x.InventoryAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(x => x.COGSAccount)
                .WithMany()
                .HasForeignKey(x => x.COGSAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(x => x.SalesAccount)
                .WithMany()
                .HasForeignKey(x => x.SalesAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(x => x.PurchaseAccount)
                .WithMany()
                .HasForeignKey(x => x.PurchaseAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SalesInvoiceMain ──────────────────────────────────
            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.BusinessPartner)
                .WithMany()
                .HasForeignKey(x => x.BusinessPartnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.SalesAccount)
                .WithMany()
                .HasForeignKey(x => x.SalesAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.SalesAccount)
                .WithMany()
                .HasForeignKey(x => x.SalesAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SalesInvoiceDetail ────────────────────────────────
            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Invoice)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceDetail>()
                .HasOne(x => x.Hsn)
                .WithMany()
                .HasForeignKey(x => x.HsnId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── SalesInvoiceTaxDetail ─────────────────────────────
            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Invoice)
                .WithMany(x => x.TaxDetails)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Detail)
                .WithMany(x => x.TaxDetails)
                .HasForeignKey(x => x.DetailId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.Tax)
                .WithMany()
                .HasForeignKey(x => x.TaxId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.IGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.IGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.CGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.CGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.SGSTPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.SGSTPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SalesInvoiceTaxDetail>()
                .HasOne(x => x.CessPostingAccount)
                .WithMany()
                .HasForeignKey(x => x.CessPostingAccountId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Company>()
                 .Property(c => c.State)
                 .HasConversion<int>();

            modelBuilder.Entity<BusinessPartnerAddress>()
                .Property(b => b.State)
                .HasConversion<int>();

            // ── SalesInvoiceMain — two FKs to BusinessPartnerAddress ────
            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.BillAddress)
                .WithMany()
                .HasForeignKey(x => x.BillAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.ShipAddress)
                .WithMany()
                .HasForeignKey(x => x.ShipAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SalesInvoiceMain — FK to BusinessPartnerContact ─────────
            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.ContactPerson)
                .WithMany()
                .HasForeignKey(x => x.ContactPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SalesInvoiceMain — FK to SalesPerson ────────────────────
            modelBuilder.Entity<SalesInvoiceMain>()
                .HasOne(x => x.SalesPerson)
                .WithMany()
                .HasForeignKey(x => x.SalesPersonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentSeriesMapping>()
                  .HasIndex(x => new { x.CompanyId, x.AccountId })
                 .IsUnique();  // one series per account per company


            // ── ItemBatch ────────────────────────────────────────────────────
            modelBuilder.Entity<ItemBatch>(e =>
            {
                e.HasKey(x => x.BatchId);

                e.HasIndex(x => new { x.CompanyId, x.ItemId, x.BatchNo })
                 .IsUnique();                          // BatchNo unique per company+item

                e.Property(x => x.BatchNo).HasMaxLength(100).IsRequired();
                e.Property(x => x.ReceivedQty).HasPrecision(18, 4);
                e.Property(x => x.UsedQty).HasPrecision(18, 4);
                e.Property(x => x.AvailableQty).HasPrecision(18, 4);

                e.HasOne(x => x.Item)
                 .WithMany()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ItemSerial ───────────────────────────────────────────────────
            modelBuilder.Entity<ItemSerial>(e =>
            {
                e.HasKey(x => x.SerialId);

                e.HasIndex(x => new { x.CompanyId, x.ItemId, x.SerialNo })
                 .IsUnique();                          // SerialNo unique per company+item

                e.Property(x => x.SerialNo).HasMaxLength(100).IsRequired();
                e.Property(x => x.Status).HasConversion<byte>();

                e.HasOne(x => x.Item)
                 .WithMany()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── SalesInvoiceDetailBatch ──────────────────────────────────────
            modelBuilder.Entity<SalesInvoiceDetailBatch>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Qty).HasPrecision(18, 4);

                e.HasOne(x => x.Detail)
                 .WithMany(d => d.Batches)
                 .HasForeignKey(x => x.DetailId)
                 .OnDelete(DeleteBehavior.Cascade);   // delete allocations when line deleted

                e.HasOne(x => x.Batch)
                 .WithMany(b => b.SalesInvoiceDetailBatches)
                 .HasForeignKey(x => x.BatchId)
                 .OnDelete(DeleteBehavior.Restrict);  // don't delete batch when line deleted
            });

            // ── SalesInvoiceDetailSerial ─────────────────────────────────────
            modelBuilder.Entity<SalesInvoiceDetailSerial>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.SerialId).IsUnique(); // a serial can only be sold once

                e.HasOne(x => x.Detail)
                 .WithMany(d => d.Serials)
                 .HasForeignKey(x => x.DetailId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Serial)
                 .WithMany(s => s.SalesInvoiceDetailSerials)
                 .HasForeignKey(x => x.SerialId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── IncomingPaymentMain ─────────────────────────────────────
            modelBuilder.Entity<IncomingPaymentMain>(e =>
            {
                e.HasKey(x => x.PaymentId);

                e.Property(x => x.PaymentNo)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(x => x.TotalAmount)
                    .HasColumnType("decimal(18,2)");
                e.Property(x => x.AllocatedAmount)
                    .HasColumnType("decimal(18,2)");
                e.Property(x => x.OnAccountAmount)
                    .HasColumnType("decimal(18,2)");

                e.Property(x => x.PaymentMode)
                    .HasMaxLength(30)
                    .HasDefaultValue("Cash");

                e.Property(x => x.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("Draft");

                e.HasOne(x => x.BusinessPartner)
                    .WithMany()
                    .HasForeignKey(x => x.BusinessPartnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.DepositAccount)
                    .WithMany()
                    .HasForeignKey(x => x.DepositAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.Allocations)
                    .WithOne(a => a.Payment)
                    .HasForeignKey(a => a.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── IncomingPaymentAllocation ───────────────────────────────
            modelBuilder.Entity<IncomingPaymentAllocation>(e =>
            {
                e.HasKey(x => x.AllocationId);

                e.Property(x => x.AmountApplied)
                    .HasColumnType("decimal(18,2)");

                e.HasOne(x => x.Invoice)
                    .WithMany()
                    .HasForeignKey(x => x.InvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ── PurchaseInvoiceMain ───────────────────────────────────────────────────
            modelBuilder.Entity<PurchaseInvoiceMain>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);

                entity.HasOne(e => e.BusinessPartner)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessPartnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                      .WithMany()
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PurchaseAccount)
                      .WithMany()
                      .HasForeignKey(e => e.PurchaseAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ContactPerson)
                      .WithMany()
                      .HasForeignKey(e => e.ContactPersonId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.BillAddress)
                      .WithMany()
                      .HasForeignKey(e => e.BillAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ShipAddress)
                      .WithMany()
                      .HasForeignKey(e => e.ShipAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Details)
                      .WithOne(d => d.Invoice)
                      .HasForeignKey(d => d.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TaxDetails)
                      .WithOne(t => t.Invoice)
                      .HasForeignKey(t => t.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RoundOff).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetTotal).HasColumnType("decimal(18,2)");
            });

            // ── PurchaseInvoiceDetail ─────────────────────────────────────────────────
            modelBuilder.Entity<PurchaseInvoiceDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId);

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hsn)
                      .WithMany()
                      .HasForeignKey(e => e.HsnId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TaxDetails)
                      .WithOne(t => t.Detail)
                      .HasForeignKey(t => t.DetailId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Batches)
                      .WithOne(b => b.Detail)
                      .HasForeignKey(b => b.DetailId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Serials)
                      .WithOne(s => s.Detail)
                      .HasForeignKey(s => s.DetailId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Qty).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Rate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.AddisDiscountRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AddisDiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
            });

            // ── PurchaseInvoiceTaxDetail ──────────────────────────────────────────────
            modelBuilder.Entity<PurchaseInvoiceTaxDetail>(entity =>
            {
                entity.HasKey(e => e.TaxDetailId);

                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.IGSTPostingAccount)
                      .WithMany()
                      .HasForeignKey(e => e.IGSTPostingAccountId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CGSTPostingAccount)
                      .WithMany()
                      .HasForeignKey(e => e.CGSTPostingAccountId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SGSTPostingAccount)
                      .WithMany()
                      .HasForeignKey(e => e.SGSTPostingAccountId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CessPostingAccount)
                      .WithMany()
                      .HasForeignKey(e => e.CessPostingAccountId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalTaxAmount).HasColumnType("decimal(18,2)");
            });

            // ── PurchaseInvoiceDetailBatch ────────────────────────────────────────────
            modelBuilder.Entity<PurchaseInvoiceDetailBatch>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Batch)
                      .WithMany()
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.Qty).HasColumnType("decimal(18,4)");
            });

            // ── PurchaseInvoiceDetailSerial ───────────────────────────────────────────
            modelBuilder.Entity<PurchaseInvoiceDetailSerial>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Serial)
                      .WithMany()
                      .HasForeignKey(e => e.SerialId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OutgoingPaymentAllocation>(entity =>
            {
                entity.HasKey(e => e.AllocationId);              // ← tell EF which property is the PK
            });


            // ── BillOfMaterial ──────────────────────────────────────
            modelBuilder.Entity<BillOfMaterial>(entity =>
            {
                entity.HasKey(e => e.BomId);

                entity.Property(e => e.OutputQuantity)
                      .HasColumnType("decimal(18,4)");

                entity.HasOne(e => e.FinishedGood)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Lines)
                      .WithOne(l => l.Bom)
                      .HasForeignKey(l => l.BomId)
                      .OnDelete(DeleteBehavior.Cascade);    // lines deleted with BOM
            });

            // ── BomLine ─────────────────────────────────────────────
            modelBuilder.Entity<BomLine>(entity =>
            {
                entity.HasKey(e => e.BomLineId);

                entity.Property(e => e.Quantity)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.ConversionFactor)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.WastagePercent)
                      .HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.Component)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<DocumentCopyLog>(entity =>
            {
                entity.HasKey(e => e.CopyLogId);

                entity.ToTable("DocumentCopyLogs");

                entity.Property(e => e.SourceQty).HasPrecision(18, 4);
                entity.Property(e => e.CopiedQty).HasPrecision(18, 4);

                // Composite index for fast pending-qty queries:
                // "Give me all copies FROM SalesOrder #10, detail #101"
                entity.HasIndex(e => new
                {
                    e.SourceType,
                    e.SourceId,
                    e.SourceDetailId,
                    e.IsDeleted
                }).HasDatabaseName("IX_CopyLog_Source");

                // Index for "what created this invoice line?"
                entity.HasIndex(e => new
                {
                    e.TargetType,
                    e.TargetId,
                    e.TargetDetailId
                }).HasDatabaseName("IX_CopyLog_Target");

                // Index for per-company, per-item pending queries
                entity.HasIndex(e => new
                {
                    e.CompanyId,
                    e.ItemId,
                    e.SourceType,
                    e.IsDeleted
                }).HasDatabaseName("IX_CopyLog_CompanyItem");
            });

            // ════════════════════════════════════════════════════════════
            // Paste this entire block inside OnModelCreating(), just before
            // the closing brace of the method — after the DocumentCopyLog block
            // ════════════════════════════════════════════════════════════

            // ── SalesQuotationMain ───────────────────────────────────────
            modelBuilder.Entity<SalesQuotationMain>(entity =>
            {
                entity.HasKey(e => e.QuotationId);

                entity.Property(e => e.QuotationNo)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Draft");

                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RoundOff).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetTotal).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.BusinessPartner)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessPartnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                      .WithMany()
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ContactPerson)
                      .WithMany()
                      .HasForeignKey(e => e.ContactPersonId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.SalesPerson)
                      .WithMany()
                      .HasForeignKey(e => e.SalesPersonId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.BillAddress)
                      .WithMany()
                      .HasForeignKey(e => e.BillAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ShipAddress)
                      .WithMany()
                      .HasForeignKey(e => e.ShipAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Details)
                      .WithOne(d => d.Quotation)
                      .HasForeignKey(d => d.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.TaxDetails)
                      .WithOne(t => t.Quotation)
                      .HasForeignKey(t => t.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SalesQuotationDetail ─────────────────────────────────────
            modelBuilder.Entity<SalesQuotationDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId);

                entity.Property(e => e.Qty).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Rate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DiscountRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.AddisDiscountRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AddisDiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hsn)
                      .WithMany()
                      .HasForeignKey(e => e.HsnId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TaxDetails)
                      .WithOne(t => t.Detail)
                      .HasForeignKey(t => t.DetailId)
                      .OnDelete(DeleteBehavior.NoAction);   // Cascade already covered by Main → Detail
            });

            // ── SalesQuotationTaxDetail ──────────────────────────────────
            modelBuilder.Entity<SalesQuotationTaxDetail>(entity =>
            {
                entity.HasKey(e => e.TaxDetailId);

                // Explicitly map ONLY the columns that exist in this table
                entity.Property(e => e.TaxDetailId);
                entity.Property(e => e.QuotationId);
                entity.Property(e => e.DetailId);
                entity.Property(e => e.TaxId);
                entity.Property(e => e.IGSTRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CGSTRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.SGSTRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CessRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.TaxableAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SGSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CessAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalTaxAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Quotation)
                      .WithMany(q => q.TaxDetails)
                      .HasForeignKey(e => e.QuotationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Detail)
                      .WithMany(d => d.TaxDetails)
                      .HasForeignKey(e => e.DetailId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ── SalesOrderTaxDetail ──────────────────────────────────────────────
            modelBuilder.Entity<SalesOrderTaxDetail>(entity =>
            {
                entity.HasKey(e => e.TaxDetailId);

                entity.HasOne(e => e.Order)
                      .WithMany(m => m.TaxDetails)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Detail)
                      .WithMany(d => d.TaxDetails)
                      .HasForeignKey(e => e.OrderDetailId)   // ← tells EF to use this column
                      .OnDelete(DeleteBehavior.NoAction);     // Cascade already handled via Order

                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ── SalesOrderDetail ─────────────────────────────────────────────────
            modelBuilder.Entity<SalesOrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId);

                entity.HasOne(e => e.Order)
                      .WithMany(m => m.Details)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SalesOrderMain ───────────────────────────────────────────────────
            modelBuilder.Entity<SalesOrderMain>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.HasOne(e => e.Quotation)
                      .WithMany()
                      .HasForeignKey(e => e.QuotationId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

          
            base.OnModelCreating(modelBuilder);

            // ── GoodsDeliveryDetail ──────────────────────────────────────
            modelBuilder.Entity<GoodsDeliveryDetail>()
                .HasOne(d => d.Delivery)
                .WithMany(m => m.Details)
                .HasForeignKey(d => d.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GoodsDeliveryDetail>()
                .HasOne(d => d.Order)
                .WithMany()
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GoodsDeliveryDetail>()
                .HasOne(d => d.OrderDetail)
                .WithMany()
                .HasForeignKey(d => d.OrderDetailId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GoodsDeliveryDetail>()
                .HasOne(d => d.Item)
                .WithMany()
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GoodsDeliveryDetail>()
                .HasOne(d => d.Hsn)
                .WithMany()
                .HasForeignKey(d => d.HsnId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── GoodsDeliveryTaxDetail ───────────────────────────────────
            modelBuilder.Entity<GoodsDeliveryTaxDetail>()
                .HasOne(t => t.Delivery)
                .WithMany(m => m.TaxDetails)
                .HasForeignKey(t => t.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GoodsDeliveryTaxDetail>()
                .HasOne(t => t.Detail)                   // nav is "Detail"
                .WithMany(d => d.TaxDetails)
                .HasForeignKey(t => t.DeliveryDetailId)  // but FK is "DeliveryDetailId" — EF can't infer this
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GoodsDeliveryTaxDetail>()
                .HasOne(t => t.Tax)
                .WithMany()
                .HasForeignKey(t => t.TaxId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PurchaseOrderMain>(entity =>
            {
                // Prevent EF cascade conflicts on the two address FK's
                entity.HasOne(e => e.BillAddress)
                      .WithMany()
                      .HasForeignKey(e => e.BillAddressId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ShipAddress)
                      .WithMany()
                      .HasForeignKey(e => e.ShipAddressId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.BusinessPartner)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessPartnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                      .WithMany()
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ContactPerson)
                      .WithMany()
                      .HasForeignKey(e => e.ContactPersonId)
                      .OnDelete(DeleteBehavior.Restrict);

                
            });

            // PurchaseOrderDetail ────────────────────────────────────────
            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Details)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Item)
                      .WithMany()
                      .HasForeignKey(e => e.ItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Hsn)
                      .WithMany()
                      .HasForeignKey(e => e.HsnId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseOrderTaxDetail ─────────────────────────────────────
            modelBuilder.Entity<PurchaseOrderTaxDetail>(entity =>
            {
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.TaxDetails)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Detail)
                      .WithMany(d => d.TaxDetails)
                      .HasForeignKey(e => e.OrderDetailId)
                      .OnDelete(DeleteBehavior.NoAction);   // avoid multiple cascade paths

                entity.HasOne(e => e.Tax)
                      .WithMany()
                      .HasForeignKey(e => e.TaxId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OnModelCreating
            modelBuilder.Entity<ProductionOrder>(e =>
            {
                e.HasKey(x => x.ProductionOrderId);

                e.HasOne(x => x.FinishedGood)
                 .WithMany()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Bom)
                 .WithMany()
                 .HasForeignKey(x => x.BomId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Property(x => x.Status)
                 .HasConversion<int>();
            });

            modelBuilder.Entity<ProductionOrderLine>(e =>
            {
                e.HasKey(x => x.ProductionOrderLineId);

                e.HasOne(x => x.ProductionOrder)
                 .WithMany(x => x.Lines)
                 .HasForeignKey(x => x.ProductionOrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Component)
                 .WithMany()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

        }




    }
}
