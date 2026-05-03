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
        }

 



    }
}
