using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IWebHostEnvironment _env;

        public CompanyController(ICompanyService companyService, IWebHostEnvironment env)
        {
            _companyService = companyService;
            _env = env;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated.");
            return Convert.ToInt32(userIdClaim);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var result = await _companyService.CreateCompanyAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.CompanyId }, result);
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
                return NotFound(new { message = "Company not found" });

            return Ok(company);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var updated = await _companyService.UpdateCompanyAsync(id, dto, userId);

            if (!updated)
                return NotFound(new { message = "Company not found" });

            return Ok(new { message = "Company updated successfully" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var deleted = await _companyService.DeleteCompanyAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Company not found" });

            return Ok(new { message = "Company deleted successfully" });
        }

        [HttpGet("state")]
        public async Task<IActionResult> GetCompanyState()
        {
            try
            {
                var result = await _companyService.GetCompanyStateAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/logo")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile file)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(new { message = "Company not found" });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Only image files are allowed (jpg, png, gif, webp, svg)" });

            var logosDir = Path.Combine(_env.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(logosDir);

            var fileName = $"{id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(logosDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Delete old logo if exists
            await _companyService.UpdateLogoAsync(id, $"/uploads/logos/{fileName}");

            return Ok(new { logo = $"/uploads/logos/{fileName}" });
        }

        [HttpDelete("{id}/logo")]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound(new { message = "Company not found" });

            if (!string.IsNullOrEmpty(company.Logo))
            {
                var oldPath = Path.Combine(_env.WebRootPath, company.Logo.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            await _companyService.UpdateLogoAsync(id, null);
            return Ok(new { message = "Logo deleted" });
        }
    }
}
