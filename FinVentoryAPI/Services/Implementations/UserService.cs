using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.UserDTOs;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(x => x.IsActive)
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id && x.IsActive);
        }

        public async Task<string> CreateAsync(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                return "Email already exists";

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Mobile = dto.Mobile,
                IsPlatformAdmin = dto.IsPlatformAdmin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "User created successfully";
        }

        public async Task<string> UpdateAsync(UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);

            if (user == null)
                return "User not found";

            user.FullName = dto.FullName;
            user.Mobile = dto.Mobile;
            user.IsPlatformAdmin = dto.IsPlatformAdmin;

            await _context.SaveChangesAsync();

            return "User updated successfully";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return "User not found";

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return "User deleted successfully";
        }
    }
}
