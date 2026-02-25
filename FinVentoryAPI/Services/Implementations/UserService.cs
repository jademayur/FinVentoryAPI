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

        public async Task<UserResponseDto> CreateAsync(UserCreateDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                throw new Exception("Email already exists");

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

            return MapToResponse(user);
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _context.Users
                .Where(x => x.IsActive)
                .Select(x => new UserResponseDto
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    Mobile = x.Mobile,
                    IsPlatformAdmin = x.IsPlatformAdmin,
                    IsActive = x.IsActive,
                    
                })
                .ToListAsync();
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id && x.IsActive);

            if (user == null)
                return null;

            return MapToResponse(user);
        }

        public async Task<bool> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id && x.IsActive);

            if (user == null)
                return false;

            user.FullName = dto.FullName;
            user.Mobile = dto.Mobile;
            user.IsPlatformAdmin = dto.IsPlatformAdmin;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == id && x.IsActive);

            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private UserResponseDto MapToResponse(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                IsPlatformAdmin = user.IsPlatformAdmin,
                IsActive = user.IsActive,
               
            };
        }
    }
}