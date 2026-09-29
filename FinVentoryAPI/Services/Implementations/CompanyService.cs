using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Common = FinVentoryAPI.Helpers.Common;

namespace FinVentoryAPI.Services.Implementations
{
    public class CompanyService: ICompanyService
    {
        private readonly AppDbContext appDbContext;
        private readonly Common _common;
        private readonly ICompanySeedService _seedService;

        public CompanyService(AppDbContext appDbContext, Common common, ICompanySeedService seedService)
        {
            this.appDbContext = appDbContext;
            _common = common;
            _seedService = seedService;
        }

        public async Task<CompanyResponseDto> CreateCompanyAsync(CompanyCreateDto dto, int userId)
        {
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                GSTNumber = dto.GSTNumber,
                PANNumber = dto.PANNumber,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PinCode = dto.PinCode,
                Phone = dto.Phone,
                Mobile = dto.Mobile,
                Email = dto.Email,
                CreatedBy = userId
            };

            appDbContext.Companies.Add(company);
            await appDbContext.SaveChangesAsync();

            // Seed accounting data (AccountGroups, Accounts, Taxes, FinancialYear)
            var seedResult = await _seedService.SeedAllAsync(company.CompanyId, userId);

            // Get the newly created FinancialYear
            var financialYear = await appDbContext.FinancialYears
                .Where(f => f.CompanyId == company.CompanyId && f.IsActive)
                .OrderByDescending(f => f.FinancialYearId)
                .FirstOrDefaultAsync();

            // Find or create an "applAdmin" role
            var adminRole = await appDbContext.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "applAdmin" && r.IsActive);
            if (adminRole == null)
            {
                adminRole = new Role { RoleName = "applAdmin", IsActive = true };
                appDbContext.Roles.Add(adminRole);
                await appDbContext.SaveChangesAsync();
            }

