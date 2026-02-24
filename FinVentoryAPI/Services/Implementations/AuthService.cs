using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CompanySelectionDtos;
using FinVentoryAPI.DTOs.LoginDtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FinVentoryAPI.Services.Interfaces;

namespace FinVentoryAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<object> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email && x.IsActive);

            if (user == null)
                return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            // If Platform Admin
            if (user.IsPlatformAdmin)
            {
                return new
                {
                    userId = user.UserId,
                    isPlatformAdmin = true,
                    companies = new List<object>(),
                    financialYears = new List<object>(),
                };
            }

            // Get assigned companies
            var companies = await _context.UserCompany
                .Where(x => x.UserId == user.UserId )
                .Include(x => x.Company)
                .Select(x => new
                {
                    x.CompanyId,
                    x.Company.CompanyName
                })
                .ToListAsync();

            //get assigned years
            var years = await _context.UserCompany
                .Where(x => x.UserId == user.UserId)
                .Include(x => x.FinancialYear)
                .Select(x => new
                {
                    x.FinancialYearId,
                    x.FinancialYear.YearName
                })
                .ToListAsync();



            return new
            {
                userId = user.UserId,
                isPlatformAdmin = false,
                companies,
                years
            };
        }

        public async Task<string> GenerateTokenAsync(CompanySelectionDto dto)
        {
            var mapping = await _context.UserCompany
                .Include(x => x.Role)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.UserId == dto.UserId &&
                    x.CompanyId == dto.CompanyId  &&
                    x.FinancialYearId == dto.FinancialYearId
                    );

            if (mapping == null)
                return null;

            var claims = new List<Claim>
    {
        new Claim("UserId", mapping.UserId.ToString()),
        new Claim("CompanyId", mapping.CompanyId.ToString()),
        new Claim("FinancialYearId", mapping.FinancialYearId.ToString()),
        new Claim(ClaimTypes.Name, mapping.User.FullName),
        new Claim(ClaimTypes.Email, mapping.User.Email),
        new Claim(ClaimTypes.Role, mapping.Role.RoleName),
        new Claim("IsPlatformAdmin", mapping.User.IsPlatformAdmin.ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    Convert.ToDouble(_configuration["Jwt:DurationInHours"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
