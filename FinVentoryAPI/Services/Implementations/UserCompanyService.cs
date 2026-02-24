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

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.UserCompanies
                .Include(x => x.User)
                .Include(x => x.Company)
                .Include(x => x.Role)
                .Select(x => new
                {
                    x.UserCompanyId,
                    User = x.User.FullName,
                    Company = x.Company.CompanyName,
                    Role = x.Role.RoleName
                })
                .ToListAsync();
        }

        public async Task<object> GetByUserAsync(int userId)
        {
            return await _context.UserCompanies
                .Include(x => x.Company)
                .Include(x => x.Role)
                .Where(x => x.UserId == userId)
                .Select(x => new
                {
                    x.UserCompanyId,
                    x.CompanyId,
                    CompanyName = x.Company.CompanyName,
                    Role = x.Role.RoleName
                })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(UserCompanyCreateDto dto)
        {
            var exists = await _context.UserCompanies
                .AnyAsync(x => x.UserId == dto.UserId &&
                               x.CompanyId == dto.CompanyId);

            if (exists)
                return "User already assigned to this company";

            var entity = new UserCompany
            {
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                RoleId = dto.RoleId
              
            };

            _context.UserCompanies.Add(entity);
            await _context.SaveChangesAsync();

            return "User assigned to company successfully";
        }

        public async Task<string> UpdateAsync(UserCompanyUpdateDto dto)
        {
            var entity = await _context.UserCompanies
                .FindAsync(dto.UserCompanyId);

            if (entity == null)
                return "Record not found";

            entity.RoleId = dto.RoleId;
          //  entity.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return "User company mapping updated";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var entity = await _context.UserCompanies.FindAsync(id);

            if (entity == null)
                return "Record not found";

            //entity.IsActive = false;

            await _context.SaveChangesAsync();

            return "Mapping deleted successfully";
        }
    }
}