            // Link the creating user to the new company with Admin role
            var existingMapping = await appDbContext.UserCompany
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == company.CompanyId);

            if (existingMapping == null)
            {
                var userCompany = new UserCompany
                {
                    UserId = userId,
                    CompanyId = company.CompanyId,
                    RoleId = adminRole.RoleId,
                    FinancialYearId = financialYear?.FinancialYearId,
                    IsActive = true
                };
                appDbContext.UserCompany.Add(userCompany);
                await appDbContext.SaveChangesAsync();
            }

            // Skip RoleRights for applAdmin — it gets all menus automatically
            if (adminRole.RoleName != "applAdmin")
            {
                await SeedRoleRightsForRoleAsync(adminRole.RoleId, userId);
            }

            // Seed approval configuration (all disabled by default)
            await SeedApprovalConfigAsync(company.CompanyId);

            // Seed default approval levels for all document types
            await SeedApprovalLevelsAsync(company.CompanyId, adminRole.RoleId);

            return MapToResponse(company);
        }

        private async Task SeedRoleRightsForRoleAsync(int roleId, int userId)
        {
            // Skip if RoleRights already exist for this role
            if (await appDbContext.RoleRights.AnyAsync(r => r.RoleId == roleId))
                return;

            // Get all active MenuItems
            var menuItems = await appDbContext.MenuItems
                .Include(mi => mi.MenuGroup)
                .Include(mi => mi.Module)
                .Where(mi => mi.IsActive && mi.MenuGroup.IsActive && mi.Module.IsActive)
                .ToListAsync();

            if (!menuItems.Any())
                return;

            var rights = menuItems.Select(mi => new RoleRight
            {
                RoleId = roleId,
                ModuleId = mi.ModuleId,
                MenuItemId = mi.MenuItemId,
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                CanPrint = true,
                CanExport = true,
                CanApprove = true,
                GrantedBy = userId,
                GrantedAt = DateTime.UtcNow
            }).ToList();

            appDbContext.RoleRights.AddRange(rights);
            await appDbContext.SaveChangesAsync();
        }

        private async Task SeedApprovalConfigAsync(int companyId)
        {
            // Master toggle
            var masterKey = "ApprovalSystemEnabled";
            var masterExists = await appDbContext.CompanyConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.ConfigKey == masterKey);
            if (masterExists == null)
            {
                appDbContext.CompanyConfigs.Add(new CompanyConfig
                {
                    CompanyId = companyId,
                    ConfigKey = masterKey,
                    ConfigValue = "false",
                    ConfigType = "boolean",
                    Description = "Enable or disable the entire approval system",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // All document types
            var documentTypes = GetAllApprovalDocumentTypes();

            foreach (var docType in documentTypes)
            {
                var configKey = $"ApprovalRequired_{docType}";
                var existing = await appDbContext.CompanyConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.ConfigKey == configKey);

                if (existing == null)
                {
                    appDbContext.CompanyConfigs.Add(new CompanyConfig
                    {
                        CompanyId = companyId,
                        ConfigKey = configKey,
                        ConfigValue = "false",
                        ConfigType = "boolean",
                        Description = $"Enable approval workflow for {docType}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await appDbContext.SaveChangesAsync();
        }

        private static string[] GetAllApprovalDocumentTypes() => new[]
        {
            // Sales
            "SalesQuotation", "SalesOrder", "GoodsDelivery", "SalesInvoice", "SalesReturn",
            // Purchase
            "PurchaseOrder", "GRN", "PurchaseInvoice", "PurchaseReturn",
            // Inventory
            "StockTransfer", "StockAdjustment",
            // Production
            "ProductionIssue", "ProductionReceipt",
            // Job Work
            "JobWorkIssue", "JobWorkReceipt",
            // Finance
            "JournalEntry", "CashBankEntry", "IncomingPayment", "OutgoingPayment"
        };

        private async Task SeedApprovalLevelsAsync(int companyId, int adminRoleId)
        {
            // Check if approval levels already exist
            if (await appDbContext.ApprovalLevels.AnyAsync(a => a.CompanyId == companyId))
                return;

            var documentTypes = GetAllApprovalDocumentTypes();

            foreach (var docType in documentTypes)
            {
                appDbContext.ApprovalLevels.Add(new ApprovalLevel
                {
                    CompanyId = companyId,
                    DocumentType = docType,
                    LevelNumber = 1,
                    LevelName = "Manager",
                    MaxAmount = null,
                    RoleId = adminRoleId,
                    IsActive = false, // Disabled by default, admin can enable
                    CreatedAt = DateTime.UtcNow
                });
            }

            await appDbContext.SaveChangesAsync();
        }

        public async Task<List<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await appDbContext.Companies
       .Where(c => !c.IsDeleted)
       .ToListAsync();          // 👈 fetch first, then map in memory

            return companies.Select(c => MapToResponse(c)).ToList();
            //return await appDbContext.Companies
            //    .Where(c => !c.IsDeleted)
            //    .Select(c => new CompanyResponseDto
            //    {
            //        CompanyId = c.CompanyId,
            //        CompanyName = c.CompanyName,
            //        GSTNumber = c.GSTNumber,
            //        PANNumber = c.PANNumber,
            //        Address = c.Address,
            //        City = c.City,
            //        State = c.State,
            //        StateName = c.State.HasValue ? EnumHelper.GetStateName((int)c.State.Value) : null,
            //        StateCode = c.State.HasValue ? ((int)c.State.Value).ToString("D2") : null,
            //        PinCode = c.PinCode,
            //        Phone = c.Phone,
            //        Mobile = c.Mobile,
            //        Email = c.Email,
            //        IsActive = c.IsActive
            //    })
            //    .ToListAsync();
        }

        public async Task<CompanyResponseDto> GetByIdAsync(int id)
        {
            var company = await appDbContext.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == id && !c.IsDeleted);

            if (company == null)
                return null;

            return MapToResponse(company);
        }

        public async Task<bool> UpdateCompanyAsync(int id, CompanyUpdateDto dto, int userId)
        {
            var company = await appDbContext.Companies               
                .FirstOrDefaultAsync(c => c.CompanyId == id && !c.IsDeleted);

            if (company == null)
                return false;

            company.CompanyName = dto.CompanyName;
            company.GSTNumber = dto.GSTNumber;
            company.PANNumber = dto.PANNumber;
            company.Address = dto.Address;
            company.City = dto.City;
            company.State = dto.State;
            company.PinCode = dto.PinCode;
            company.Phone = dto.Phone;
            company.Mobile = dto.Mobile;
            company.Email = dto.Email;
            company.IsActive = dto.IsActive;

            company.UpdatedDate = DateTime.UtcNow;
            company.UpdatedBy = userId;

            await appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCompanyAsync(int id, int userId)
        {
            var company = await appDbContext.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == id && !c.IsDeleted);

            if (company == null)
                return false;

            company.IsDeleted = true;
            company.UpdatedDate = DateTime.UtcNow;
            company.UpdatedBy = userId;

            await appDbContext.SaveChangesAsync();

            return true;
        }
        public async Task<CompanyStateDto> GetCompanyStateAsync()
        {
            var companyId = _common.GetCompanyId();

            var company = await appDbContext.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && !c.IsDeleted);

            if (company == null)
                throw new Exception("Company not found.");

            return new CompanyStateDto
            {
                StateCode = company.State.HasValue ? (int?)((int)company.State.Value) : null,
                StateName = company.StateName
            };
        }

        private CompanyResponseDto MapToResponse(Company c)
        {
            return new CompanyResponseDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                GSTNumber = c.GSTNumber,
                PANNumber = c.PANNumber,
                Address = c.Address,
                City = c.City,
                State = c.State,
                StateName = c.State.HasValue ? EnumHelper.GetStateName((int)c.State.Value) : null,
                StateCode = c.State.HasValue ? ((int)c.State.Value).ToString("D2") : null,
                PinCode = c.PinCode,
                Phone = c.Phone,
                Mobile = c.Mobile,
                Email = c.Email,
                Logo = c.Logo,
                IsActive = c.IsActive
            };
        }

        public async Task<bool> UpdateLogoAsync(int companyId, string? logoPath)
        {
            var company = await appDbContext.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && !c.IsDeleted);

            if (company == null)
                return false;

            company.Logo = logoPath;
            company.UpdatedDate = DateTime.UtcNow;
            await appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
