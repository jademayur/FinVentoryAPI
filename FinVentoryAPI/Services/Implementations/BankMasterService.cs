using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.BankDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class BankMasterService : IBankMasterService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;

        public BankMasterService(AppDbContext context, Common common)
        {
            _context = context;
            _common = common;
        }

        public async Task<BankMasterResponseDto> CreateAsync(CreateBankMasterDto dto)
        {
            var companyId = _common.GetCompanyId();  // ✅ from JWT

            if (await _context.Bank.AnyAsync(x =>
                    x.CompanyId == companyId &&       // ✅ scoped to company
                    x.AccountNo == dto.AccountNo &&
                    !x.IsDeleted))
                throw new Exception("Bank account number already exists.");

            var bank = new Bank
            {
                BankName = dto.BankName,
                Branch = dto.Branch,
                AccountNo = dto.AccountNo,
                SwiftCode = dto.SwiftCode,
                IFSCCode = dto.IFSCCode,
                CompanyId = companyId,               // ✅ from JWT, not DTO
                CreatedBy = _common.GetUserId(),     // ✅ from JWT
                CreatedDate = DateTime.UtcNow,
            };

            _context.Bank.Add(bank);
            await _context.SaveChangesAsync();

            return MapToResponse(bank);
        }

        public async Task<bool> UpdateAsync(int id, UpdateBankMasterDto dto)
        {
            var companyId = _common.GetCompanyId();  // ✅ from JWT

            var bank = await _context.Bank
                .FirstOrDefaultAsync(x =>
                    x.BankId == id &&
                    x.CompanyId == companyId &&      // ✅ scoped to company
                    !x.IsDeleted);

            if (bank == null)
                return false;

            var duplicate = await _context.Bank
                .AnyAsync(x =>
                    x.CompanyId == companyId &&      // ✅ scoped to company
                    x.AccountNo == dto.AccountNo &&
                    x.BankId != id &&
                    !x.IsDeleted);

            if (duplicate)
                throw new Exception("Bank account number already exists.");

            bank.BankName = dto.BankName;
            bank.Branch = dto.Branch;
            bank.AccountNo = dto.AccountNo;
            bank.SwiftCode = dto.SwiftCode;
            bank.IFSCCode = dto.IFSCCode;
            bank.IsActive = dto.IsActive;
            bank.ModifiedBy = _common.GetUserId();   // ✅ from JWT
            bank.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BankMasterResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();  // ✅ from JWT

            return await _context.Bank
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)  // ✅ scoped to company
                .Select(x => new BankMasterResponseDto
                {
                    BankId = x.BankId,
                    BankName = x.BankName,
                    Branch = x.Branch,
                    AccountNo = x.AccountNo,
                    SwiftCode = x.SwiftCode ?? string.Empty,
                    IFSCCode = x.IFSCCode,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<BankMasterResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();  // ✅ from JWT

            var bank = await _context.Bank
                .FirstOrDefaultAsync(x =>
                    x.BankId == id &&
                    x.CompanyId == companyId &&      // ✅ scoped to company
                    !x.IsDeleted);

            if (bank == null)
                return null;

            return MapToResponse(bank);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();  // ✅ from JWT

            var bank = await _context.Bank
                .FirstOrDefaultAsync(x =>
                    x.BankId == id &&
                    x.CompanyId == companyId &&      // ✅ scoped to company
                    !x.IsDeleted);

            if (bank == null)
                return false;

            bank.IsDeleted = true;
            bank.IsActive = false;
            bank.ModifiedBy = _common.GetUserId();   // ✅ from JWT
            bank.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private BankMasterResponseDto MapToResponse(Bank bank)
        {
            return new BankMasterResponseDto
            {
                BankId = bank.BankId,
                BankName = bank.BankName,
                Branch = bank.Branch,
                AccountNo = bank.AccountNo,
                SwiftCode = bank.SwiftCode ?? string.Empty,
                IFSCCode = bank.IFSCCode,
                IsActive = bank.IsActive,
                CreatedDate = bank.CreatedDate
            };
        }
    }
}