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




        }

 



    }
}
