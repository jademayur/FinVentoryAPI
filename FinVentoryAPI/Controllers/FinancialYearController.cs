using FinVentoryAPI.DTOs.FinancialYearDTOs;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinVentoryAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialYearController : ControllerBase
    {
        private readonly IFinancialYearService _service;

        public FinancialYearController(IFinancialYearService service)
        {
            _service = service;
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompany(int companyId)
        {
            var result = await _service.GetByCompanyAsync(companyId);

            if (result == null || !result.Any())
                return NotFound(new { message = "No financial years found" });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFinancialYearDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _service.CreateAsync(dto);

            if (message.Contains("already"))
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateFinancialYearDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = await _service.UpdateAsync(dto);

            if (message == "Record not found")
                return NotFound(new { message });

            if (message.Contains("cannot") || message.Contains("already"))
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPut("close/{id}")]
        public async Task<IActionResult> Close(int id)
        {
            var message = await _service.CloseAsync(id);

            if (message == "Record not found")
                return NotFound(new { message });

            return Ok(new { message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _service.DeleteAsync(id);

            if (message == "Record not found")
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}