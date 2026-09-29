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

            // Check if any company exists in the system at all
            var hasAnyCompany = await _context.Companies.AnyAsync(c => c.IsActive);

            if (!hasAnyCompany)
            {
                return new
                {
                    userId = user.UserId,
                    roleId = user.RoleId,
                    needsCompanySetup = true,
                    companies = new List<object>()
                };
            }

            // User has companies — load them (include companies even if no FY exists yet)
            var userCompanyRecords = await _context.UserCompany
                .Where(x => x.UserId == user.UserId && x.IsActive
                    && x.Company != null && x.Company.IsActive)
                .Include(x => x.Company)
                .Include(x => x.FinancialYear)
                .ToListAsync();

            // Separate: companies that have at least one FY (for UserCompany mapping)
            var companiesWithFY = userCompanyRecords
                .Where(x => x.FinancialYear != null && x.FinancialYear.IsActive)
                .ToList();

            // All companies the user has access to
            var allCompanies = userCompanyRecords
                .GroupBy(x => new { x.CompanyId, x.Company.CompanyName })
                .Select(g => new
                {
                    companyId = g.Key.CompanyId,
                    companyName = g.Key.CompanyName,
                    years = g.Where(x => x.FinancialYear != null && x.FinancialYear.IsActive)
                             .Select(y => new
                             {
                                 financialYearId = y.FinancialYearId,
                                 yearName = y.FinancialYear!.YearName
                             }).ToList()
                })
                .ToList();

            return new
            {
                userId = user.UserId,
                roleId = user.RoleId,
                needsCompanySetup = false,
                companies = allCompanies
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
                    x.FinancialYearId == dto.FinancialYearId &&
                    x.IsActive
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
                new Claim("RoleId", mapping.RoleId.ToString()),
                new Claim(ClaimTypes.Role, mapping.Role.RoleName),
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
