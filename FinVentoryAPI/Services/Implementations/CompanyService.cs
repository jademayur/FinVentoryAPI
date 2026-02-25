using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FinVentoryAPI.Services.Implementations
{
    public class CompanyService: ICompanyService
    {
        private readonly AppDbContext appDbContext;

        public CompanyService(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
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

            return MapToResponse(company);
        }

        public async Task<List<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            return await appDbContext.Companies
                .Where(c => !c.IsDeleted)
                .Select(c => new CompanyResponseDto
                {
                    CompanyId = c.CompanyId,      // or c.Id (use your actual PK)
                    CompanyName = c.CompanyName,
                    GSTNumber = c.GSTNumber,
                    PANNumber = c.PANNumber,
                    Address = c.Address,
                    City = c.City,
                    State = c.State,
                    PinCode = c.PinCode,
                    Phone = c.Phone,
                    Mobile = c.Mobile,
                    Email = c.Email,
                    IsActive = c.IsActive
                })
                .ToListAsync();
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

        private CompanyResponseDto MapToResponse(Company c)
        {
            return new CompanyResponseDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                GSTNumber = c.GSTNumber,
                PANNumber = c.PANNumber,
                City = c.City,
                State = c.State,
                Email = c.Email,
                IsActive = c.IsActive
            };
        }
    }
}
