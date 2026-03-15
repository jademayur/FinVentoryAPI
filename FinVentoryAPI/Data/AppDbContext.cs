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
        }




    }
}
