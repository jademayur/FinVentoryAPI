using FinVentoryAPI.DTOs.CompanyDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = 1; // Later get from JWT claim

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

            int userId = 1; // Later get from JWT

            var updated = await _companyService.UpdateCompanyAsync(id, dto, userId);

            if (!updated)
                return NotFound(new { message = "Company not found" });

            return Ok(new { message = "Company updated successfully" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            int userId = 1; // Later get from JWT

            var deleted = await _companyService.DeleteCompanyAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = "Company not found" });

            return Ok(new { message = "Company deleted successfully" });
        }
    }
}
