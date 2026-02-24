using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.FinancialYearDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class FinancialYearService : IFinancialYearService
    {
        private readonly AppDbContext _context;

        public FinancialYearService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetByCompanyAsync(int companyId)
        {
            return await _context.FinancialYears
                .Where(x => x.CompanyId == companyId)
                .Select(x => new
                {
                    x.FinancialYearId,
                    x.YearName,
                    x.StartDate,
                    x.EndDate,
                    x.IsActive,
                    x.IsClosed
                })
                .ToListAsync();
        }

        public async Task<string> CreateAsync(CreateFinancialYearDto dto)
        {
            var exists = await _context.FinancialYears
                .AnyAsync(x => x.CompanyId == dto.CompanyId &&
                               x.YearName == dto.YearName);

            if (exists)
                return "Financial year already exists for this company";

            // Deactivate old active year
            var activeYears = await _context.FinancialYears
                .Where(x => x.CompanyId == dto.CompanyId && x.IsActive)
                .ToListAsync();

            foreach (var year in activeYears)
            {
                year.IsActive = false;
            }

            var entity = new FinancialYear
            {
                CompanyId = dto.CompanyId,
                YearName = dto.YearName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedBy = dto.CreatedBy,
                IsActive = true
            };

            _context.FinancialYears.Add(entity);
            await _context.SaveChangesAsync();

            return "Financial year created successfully";
        }

        public async Task<string> UpdateAsync(UpdateFinancialYearDto dto)
        {
            var entity = await _context.FinancialYears
                .FindAsync(dto.FinancialYearId);

            if (entity == null)
                return "Record not found";

            if (entity.IsClosed)
                return "Closed financial year cannot be updated";

            var duplicate = await _context.FinancialYears
                .AnyAsync(x => x.CompanyId == entity.CompanyId &&
                               x.YearName == dto.YearName &&
                               x.FinancialYearId != dto.FinancialYearId);

            if (duplicate)
                return "Financial year already exists for this company";

            entity.YearName = dto.YearName;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.UpdatedBy = dto.UpdatedBy;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return "Financial year updated successfully";
        }

        public async Task<string> CloseAsync(int id)
        {
            var entity = await _context.FinancialYears.FindAsync(id);

            if (entity == null)
                return "Record not found";

            entity.IsClosed = true;
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return "Financial year closed successfully";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var entity = await _context.FinancialYears.FindAsync(id);

            if (entity == null)
                return "Record not found";

            _context.FinancialYears.Remove(entity);
            await _context.SaveChangesAsync();

            return "Financial year deleted successfully";
        }
    }
}
