using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.ModuleDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class ModuleService : IModuleService
    {
        private readonly AppDbContext _context;
        public ModuleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ModuleResponseDto> CreateAsync(ModuleCreateDto dto)
        {
            if (await _context.Modules.AnyAsync(x => x.ModuleName == dto.ModuleName))
                throw new Exception("Module already exists");

            var module = new Module
            {
               ModuleName = dto.ModuleName,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder,
                //IsActive = true
            };

            _context.Modules.Add(module);
            await _context.SaveChangesAsync();

            return MapToResponse(module);
        }

        public async Task<bool> UpdateAsync(int id, ModuleUpdateDto dto)
        {
            var module = await _context.Modules
                .FirstOrDefaultAsync(x => x.ModuleId == id && x.IsActive);

            if (module == null)
                return false;

            var duplicate = await _context.Modules
                .AnyAsync(x =>
                    x.ModuleName.ToLower() == dto.ModuleName.ToLower() &&
                    x.ModuleId != id &&
                    x.IsActive);

            if (duplicate)
                throw new Exception("Modul already exists");

            module.ModuleName = dto.ModuleName;
            module.Icon = dto.Icon;
            module.SortOrder = dto.SortOrder;
            module.IsActive = dto.IsActive;
           
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ModuleResponseDto>> GetAllAsync()
        {
            return await _context.Modules
                .Where(x => x.IsActive)
                .Select(x => new ModuleResponseDto
                {
                    ModuleId = x.ModuleId,
                    ModuleName = x.ModuleName,
                    Icon = x.Icon,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive
                }).ToListAsync();
        }

        public async Task<ModuleResponseDto?> GetByIdAsync(int id)
        {
            var module = await _context.Modules
               .FirstOrDefaultAsync(x => x.ModuleId == id && x.IsActive);
            if (module == null)
                return null;
            return MapToResponse(module);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var location = await _context.Modules
                .FirstOrDefaultAsync(x => x.ModuleId == id && x.IsActive);
            if (location == null)
                return false;
            location.IsActive = false;
          
            await _context.SaveChangesAsync();
            return true;
        }

        private ModuleResponseDto MapToResponse(Module  module)
        {
            return new ModuleResponseDto
            {
               ModuleId = module.ModuleId,
               ModuleName = module.ModuleName,
               Icon = module.Icon,
               SortOrder = module.SortOrder,
               IsActive = module.IsActive
            };
        }
    }
}
