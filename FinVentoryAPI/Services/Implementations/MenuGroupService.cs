using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.MenuGroupDTOs;
using FinVentoryAPI.DTOs.ModuleDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class MenuGroupService : IMenuGroupService
    {
        private readonly AppDbContext _context;
        public MenuGroupService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<MenuGroupResponseDto>> GetAllAsync()
        {
            return await _context.MenuGroups
               .Where(x => x.IsActive)
               .Select(x => new MenuGroupResponseDto
               {
                   MenuGroupId = x.MenuGroupId,
                   ModuleId = x.ModuleId,
                   MenuGroupName = x.MenuGroupName,
                   Icon = x.Icon,
                   SortOrder = x.SortOrder,
                   IsActive = x.IsActive
               }).ToListAsync();
        }

        public async Task<MenuGroupResponseDto?> GetByIdAsync(int id)
        {
            var module = await _context.MenuGroups
                .FirstOrDefaultAsync(x => x.MenuGroupId == id && x.IsActive);
            if (module == null)
                return null;
            return MapToResponse(module);
        }

        private MenuGroupResponseDto MapToResponse(MenuGroup module)
        {
            return new MenuGroupResponseDto
            {
                MenuGroupId = module.MenuGroupId,
                ModuleId = module.ModuleId,
                MenuGroupName = module.MenuGroupName,
                Icon = module.Icon,
                SortOrder = module.SortOrder,
                IsActive = module.IsActive
            };
        }
    }
}
