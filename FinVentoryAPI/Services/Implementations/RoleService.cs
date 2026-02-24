using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.RoleDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(x => x.RoleId == id && x.IsActive);
        }

        public async Task<string> CreateAsync(RoleCreateDto dto)
        {
            if (await _context.Roles.AnyAsync(x => x.RoleName == dto.RoleName))
                return "Role already exists";

            var role = new Role
            {
                RoleName = dto.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return "Role created successfully";
        }

        public async Task<string> UpdateAsync(RoleUpdateDto dto)
        {
            var role = await _context.Roles.FindAsync(dto.RoleId);

            if (role == null)
                return "Role not found";

            role.RoleName = dto.RoleName;
            role.IsActive = dto.IsActive;   

            await _context.SaveChangesAsync();

            return "Role updated successfully";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
                return "Role not found";

            role.IsActive = false;

            await _context.SaveChangesAsync();

            return "Role deleted successfully";
        }
    }
}
