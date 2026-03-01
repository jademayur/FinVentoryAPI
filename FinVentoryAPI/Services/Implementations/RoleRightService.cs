using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.LocationDTOs;
using FinVentoryAPI.DTOs.MenuItemDTOs;
using FinVentoryAPI.DTOs.RoleDTOs;
using FinVentoryAPI.DTOs.RoleRightsDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Migrations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class RoleRightService : IRoleRightService
    {
        private readonly AppDbContext _context;
        public RoleRightService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoleRightResponseDto> CreateAsync(RoleRightCreateDto dto)
        {            

            var roleRight = new RoleRight
            {
                RoleId = dto.RoleId,
                ModuleId = dto.ModuleId,
                MenuItemId = dto.MenuItemId,
                CanAdd = dto.CanAdd,
                CanEdit = dto.CanEdit,
                CanDelete = dto.CanDelete,
                CanView = dto.CanView,
                CanPrint = dto.CanPrint,
                CanExport = dto.CanExport,
                CanApprove = dto.CanApprove,
                GrantedBy = dto.GrantedBy,
                GrantedAt = DateTime.UtcNow

            };

            _context.RoleRights.Add(roleRight);
            await _context.SaveChangesAsync();

            return MapToResponse(roleRight);
        }

        public async Task<bool> DeleteAsync(int id,int UserId)
        {
            var roleRight = await _context.RoleRights
                .FirstOrDefaultAsync(x => x.RoleRightId == id);
            if (roleRight == null)
                return false;

            roleRight.GrantedBy = UserId;
            roleRight.GrantedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RoleRightResponseDto>> GetAllAsync()
        {
            return await _context.RoleRights
                .Include(x => x.MenuItem)   // 👈 Important
                .Select(x => new RoleRightResponseDto
                {
                    RoleRightId = x.RoleRightId,
                    RoleId = x.RoleId,
                    ModuleId = x.ModuleId,
                    MenuItemId = x.MenuItemId,
                    MenuItemName = x.MenuItem.MenuName,  // 👈 FIX

                    CanAdd = x.CanAdd,
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    CanView = x.CanView,
                    CanPrint = x.CanPrint,
                    CanExport = x.CanExport,
                    CanApprove = x.CanApprove,
                    GrantedBy = x.GrantedBy,
                    GrantedAt = x.GrantedAt
                })
                .ToListAsync();
        }

        public async Task<RoleRightResponseDto> GetByIdAsync(int id)
        {
            var roleRight = await _context.RoleRights              
               .FirstOrDefaultAsync(x => x.RoleRightId == id );
            if (roleRight == null)
                return null;
            return MapToResponse(roleRight);
        }

        public async Task<bool> UpdateAsync(int id, RoleRightUpdateDto dto)
        {
            var roleRight = await _context.RoleRights
                .FirstOrDefaultAsync(x => x.RoleRightId == id);

            if (roleRight == null)
                return false;

   
            roleRight.RoleRightId = id;
            roleRight.RoleId = dto.RoleId;
            roleRight.ModuleId = dto.ModuleId;
            roleRight.MenuItemId = dto.MenuItemId;
            roleRight.CanAdd = dto.CanAdd;
            roleRight.CanEdit = dto.CanEdit;
            roleRight.CanDelete = dto.CanDelete;
            roleRight.CanView = dto.CanView;
            roleRight.CanPrint = dto.CanPrint;
            roleRight.CanExport = dto.CanExport;
            roleRight.CanApprove = dto.CanApprove;
            roleRight.GrantedBy = dto.GrantedBy;
            roleRight.GrantedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<MenuItemResponseDto>> GetMenuByRoleAsync(int roleId)
        {
            return await _context.RoleRights
                .Where(r => r.RoleId == roleId
                            && r.CanView
                            && r.MenuItem.IsActive
                            && r.MenuItem.MenuGroup.IsActive
                            && r.MenuItem.Module.IsActive)
                .Select(r => new MenuItemResponseDto
                {
                    // Menu Item
                    MenuItemId = r.MenuItem.MenuItemId,
                    MenuName = r.MenuItem.MenuName,
                    MenuItemIcon = r.MenuItem.Icon,
                    MenuItemSortOrder = r.MenuItem.SortOrder,
                    MenuItemIsActive = r.MenuItem.IsActive,

                    // Module
                    ModuleId = r.MenuItem.ModuleId,
                    ModuleName = r.MenuItem.Module.ModuleName,
                    ModuleIcon = r.MenuItem.Module.Icon,
                    ModuleSortOrder = r.MenuItem.Module.SortOrder,
                    ModuleIsActive = r.MenuItem.Module.IsActive,

                    // Menu Group
                    MenuGroupId = r.MenuItem.MenuGroupId,
                    MenuGroupName = r.MenuItem.MenuGroup.MenuGroupName,
                    MenuGroupIcon = r.MenuItem.MenuGroup.Icon,
                    MenuGroupSortOrder = r.MenuItem.MenuGroup.SortOrder,
                    MenuGroupIsActive = r.MenuItem.MenuGroup.IsActive,

                    ControllerName = r.MenuItem.ControllerName,
                    ActionName = r.MenuItem.ActionName
                })
                .Distinct()
                .OrderBy(x => x.ModuleSortOrder)
                .ThenBy(x => x.MenuGroupSortOrder)
                .ThenBy(x => x.MenuItemSortOrder)
                .ToListAsync();
        }

        private RoleRightResponseDto MapToResponse(RoleRight roleRight)
        {
            return new RoleRightResponseDto
            {
                RoleRightId = roleRight.RoleRightId,
                RoleId = roleRight.RoleId,
                ModuleId = roleRight.ModuleId,
                MenuItemId = roleRight.MenuItemId,
                CanAdd = roleRight.CanAdd,
                CanEdit = roleRight.CanEdit,
                CanDelete = roleRight.CanDelete,
                CanView = roleRight.CanView,
                CanPrint = roleRight.CanPrint,
                CanExport = roleRight.CanExport,
                CanApprove = roleRight.CanApprove,
                GrantedBy = roleRight.GrantedBy,
                GrantedAt = roleRight.GrantedAt,

            };
        }
    }
}
