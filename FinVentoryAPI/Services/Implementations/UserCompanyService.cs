using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.UserCompany;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class UserCompanyService : IUserCompanyService
    {
        private readonly AppDbContext _context;

        public UserCompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserCompanyResponseDto>> GetAllAsync()
        {
            return await _context.UserCompany
                .Include(x => x.User)
                .Include(x => x.Company)
                .Include(x => x.FinancialYear)
                .Include(x => x.Role)
                .Select(x => new UserCompanyResponseDto
                {
                    UserCompanyId = x.UserCompanyId,

                    UserId = x.UserId,
                    UserName = x.User.FullName,

                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName,

                    FinancialYearId = (int) x.FinancialYearId,
                    FinancialYearName = x.FinancialYear.YearName,

                    RoleId = x.RoleId,
                    RoleName = x.Role.RoleName
                })
                .ToListAsync();
        }

        public async Task<UserCompanyResponseDto?> CreateAsync(UserCompanyCreateDto dto)
        {
            var exists = await _context.UserCompany.AnyAsync(x =>
                x.UserId == dto.UserId &&
                x.CompanyId == dto.CompanyId &&
                x.FinancialYearId == dto.FinancialYearId);

            if (exists)
                throw new Exception("Access already assigned.");

            var entity = new UserCompany
            {
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                FinancialYearId = dto.FinancialYearId,
                RoleId = dto.RoleId
            };

            _context.UserCompany.Add(entity);
            await _context.SaveChangesAsync();

            return await _context.UserCompany
                .Include(x => x.User)
                .Include(x => x.Company)
                .Include(x => x.FinancialYear)
                .Include(x => x.Role)
                .Where(x => x.UserCompanyId == entity.UserCompanyId)
                .Select(x => new UserCompanyResponseDto
                {
                    UserCompanyId = x.UserCompanyId,

                    UserId = x.UserId,
                    UserName = x.User.FullName,

                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName,

                    FinancialYearId = (int)x.FinancialYearId,
                    FinancialYearName = x.FinancialYear.YearName,

                    RoleId = x.RoleId,
                    RoleName = x.Role.RoleName
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateAsync(int id, UserCompanyCreateDto dto)
        {
            var entity = await _context.UserCompany
                .FirstOrDefaultAsync(x => x.UserCompanyId == id);

            if (entity == null)
                return false;

            var duplicateExists = await _context.UserCompany.AnyAsync(x =>
                x.UserCompanyId != id &&
                x.UserId == dto.UserId &&
                x.CompanyId == dto.CompanyId &&
                x.FinancialYearId == dto.FinancialYearId);

            if (duplicateExists)
                throw new Exception("Access already assigned.");

            entity.UserId = dto.UserId;
            entity.CompanyId = dto.CompanyId;
            entity.FinancialYearId = dto.FinancialYearId;
            entity.RoleId = dto.RoleId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.UserCompany
                .FirstOrDefaultAsync(x => x.UserCompanyId == id);

            if (entity == null)
                return false;

            _context.UserCompany.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> BulkCreateAsync(UserCompanyBulkCreateDto dto)
        {
            int inserted = 0;

            foreach (var assignment in dto.Assignments)
            {
                foreach (var yearId in assignment.FinancialYearIds)
                {
                    bool exists = await _context.UserCompany.AnyAsync(x =>
                        x.UserId == dto.UserId &&
                        x.CompanyId == assignment.CompanyId &&
                        x.FinancialYearId == yearId);

                    if (exists) continue; // skip duplicates silently

                    _context.UserCompany.Add(new UserCompany
                    {
                        UserId = dto.UserId,
                        CompanyId = assignment.CompanyId,
                        FinancialYearId = yearId,
                        RoleId = dto.RoleId
                    });

                    inserted++;
                }
            }

            await _context.SaveChangesAsync();
            return inserted;
        }
    }
}
