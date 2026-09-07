using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CompanyConfigDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class CompanyConfigService : ICompanyConfigService
    {
        private readonly AppDbContext _context;

        public CompanyConfigService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyConfigDto>> GetAllAsync(int companyId)
        {
            return await _context.CompanyConfigs
                .Where(c => c.CompanyId == companyId)
                .Select(c => new CompanyConfigDto
                {
                    ConfigId = c.ConfigId,
                    ConfigKey = c.ConfigKey,
                    ConfigValue = c.ConfigValue,
                    ConfigType = c.ConfigType,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<CompanyConfigDto?> GetByKeyAsync(int companyId, string key)
        {
            return await _context.CompanyConfigs
                .Where(c => c.CompanyId == companyId && c.ConfigKey == key)
                .Select(c => new CompanyConfigDto
                {
                    ConfigId = c.ConfigId,
                    ConfigKey = c.ConfigKey,
                    ConfigValue = c.ConfigValue,
                    ConfigType = c.ConfigType,
                    Description = c.Description
                })
                .FirstOrDefaultAsync();
        }

        public async Task<string?> GetValueAsync(int companyId, string key)
        {
            return await _context.CompanyConfigs
                .Where(c => c.CompanyId == companyId && c.ConfigKey == key)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();
        }

        public async Task UpsertAsync(int companyId, UpsertCompanyConfigDto dto)
        {
            var existing = await _context.CompanyConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.ConfigKey == dto.ConfigKey);

            if (existing != null)
            {
                existing.ConfigValue = dto.ConfigValue;
                existing.ConfigType = dto.ConfigType;
                existing.Description = dto.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.CompanyConfigs.Add(new CompanyConfig
                {
                    CompanyId = companyId,
                    ConfigKey = dto.ConfigKey,
                    ConfigValue = dto.ConfigValue,
                    ConfigType = dto.ConfigType,
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task BulkUpsertAsync(int companyId, BulkUpsertCompanyConfigDto dto)
        {
            foreach (var item in dto.Configs)
            {
                var existing = await _context.CompanyConfigs
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.ConfigKey == item.ConfigKey);

                if (existing != null)
                {
                    existing.ConfigValue = item.ConfigValue;
                    existing.ConfigType = item.ConfigType;
                    existing.Description = item.Description;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.CompanyConfigs.Add(new CompanyConfig
                    {
                        CompanyId = companyId,
                        ConfigKey = item.ConfigKey,
                        ConfigValue = item.ConfigValue,
                        ConfigType = item.ConfigType,
                        Description = item.Description,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int companyId, int configId)
        {
            var config = await _context.CompanyConfigs
                .FirstOrDefaultAsync(c => c.ConfigId == configId && c.CompanyId == companyId);

            if (config != null)
            {
                _context.CompanyConfigs.Remove(config);
                await _context.SaveChangesAsync();
            }
        }
    }
}
