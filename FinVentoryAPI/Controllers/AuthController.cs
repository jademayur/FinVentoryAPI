using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.DTOs.CompanySelectionDtos;
using FinVentoryAPI.DTOs.LoginDtos;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly ICompanyService _companyService;
        private readonly AppDbContext _context;

        public AuthController(IAuthService service, ICompanyService companyService, AppDbContext context)
        {
            _service = service;
            _companyService = companyService;
            _context = context;
        }

        // Phase 1: Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _service.LoginAsync(dto);

            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        // Phase 2: Select Company — AllowAnonymous because login doesn't return a token yet
        [AllowAnonymous]
        [HttpPost("select-company")]
        public async Task<IActionResult> SelectCompany([FromBody] CompanySelectionDto dto)
        {
            // Validate user exists and has access to this company
            var hasAccess = await _context.UserCompany
                .AnyAsync(x => x.UserId == dto.UserId
                    && x.CompanyId == dto.CompanyId
                    && x.FinancialYearId == dto.FinancialYearId
                    && x.IsActive);
            if (!hasAccess)
                return Unauthorized("Invalid company selection");

            var token = await _service.GenerateTokenAsync(dto);

            if (token == null)
                return Unauthorized("Invalid company selection");

            return Ok(new { token });
        }

        // Create company — available when no company exists yet (no token yet)
        [AllowAnonymous]
        [HttpPost("create-company")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            // Validate user exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.UserId && u.IsActive);
            if (user == null)
                return BadRequest("Invalid user.");

            // Only allow if no companies exist in the system
            if (await _context.Companies.AnyAsync(c => c.IsActive))
                return BadRequest("Companies already exist. Please login and select a company.");

            var company = await _companyService.CreateCompanyAsync(dto.Company, dto.UserId);

            // Get the financial year that was seeded for this company
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(f => f.CompanyId == company.CompanyId && f.IsActive);

            if (financialYear == null)
                return BadRequest("Company created but no financial year found");

            // Generate token for the user
            var selectionDto = new CompanySelectionDto
            {
                UserId = dto.UserId,
                CompanyId = company.CompanyId,
                FinancialYearId = financialYear.FinancialYearId
            };

            var token = await _service.GenerateTokenAsync(selectionDto);

            if (token == null)
                return BadRequest("Company created but failed to generate token");

            return Ok(new { token, companyId = company.CompanyId, companyName = company.CompanyName, financialYearId = financialYear.FinancialYearId });
        }

        // Check setup status — returns what step the user is on
        [Authorize]
        [HttpGet("setup-status")]
        public async Task<IActionResult> GetSetupStatus()
        {
            var jwtUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var hasCompany = await _context.Companies.AnyAsync(c => c.IsActive);

            if (!hasCompany)
                return Ok(new { step = "company", hasCompany = false, hasFinancialYear = false });

            var userCompanyIds = await _context.UserCompany
                .Where(x => x.UserId == jwtUserId && x.IsActive && x.Company != null && x.Company.IsActive)
                .Select(x => x.CompanyId)
                .ToListAsync();

            var hasFinancialYear = await _context.FinancialYears
                .AnyAsync(f => userCompanyIds.Contains(f.CompanyId) && f.IsActive);

            if (!hasFinancialYear)
                return Ok(new { step = "financialyear", hasCompany = true, hasFinancialYear = false });

            return Ok(new { step = "ready", hasCompany = true, hasFinancialYear = true });
        }

        // Create financial year for a company — AllowAnonymous since no token yet
        [AllowAnonymous]
        [HttpPost("create-financial-year")]
        public async Task<IActionResult> CreateFinancialYear([FromBody] AuthSetupFinancialYearDto dto)
        {
            // Validate company exists
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == dto.CompanyId && c.IsActive);
            if (company == null)
                return BadRequest("Invalid company.");

            var fy = new FinancialYear
            {
                CompanyId = dto.CompanyId,
                YearName = dto.YearName,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                IsClosed = false,
                CreatedBy = 0
            };

            _context.FinancialYears.Add(fy);
            await _context.SaveChangesAsync();

            return Ok(new { financialYearId = fy.FinancialYearId, yearName = fy.YearName });
        }
    }

    public class CreateCompanyDto
    {
        public int UserId { get; set; }
        public CompanyCreateDto Company { get; set; }
    }

    public class AuthSetupFinancialYearDto
    {
        public int CompanyId { get; set; }
        public string YearName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
