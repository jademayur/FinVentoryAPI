using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.MenuItemDTOs;
using FinVentoryAPI.DTOs.ModuleDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class MenuItemService : IMenuItemService
    {
        private readonly AppDbContext _context;
        public MenuItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItemResponseDto> CreateAsync(MenuItemCreateDto dto)
        {

            if (await _context.MenuItems.AnyAsync(x => x.MenuName == dto.MenuName))
                throw new Exception("Menu Item already exists");

            var menuItem = new MenuItem
            {
                MenuName = dto.MenuName,
                MenuGroupId = dto.MenuGroupId,
                ModuleId = dto.ModuleId,
                ControllerName = dto.ControllerName,
                ActionName = dto.ActionName,
                Icon = dto.Icon,
                SortOrder = dto.SortOrder,
                IsActive = true
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return MapToResponse(menuItem);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var location = await _context.MenuItems
                 .FirstOrDefaultAsync(x => x.MenuItemId == id && x.IsActive);
            if (location == null)
                return false;
            location.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MenuItemResponseDto>> GetAllAsync()
        {
            return await _context.MenuItems
                .Where(x => x.IsActive)
                .Select(x => new MenuItemResponseDto
                {
                    MenuItemId = x.MenuItemId,
                    ModuleId = x.ModuleId,                   
                    MenuGroupId = x.MenuGroupId,
                    MenuName = x.MenuName,
                    ControllerName = x.ControllerName,
                    ActionName = x.ActionName,
                    MenuItemIcon = x.Icon,
                    MenuItemSortOrder = x.SortOrder,
                    MenuItemIsActive = x.IsActive
                }).ToListAsync();
        }

        public async Task<MenuItemResponseDto?> GetByIdAsync(int id)
        {
            var menuItem = await _context.MenuItems
              .FirstOrDefaultAsync(x => x.MenuItemId == id && x.IsActive);
            if (menuItem == null)
                return null;
            return MapToResponse(menuItem);
        }

        public async Task<bool> UpdateAsync(int id, MenuItemUpdateDto dto)
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(x => x.MenuItemId == id && x.IsActive);

            if (menuItem == null)
                return false;

            var duplicate = await _context.MenuItems
                .AnyAsync(x =>
                    x.MenuName.ToLower() == dto.MenuName.ToLower() &&
                    x.MenuItemId != id &&
                    x.IsActive);

            if (duplicate)
                throw new Exception("Menu Item already exists");

            menuItem.MenuGroupId = dto.MenuGroupId;
            menuItem.ModuleId = dto.ModuleId;
            menuItem.MenuName = dto.MenuName;
            menuItem.ControllerName = dto.ControllerName;
            menuItem.ActionName = dto.ActionName;
            menuItem.Icon = dto.Icon;
            menuItem.SortOrder = dto.SortOrder;
            menuItem.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return true;
        }

        private MenuItemResponseDto MapToResponse(MenuItem menuItem)
        {
            return new MenuItemResponseDto
            {
                MenuItemId = menuItem.MenuItemId,
                ModuleId = menuItem.ModuleId,
                MenuGroupId = menuItem.MenuGroupId,
                MenuName = menuItem.MenuName,
                ControllerName = menuItem.ControllerName,
                ActionName = menuItem.ActionName,
                MenuItemIcon = menuItem.Icon,
                MenuGroupSortOrder = menuItem.SortOrder,
                MenuItemIsActive = menuItem.IsActive
            };
        }
    }
}
